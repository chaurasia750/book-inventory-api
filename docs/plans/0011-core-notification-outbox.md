# Plan 0011: core - notification outbox (templates, recorded emails, worker, SMTP)

- **Number:** 0011 (global serial; it must be built **before** Plan 0005, which depends on it)
- **Module:** `core` (primary) ; also touches: `platform` (two packages in `Directory.Packages.props`)
- **Spec:** [docs/specs/core.md](../specs/core.md) - N1-N14, NAC1-NAC9
- **Status:** Done
- **Created:** 2026-09-27   **Completed:** 2026-09-27

## 1. Goal

Any module can record an email with `INotificationService.QueueAsync`; a background worker renders it from a database template inside the master layout, sends it (SMTP through MailKit, or the logging sender when no host is configured), retries with back-off, writes one log row per attempt and redacts secrets after delivery. Nothing is exposed over HTTP.

## 2. Out of scope

HTTP endpoints and admin screens for notifications/templates, SMS/WhatsApp/push senders, attachments, unsubscribe, choosing the production provider (CO1), the iam templates (Plan 0005 ships `PASSWORD_RESET`).

## 3. Approach

Modelled on the owner's sample (`Binsera` notification module): outbox table + template table + log table, Scriban templates with a master layout, background worker, retry policy, MailKit sender. Changes against the sample, on purpose: aggregate roots with behaviour (no public setters, transitions enforced inside `Notification`), UUID v7 keys, idempotency key with a unique index, secrets redacted after delivery (N9; the sample stores a welcome password in clear), one sender port instead of a factory of channels (only email now), the worker claims rows through status + `xmin` instead of loading everything, and the logging sender for development (N12).

**Domain (`BookInventory.Domain/Core`):**
- Enums `NotificationChannel`, `NotificationPriority`, `NotificationStatus` (stored UPPER_SNAKE by the platform convention).
- `Notification : AggregateRoot` with `Queue(...)`, `StartProcessing(now)`, `MarkSent(providerMessageId, response, now)`, `MarkFailed(error, now, retryDelays)` (retry or `FAILED`), `Cancel()`, `Requeue()` (from `FAILED`), `Redact()`; child `NotificationLog : TransactionDomainBase` added by the root when an attempt ends (`RecordAttempt`). Message codes `core.notification.*` in `CoreMessages`.
- `NotificationTemplate : AggregateRoot` with `Create(...)` and `Update(subject, body, isActive)`; code normalised upper-case.
- Parameter objects (records) so no method has more than four parameters (CLAUDE.md).

**Application (`BookInventory.Application/Core`):**
- `INotificationService` / `NotificationService` (queue with idempotency and recipient validation, cancel, requeue), `QueueNotification` record.
- `INotificationSender` port: `SendAsync(EmailMessage, ct) -> SendResult`; `IEmailComposer` (loads content + master layout, renders with `ITemplateRenderer`), `ITemplateRenderer` port (Scriban implementation in Infrastructure), `NotificationDispatcher` (one batch: claim, compose, send, record, redact, schedule retry).
- `NotificationTemplateSeeder` collects `INotificationTemplateSource` contributions from any module and inserts the missing codes (N13); `core` contributes `MASTER_EMAIL_LAYOUT`.
- `NotificationOptions`, `EmailOptions`, `MasterLayoutOptions` bound with `AddValidatedOptions` (the nested-validation rule from Plan 0003 applies).

**Infrastructure:**
- EF configurations (schema `core`), unique indexes on `notification_template(code)` and `notification(idempotency_key)`, indexes for the worker queries, migration `Core_Notification` (SQL shown first; applied only to the local `book_inventory` database).
- `ScribanTemplateRenderer`, `MailKitEmailSender`, `LoggingEmailSender` (selected by `Email:Host`), masked-address helper.

**Api (host):** `NotificationWorker : BackgroundService` (interval and batch size from options; every tick in its own scope, errors logged and never fatal), registration of the sender by configuration, startup warning when the logging sender is active, template seeding hosted service (next to the existing seeder).

**Packages (approved 2026-09-27):** `MailKit`, `Scriban` in `Directory.Packages.props` (Application/Infrastructure references only).

## 4. Tasks

- [x] 1. Domain: enums, `Notification` (+ log), `NotificationTemplate`, records, `CoreMessages`
- [x] 2. Infrastructure: EF configurations, indexes, migration `Core_Notification` (show the SQL, apply to local `book_inventory`)
- [x] 3. Options with startup validation (`Notification`, `Email`, `MasterLayout`); packages added
- [x] 4. Application: `INotificationService`, composer + Scriban renderer, sender port with MailKit and logging senders
- [x] 5. Dispatcher + `NotificationWorker` + retry/redaction; template seeder and `MASTER_EMAIL_LAYOUT`
- [x] 6. Message catalogue entries for `core.notification.*`
- [x] 7. Verify by hand (section 5); set the spec to Approved/current and link this plan

## 5. Verification

Run on 2026-09-27. Build: `dotnet build src/BookInventory.Api` (all referenced projects) 0 warnings, 0 errors (the normal solution build is blocked while a dev API holds the DLLs, so it was built to a separate output folder and the migration with `--configuration Release`). Scratch apps under `%TEMP%\opencode\corecheck` (real PostgreSQL, throw-away database created and dropped by the check, a mutable clock, a scripted sender) and the session scratchpad script `api-check5.sh` (real host); none is committed.

**Migration** `20260926184457_Core_Notification`: generated SQL shown before applying: schema `core`; tables `notification`, `notification_template`, `notification_log`; `uuid` keys, `timestamptz`, enum CHECKs (`channel`, `priority`, `status`), FK `notification_log -> notification` RESTRICT, unique indexes on `notification(idempotency_key)` and `notification_template(code)`, worker indexes `(status, next_retry_on)` and `(status, schedule_on)`. Additive only. Applied to the local `book_inventory` (target confirmed `Database=book_inventory`); `__ef_migrations_history` lists it and the three `core` tables exist.

**Scenario run** (`corecheck`, final line `==== 40 passed, 0 failed ====`):

| Rule | Input | Observed |
| --- | --- | --- |
| N13 | seeder with the core master layout plus two test templates; run twice | adds only the missing codes; the second run adds nothing (3 templates) |
| NAC1 | queue inside a transaction, roll it back | no row for the key |
| NAC2 | queue twice with one idempotency key | the same id twice |
| N10, N2 | recipient `not-an-email`; unknown template code | `core.notification.invalid_recipient`; `core.notification.template_not_found` |
| NAC3 | real logging sender (no `Email:Host`), `LogBody` on | PENDING -> SENT, one log row with response `logged only, no provider configured`; the log shows the email inside the master layout; address masked (`r***@user.test`); with `LogBody` off no body is printed |
| N3 | variable `Asha <b>Verma</b>`, link `?t=1&u=2` | body has `Asha &lt;b&gt;Verma&lt;/b&gt;` and `t=1&amp;u=2`; the subject keeps the plain text |
| N11, N12 | sender resolved with and without a host | `MailKitEmailSender` with a host, `LoggingEmailSender` without |
| NAC6 | sensitive template after SENT; normal template after SENT | variables `[redacted]`; the normal one keeps its JSON |
| NAC4 | sender that always fails, delays 1 min and 2 min, clock advanced | RETRY at +60 s, not due before, due after; RETRY at +120 s; then FAILED; log attempts 1, 2, 3 |
| NAC7 | cancel FAILED; requeue FAILED; cancel PENDING; cancel SENT; requeue a redacted sensitive one | `invalid_transition`; back to PENDING; CANCELLED; `invalid_transition`; `cannot_requeue_redacted` |
| N9 | sensitive notification that used up its retries | FAILED and `[redacted]` |
| NAC5 | template switched off, then a second notification in the same batch | first: RETRY with error `Template PLAIN_NOTE was not found or is switched off.`; second: SENT |
| NAC9 | 12 notifications, each processed by two workers at once | 12 sends in total (never twice), 12 of 24 attempts processed, exactly one log row per notification |
| N5 | a PROCESSING row that is fresh; the same row 11 minutes later | not due; due again (stuck recovery) |
| NAC8 | `EmailOptions`/`NotificationOptions` validated | empty host valid; host with port 0, without from address, with user but no password invalid; complete valid; empty or zero retry list invalid |

**Real host** (`api-check5.sh`, throw-away database): `Email__Host=smtp.test Email__Port=0` stops startup with `OptionsValidationException ... Email:Port must be between 1 and 65535`. A normal start logs `Seeded 1 default notification template(s): MASTER_EMAIL_LAYOUT`; a row recorded by SQL is picked up by the worker within the interval: `SENT`, one `notification_log` row `logged only, no provider configured`, log line `No email provider is configured; email to s***@user.test was logged only.`.

**Defects found and fixed while verifying:** a second worker that loaded a row after the first had claimed it could re-claim it (the claim is now re-checked with the same due rule, `Notification.IsDueAt`); HTML bodies inserted variable values unencoded (now encoded); `BookInventory.Infrastructure` cannot reference the hosting abstractions, so the development-only body logging is a setting (`Email:LogBody`) and not an environment check.

**NOT verified:** a real SMTP server (no provider yet, CO1) - the MailKit sender is selected and configured but never connected; two separate API processes running the worker at once (the two-worker race was run inside one process with separate scopes and connections); the release checklist entry for `Email:Host` is in Plan 0010's text.

## 6. Risks and notes

- The logging sender must never be the production wiring (N12); Plan 0010's release checklist lists it.
- One worker instance is assumed (single-instance deployment); the claim saved before sending plus `xmin` keeps two accidental workers from double-sending (NAC9, verified in one process), which is not a scaling design. A worker that dies mid-send can make a mail go out twice after `StuckAfterMinutes` (at-least-once delivery).
- `NotificationTemplate.Update` exists on the aggregate but nothing calls it yet: editing templates is CO3 (a later plan).
- Redaction (N9) protects the database at rest, not the mailbox: a reset link is still delivered by email, as accepted in the iam spec.
