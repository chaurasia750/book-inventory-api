# Spec: core - Notifications (outgoing email) and shared system services

- **Module:** `core` (matches the PostgreSQL schema `core`)
- **Status:** the notification part (N1-N14) was carried over from the base project (HautInHousing) and describes the code as it is; the audit trail (A1-A10, AAC1-AAC10, AD1-AD4, AQ1-AQ5 recommended answers) was approved by the owner on 2026-11-14 and is being built (plans core#01, core#02).
- **Last updated:** 2026-11-14
- **Plans:** [core#01](../plans/core/2026-11-14-01-audit-trail.md), [core#02](../plans/core/2026-11-14-02-ui-audit-screen.md) (both Approved 2026-11-14)
- **Source:** the Book Inventory business documents, see [docs index](../README.md) (none of the rules below were derived from them yet).

> This file holds the **notification** capability and the **audit trail** (Draft). System settings are added to this spec when their plans exist.

## Base-project rules under review (Book Inventory)

- **Old references in this file:** numbers of removed plans (0005, 0010, 0011), the ERD (never in this repository), and the sample projects the notification design was modelled on. These are history only: do not look for them and do not build from them.
- **How to work with the rules below:** rules not listed in the table describe the code as it is and stay valid. A listed rule also describes current code, but it belongs to the base project's business: do not extend it or build on it. A Book Inventory feature that touches it changes or retires it through the spec flow, and its row is removed then.

| Rules | What is from the base project | For Book Inventory |
| --- | --- | --- |
| Dependency sentence in section on dependencies (`mlm` and `finance` depend on `core`) | Names `mlm`, which is not part of Book Inventory | `finance` and later modules may still use `INotificationService`; the sentence is corrected when a feature touches it |
| ERD references (header note, schema section) | ERD sections of the base project | No ERD in the repository; the tables are described by the code |

## 1. Purpose and scope

No module sends an email directly. A module that wants to tell somebody something **records a notification** (what, to whom, from which template, with which values); the record is saved in the same transaction as the business change, and a background worker sends it later, retries failures and keeps a log of every attempt. That gives one place to see every email the system ever tried to send, safe retries, and a provider (SMTP today, another tomorrow) that can change without touching any module.

Feature **Audit trail** (A1-A10, approved 2026-11-14): every change of an audited row is recorded (who, when, what, in which company) and can be read, never changed, by the Super Admin (all companies) and by an Owner (their own company) - BRD FR-USR-04 and NFR-08.

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

**Audit trail (Approved 2026-11-14)**
- **A1** Every create, change and delete of an audited row (AQ1) writes one **audit entry** in the same transaction as the change: entity type, entity id, action (`CREATED`, `UPDATED`, `DELETED`), old value, new value, who, when (UTC) and the company of the row (BRD FR-USR-04, NFR-08, section 8 `AuditLog`). A change that is rolled back leaves no entry.
- **A2** A created row has only a new value and a deleted row only an old value; an update has both, limited to the fields that changed. Values are JSON text keyed by column name.
- **A3** Secrets are never recorded: password hashes, refresh and reset token hashes, temporary passwords and any column its module marks as secret. The entry of a password change shows that the account changed, with no password field.
- **A4** The actor is the user who made the change (id and user name as they were then). A change made by a background job or by the hidden system account has no actor id and is shown as "System" (iam R48).
- **A5** The entry's company is the company of the changed row: the company itself for a company, the account's company for an Owner or Staff account, the row's company for any company-owned row; none for Super Admin accounts and other platform rows.
- **A6** An entry is never edited or deleted: there is no endpoint, screen or job that changes one.
- **A7** A Super Admin reads every entry and may filter by company. An Owner reads only the entries of their own company; the company comes from the session ([specs/company.md](company.md) R6), never from the request, so asking for another company's id still answers the Owner's own. Staff and every other role are refused (`403`).
- **A8** The list is paged (page size capped at 200), newest first, and filters by entity type, entity id, actor, company (Super Admin only) and a date range read in the business time zone ([specs/company.md](company.md) D7). A second call lists the entity types that exist in the log, for the filter.
- **A9** One audit screen serves the Super Admin and the Owner: the list with its filters, and a detail of the old and new value of an entry. It shows loading, empty, error (with retry) and success.
- **A10** Entries are kept and never removed in v1 (AD2); the table is built so it can be split by time later.

## 4. Data

Schema `core` (ERD §11 conventions). Names follow the platform rules (singular snake_case).

| Table | Columns (besides id and audit) | Notes |
| --- | --- | --- |
| `notification_template` | `code` (unique, max 100), `name`, `channel`, `subject`, `body`, `is_html`, `is_sensitive`, `language_code` (default `en`), `is_active`, `variables_hint` (text, documents the expected variables) | root |
| `notification` | `template_code`, `channel`, `priority`, `recipient` (max 254), `variables` (text holding JSON, or `[redacted]` per N9), `status`, `retry_count`, `max_retry`, `next_retry_on`, `schedule_on`, `sent_on`, `provider_message_id`, `error_message`, `correlation_id`, `idempotency_key` (unique) | root; indexes on `(status, next_retry_on)` and `(status, schedule_on)` |
| `notification_log` | `notification_id` (FK, restrict), `attempt_no`, `status`, `response`, `error_message`, `provider_message_id` | child, insert-only |
| `audit_entry` | `entity_type` (max 100), `entity_id`, `action`, `old_value` and `new_value` (JSON text, A2), `actor_id` (nullable), `actor_name` (nullable, a snapshot, AD3), `is_system`, `company_id` (nullable, a plain id, A5) | insert-only (A6); indexes `(company_id, created_at desc)`, `(entity_type, entity_id, created_at desc)`, `(actor_id, created_at desc)`; the time of the change is `created_at` |

Enums: `action` (audit) = `CREATED`, `UPDATED`, `DELETED`; `channel` = `EMAIL` (`SMS`, `WHATS_APP`, `PUSH` reserved); `priority` = `HIGH`, `NORMAL`, `LOW`; `status` = `PENDING`, `PROCESSING`, `SENT`, `RETRY`, `FAILED`, `CANCELLED`.

Configuration: `Notification:*` (worker interval, batch size, retry minutes, default max retry), `Email:*` (N11), `MasterLayout:*` (company name, logo URL, website, address, support email and phone).

## 5. API contract

Audit trail (`/api/v1`, read only, A6):

| Method + route | Who | Notes |
|---|---|---|
| `GET audit` | `SUPER_ADMIN`, `OWNER` | filters `entityType`, `entityId`, `userId`, `companyId` (Super Admin only; an Owner's is ignored), `from`, `to`, `pageIndex`, `pageSize` (cap 200) -> paged entries `{id, createdAt, actor {userId, name, isSystem}, entityType, entityId, action, oldValue, newValue}` newest first (A7, A8) |
| `GET audit/entity-types` | `SUPER_ADMIN`, `OWNER` | the entity types present in the entries the caller may read (A8) |

There is no endpoint for notifications or templates in this spec version. Other modules use one application interface:

`INotificationService.QueueAsync(QueueNotification request, CancellationToken)` where `QueueNotification(string TemplateCode, string Recipient, IReadOnlyDictionary<string,string> Variables, string IdempotencyKey, string? CorrelationId = null)` returns the notification id and status. Errors: unknown or inactive template -> 404 `core.notification.template_not_found`; invalid recipient -> 400 `core.notification.invalid_recipient`.

## 6. Events

None raised now. The audit entry is written by the persistence layer when the change is saved, not by an event (AD1). (A later `NotificationFailed` event may feed an admin alert.)

## 7. Dependencies

`core` depends on no business module; the audit trail takes the actor and the company from the current session through ports (`ICurrentUser`, `ICurrentCompany`) and never reads `iam` or `company` tables (AD3). `iam` (Plan 0005), and later `mlm` and `finance`, depend on `core`'s `INotificationService`. Packages approved by the owner (2026-09-27): **MailKit** (SMTP) and **Scriban** (templates).

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
- **AAC1** Creating a company writes one `CREATED` entry: entity `Company`, a new value with its name, no old value, the Super Admin as actor, the company's own id as company (A1, A2, A5).
- **AAC2** Editing only a company's name writes one `UPDATED` entry whose old and new value hold only `name` (A2).
- **AAC3** A request that is rolled back (a company creation refused because the Owner's user id is taken) leaves no entry (A1).
- **AAC4** Creating an Owner or a Staff account writes an entry whose company is that company and whose values contain no password hash (A3, A5); a password change writes an `UPDATED` entry with no password field (A3).
- **AAC5** A change made by the suspension-lift job, and one made by the hidden system account, show actor "System" with no id (A4).
- **AAC6** The Super Admin lists every entry and filters by company and by date range; the Owner of company A sees only A's entries, also when the request carries B's company id (A7, A8).
- **AAC7** Staff and an anonymous caller get `403` and `401`; no endpoint and no database role of the application can update or delete an entry (A6, A7).
- **AAC8** The list is newest first, the page size is capped at 200, and `audit/entity-types` lists only types the caller may read (A8).
- **AAC9** The screen shows loading, empty, error with retry and success, and opening an entry shows its old and new value (A9).
- **AAC10** A secret column marked by its module never appears in any entry, including after a later migration adds a column to that entity (A3).

## 9. Open questions

- **CO1** Which SMTP/API provider is used in production, and the real `Email:*` values (secret store). Development uses the logging sender until then.
- **CO2** Whether SMS/WhatsApp are wanted in v1 (the model allows them; no plan yet).
- **CO3** An admin screen to view notifications, edit templates and retry failed ones (a later plan; needs role rules).

Audit trail, answered "recommended" by the owner on 2026-11-14 (A1-A10 follow these answers):
- **AQ1** Which rows are audited? *Recommended:* every aggregate root and child entity, except the insert-only history and log tables (login log, status, role and password-reset history, notification log, company entry, the audit entries themselves) and the session and token tables (high volume and security data, already recorded by the login log).
- **AQ2** Does the Owner see everything whose company is theirs, including the changes a Super Admin made inside their company and the changes of their Staff accounts? *Recommended:* yes, with the actor's user name shown.
- **AQ3** Are refused or failed actions (`403`, validation errors) audited? *Recommended:* no, only committed changes (failed logins are in the login log).
- **AQ4** How are old and new value shown? *Recommended:* the raw JSON plus a field-by-field table in the detail; human wording per entity comes with the business modules.
- **AQ5** Export of the audit log to Excel or CSV? *Recommended:* not in v1 (BRD NFR-12 lists exports of masters only).

## 10. Decisions

Proposed by the assistant on the owner's request of 2026-11-14 and approved with the audit trail on 2026-11-14.

| # | Decision |
|---|---|
| AD1 | The entry is written by the persistence layer when the change is saved, in the same transaction, so no module has to remember it (A1). The capture lives in the application's own `AppDbContext`, not in the shared library. (proposed by the assistant) |
| AD2 | Entries are kept forever in v1, the same as the iam login log (A10). (proposed by the assistant) |
| AD3 | An entry stores the actor's user name as a snapshot and the company as a plain id, so `core` stays independent of `iam` and `company` (A4, A5). (proposed by the assistant) |
| AD4 | The UI already has an audit screen for the Super Admin that calls `GET audit` and `GET audit/entity-types`; the contract of A8 matches it, and the Owner's screen reuses it (A9). (proposed by the assistant) |
