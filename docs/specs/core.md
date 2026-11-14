# Spec: core - Notifications (outgoing email) and shared system services

- **Module:** `core` (matches the PostgreSQL schema `core` in `docs/BookInventory_ERD.md`)
- **Status:** Approved
- **Last updated:** 2026-09-27
- **Source docs:** [ERD §11 core](../BookInventory_ERD.md), [specs/iam.md R17, R56](iam.md), [specs/platform.md](platform.md), `CLAUDE.md`; behaviour modelled on the owner's sample project (`Rudrakshama` / `Binsera` notification module) with the changes listed in the rules below
- **Plans:** [0011 notification outbox](../plans/0011-core-notification-outbox.md) (done)

> This file starts with the **notification** capability. The ERD's audit log and system settings are added to this spec when their plans exist.

## 1. Purpose and scope

No module sends an email directly. A module that wants to tell somebody something **records a notification** (what, to whom, from which template, with which values); the record is saved in the same transaction as the business change, and a background worker sends it later, retries failures and keeps a log of every attempt. That gives one place to see every email the system ever tried to send, safe retries, and a provider (SMTP today, another tomorrow) that can change without touching any module.

Not in scope now: SMS/WhatsApp/push channels (the model has a channel column so they can be added), attachments, an admin screen or HTTP endpoints for templates and notifications (a later plan), user preferences/unsubscribe, bulk campaigns.

## 2. Concepts and glossary

- **Notification** (aggregate root): one message to one recipient on one channel (`EMAIL` now). Owns its **attempt log** (child, insert-only).
- **NotificationTemplate** (aggregate root): a named, editable text with `{{ variables }}` (Scriban), identified by a stable `code` (`PASSWORD_RESET`). Modules ship default templates; the database copy is what is used, so wording can change without a release.
- **Master layout**: one template (`MASTER_EMAIL_LAYOUT`) that wraps every HTML email (logo, company name, footer from `MasterLayout:*` configuration); the content template is rendered first and placed in `{{ content }}`.
- **Outbox**: the notification table itself. "Recorded" and "sent" are separate steps.

Status lifecycle of a notification:

```
PENDING --worker takes it--> PROCESSING --ok--> SENT
                                  |--failed, attempts left--> RETRY --due--> PROCESSING
                                  |--failed, no attempts left--> FAILED
PENDING | RETRY --cancel--> CANCELLED         FAILED --manual retry--> PENDING
```

## 3. Business rules

- **N1** Every outgoing email is first recorded as a `PENDING` notification, in the **same transaction** as the change that caused it. Nothing is sent inside a request. If the business change rolls back, no notification exists.
- **N2** A caller supplies the template `code`, the recipient address, a dictionary of variables and an **idempotency key**. The same idempotency key never creates a second notification (a repeated request, a retried job); the existing one is returned.
- **N3** The subject and body are rendered from the template when the worker sends, with the variables stored on the notification. A template that is missing, inactive or fails to render fails that attempt with a clear error and never stops the worker. Values are HTML-encoded in an HTML body (a name such as `A<b>` never becomes markup); the subject stays plain text.
- **N4** HTML emails are wrapped in the master layout when it exists; without it the content template is sent as is (a warning is logged once).
- **N5** The worker runs every `Notification:WorkerIntervalSeconds` (default 30), takes notifications that are `PENDING` or `RETRY` and due (`schedule_on` / `next_retry_on` not in the future), oldest first, in batches of at most `Notification:BatchSize` (default 50). A notification is handled by exactly one worker at a time: the claim (`PROCESSING`) is saved before anything is sent and the optimistic-concurrency token lets only one worker win. A row left in `PROCESSING` by a worker that died is picked up again after `Notification:StuckAfterMinutes` (default 10).
- **N6** Every attempt writes one `notification_log` row (attempt number, outcome, provider response or error text, provider message id). Log rows are never edited or deleted.
- **N7** A failed attempt schedules the next one by the back-off in `Notification:RetryMinutes` (default `5, 15, 30, 60, 360`); when the list is used up, or `max_retry` is reached, the notification becomes `FAILED`. A Super Admin can move a `FAILED` notification back to `PENDING` (application capability, no endpoint yet).
- **N8** A notification can be cancelled while it is `PENDING` or `RETRY`; a `SENT` one cannot. Every status change follows the diagram above, enforced inside the aggregate; anything else is a 409.
- **N9** **Sensitive templates** (reset links, temporary passwords) are flagged `is_sensitive`. As soon as such a notification is `SENT`, `FAILED` or `CANCELLED`, its stored variables are replaced by `[redacted]`; the record (recipient, template, times, status, log) stays. A secret therefore lives in the database only until it is delivered or given up (owner decision, 2026-09-27). The rendered text is never stored at all; it exists only while the attempt runs.
- **N10** Recipient addresses are validated when the notification is recorded (format, length). In application logs the address is masked (`a***@example.com`); the log never contains variables or a rendered body, except the development sender (N12).
- **N11** The SMTP provider is configured under `Email:*` (host, port, TLS mode, user, password from a secret store, from address, display name, timeout). Settings are validated at startup when a host is configured. The password is never logged.
- **N12** When `Email:Host` is empty the **logging sender** is used: the notification goes through the same steps and is marked `SENT` with the response `logged only, no provider configured`; only when `Email:LogBody` is true (set in the Development settings, never elsewhere) the rendered email is written to the log so a developer can follow a reset link. A warning at startup says that no real email is sent. This is a development setting and is listed on the release checklist (Plan 0010).
- **N13** Default templates are owned by the module that sends them (iam owns `PASSWORD_RESET`); at startup each is inserted **only if its code does not exist yet**, so edited templates are never overwritten. `MASTER_EMAIL_LAYOUT` is owned by `core`.
- **N14** Timestamps are UTC. Ids are UUID v7. Enum values are stored as UPPER_SNAKE text (platform rules P1-P8).

## 4. Data

Schema `core` (ERD §11 conventions). Names follow the platform rules (singular snake_case).

| Table | Columns (besides id and audit) | Notes |
| --- | --- | --- |
| `notification_template` | `code` (unique, max 100), `name`, `channel`, `subject`, `body`, `is_html`, `is_sensitive`, `language_code` (default `en`), `is_active`, `variables_hint` (text, documents the expected variables) | root |
| `notification` | `template_code`, `channel`, `priority`, `recipient` (max 254), `variables` (text holding JSON, or `[redacted]` per N9), `status`, `retry_count`, `max_retry`, `next_retry_on`, `schedule_on`, `sent_on`, `provider_message_id`, `error_message`, `correlation_id`, `idempotency_key` (unique) | root; indexes on `(status, next_retry_on)` and `(status, schedule_on)` |
| `notification_log` | `notification_id` (FK, restrict), `attempt_no`, `status`, `response`, `error_message`, `provider_message_id` | child, insert-only |

Enums: `channel` = `EMAIL` (`SMS`, `WHATS_APP`, `PUSH` reserved); `priority` = `HIGH`, `NORMAL`, `LOW`; `status` = `PENDING`, `PROCESSING`, `SENT`, `RETRY`, `FAILED`, `CANCELLED`.

Configuration: `Notification:*` (worker interval, batch size, retry minutes, default max retry), `Email:*` (N11), `MasterLayout:*` (company name, logo URL, website, address, support email and phone).

## 5. API contract

No HTTP endpoints in this spec version. Other modules use one application interface:

`INotificationService.QueueAsync(QueueNotification request, CancellationToken)` where `QueueNotification(string TemplateCode, string Recipient, IReadOnlyDictionary<string,string> Variables, string IdempotencyKey, string? CorrelationId = null)` returns the notification id and status. Errors: unknown or inactive template -> 404 `core.notification.template_not_found`; invalid recipient -> 400 `core.notification.invalid_recipient`.

## 6. Events

None raised now. (A later `NotificationFailed` event may feed an admin alert.)

## 7. Dependencies

`core` depends on no business module. `iam` (Plan 0005), and later `mlm` and `finance`, depend on `core`'s `INotificationService`. Packages approved by the owner (2026-09-27): **MailKit** (SMTP) and **Scriban** (templates).

## 8. Acceptance criteria

- **NAC1** Queueing a notification and then rolling back the surrounding transaction leaves no row (N1).
- **NAC2** Queueing twice with the same idempotency key creates one row and returns the same id (N2).
- **NAC3** With the logging sender the worker turns a `PENDING` row into `SENT` with one log row, the Development log shows the rendered email inside the master layout, and a non-Development run does not print the body (N3, N4, N6, N12).
- **NAC4** A sender that fails moves the row to `RETRY` with the right `next_retry_on`, and after the last attempt to `FAILED`, one log row per attempt (N6, N7).
- **NAC5** A missing or inactive template fails the attempt with an error text and the worker keeps processing the next notification (N3).
- **NAC6** A sensitive notification has `[redacted]` variables after `SENT`; a normal one keeps them (N9).
- **NAC7** Cancel works on `PENDING`/`RETRY` and is refused on `SENT`; every other invalid transition is refused (N8).
- **NAC8** A configured but invalid `Email:*` (port 0, missing from address) stops startup; an empty host does not (N11, N12).
- **NAC9** Two workers started together never send the same notification twice (N5).

## 9. Open questions

- **CO1** Which SMTP/API provider is used in production, and the real `Email:*` values (secret store). Development uses the logging sender until then.
- **CO2** Whether SMS/WhatsApp are wanted in v1 (the model allows them; no plan yet).
- **CO3** An admin screen to view notifications, edit templates and retry failed ones (a later plan; needs role rules).
