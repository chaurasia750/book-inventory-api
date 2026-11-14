# Spec: platform - Shared conventions and BuildingBlocks

- **Module:** `platform` (cross-cutting; code in `library/BuildingBlocks`)
- **Status:** carried over from the base project (HautInHousing); describes the code as it is. P18-P22 and PAC13-PAC17 (company-owned data) are Approved by abhishek on 2026-11-14, not built yet.
- **Source:** the Book Inventory business documents, see [docs index](../README.md) (none of the rules below were derived from them yet).
- **Plans:** [platform#08](../plans/platform/2026-11-14-08-handler-writes-saved.md) (Done, P24, PAC19), [platform#07](../plans/platform/2026-11-14-07-company-code-series.md) (Done, P23, PAC18), [platform#05](../plans/platform/2026-11-14-05-ui-company-address.md) (In progress, U12, UAC8), [platform#06](../plans/platform/2026-11-14-06-ui-member-mobile.md) (In progress, U13, UAC9), [platform#04](../plans/platform/2026-11-14-04-ui-sign-in-frame.md) (In progress, U11, UAC7), [platform#03](../plans/platform/2026-11-14-03-ui-list-and-detail.md) (In progress, U10, UAC6), [platform#02](../plans/platform/2026-11-14-02-ui-app-shell-theme.md) (In progress, U9, UAC5), [platform#01](../plans/platform/2026-11-14-01-company-owned-data.md) (Done, company-owned data P18-P22)

> Every module builds on these rules. They exist so that `iam`, `mlm`, `finance`, `realty` and `core` look and behave the same, and so that a second project can reuse the library unchanged.

## Base-project rules under review (Book Inventory)

- **Old references in this file:** numbers of removed plans (0001, 0002, 0010, 0015), the ERD (never in this repository), the `legacy-snapshot` tag of the base repository, and the modules `mlm` and `realty`. These are history only: do not look for them and do not build from them.
- **How to work with the rules below:** rules not listed in the table describe the code as it is and stay valid. A listed rule also describes current code, but it belongs to the base project's business: do not extend it or build on it. A Book Inventory feature that touches it changes or retires it through the spec flow, and its row is removed then.

| Rules | What is from the base project | For Book Inventory |
| --- | --- | --- |
| Intro and module-schema line (`iam`, `mlm`, `finance`, `realty`, `core`) | Module list of the base project | Book Inventory modules are defined by the BRD; `mlm` and `realty` are not wanted |
| PQ1 | Rank and rule reference tables for `mlm` | Not wanted |
| U6 | UI roles of the base project (`SALES_CRM`, `ACCOUNTS`, `MEMBER`) | Follows the iam roles decision above |
| U7, UAC1 | Legacy property, inventory, leads and commission UI code of the base project and its `legacy-snapshot` tag | History only; the carried-over UI pages of the old product are replaced feature by feature |
| ERD references (P4, PQ1, header) | ERD sections of the base project | No ERD in the repository |

## 1. Purpose and scope

Defines the data, API and configuration conventions that BuildingBlocks enforces for all modules. Out of scope: any business feature (those live in module specs) and UI code.

## 2. Concepts

- **Aggregate root / child entity** as described in `CLAUDE.md` (DDD section). Only roots have repositories.
- **Module schema:** each module owns one PostgreSQL schema (`iam`, `mlm`, `finance`, `realty`, `core`).
- **Message code:** a stable machine identifier for a user-facing message (for example `iam.auth.invalid_credentials`).

## 3. Rules

**Keys and audit**
- **P1** Every entity has a `Guid` primary key generated in the application as UUID v7 (`Guid.CreateVersion7()`), never by the database. Small reference tables that need an `int` identity are a later, explicit exception.
- **P2** Audit columns on every mutable table: `created_at`, `created_by`, `updated_at`, `updated_by` (UTC `timestamptz`; `*_by` is the user's `Guid`, or `Guid.Empty` for background work). Insert-only tables have only `created_*`. They are stamped by the DbContext, never by hand.
- **P3** Aggregate roots carry an optimistic concurrency token named `RowVersion`, mapped to PostgreSQL `xmin` (no extra column). A concurrent edit surfaces as `DbUpdateConcurrencyException` -> 409.
- **P4** `is_deleted` remains the soft-delete flag: `Remove` sets it, a named global query filter hides deleted rows. (The ERD's "terminal status" rule still applies to business entities; `is_deleted` is the technical safety net.)

**Naming and schema**
- **P5** Table, column, key, index and constraint names are `snake_case`; **table names are singular** and come from the entity class name (not from a `DbSet` property name). The primary key column is `id`; keys, foreign keys and indexes are named `pk_<table>`, `fk_<table>_<principal>_<cols>`, `ix_<table>_<cols>`, derived after the final table names are known.
- **P6** Each entity is placed in its module's schema through a shared configuration base class that takes the schema name and derives the table name (P5). Entities never call `ToTable` by hand.
- **P7** Enum properties are stored as `text` in UPPER_SNAKE_CASE (`SuperAdmin` -> `SUPER_ADMIN`) with a database `CHECK` constraint listing the allowed values, applied automatically by convention.
- **P8** Foreign keys default to `ON DELETE RESTRICT`; money is `numeric(18,2)`.

**Transactions**
- **P9** Write requests run in one transaction that commits on success and rolls back on an exception or a 4xx/5xx result (existing behaviour).
- **P10** An action marked `[PersistOnFailure]` commits the work done before it returned a 4xx result (it still rolls back on an exception). Used where a failure must leave a trace (failed-login log, lockout counter).

**Identity and API**
- **P11** The authenticated user is exposed through `ICurrentUser` (`UserId: Guid`, `UserName`, `Role: string`, `SessionId: Guid?`, `IsAuthenticated`). JWT claims are standard names: `sub` (user id), `role`, `sid` (session id), `name`; inbound claim mapping is off. Custom claims (`pwd_change`, `amr`, `sys`) are read by the modules that define them.
- **P12** Role checks use ASP.NET Core `[Authorize(Roles = ...)]` with role names defined by the application; BuildingBlocks contains no role constants or numeric role ids.
- **P13** Controllers declare short routes (`[Route("auth")]`); a route-prefix convention prepends `api/v{version:apiVersion}` (default v1), so the full route is `api/v1/auth`.
- **P14** Domain and validation exceptions carry a message **code**; the exception handler returns it as the problem-details extension `code`. Human text is resolved through a message-catalogue port, never hard-coded (`iam` R55).
- **P15** Configuration sections are bound with validation and fail at startup when invalid (`ValidateOnStart`).

**Company-owned data (Approved)**
- **P18** Every table that holds one company's business data has a `company_id` column (`uuid`, not null, a plain id with no foreign key across schemas; BRD §8 "Har table me CompanyId"), and its aggregate root implements the shared marker `ICompanyOwned` (a child entity of a company-owned aggregate carries its own `company_id` and the same marker, so it is filtered and audited by company too). Tables of the platform itself (users, companies, notification templates) are not company-owned.
- **P19** One global query filter limits every read of a company-owned entity to the company of the current session ([specs/company.md](company.md) R6): a company account has its own company, a Super Admin the company they entered. With no company the filter matches nothing, never everything. A row of another company answers `404`, identical to an unknown id. Code that must deliberately see across companies says so with `IgnoreQueryFilters([CompanyScope.FilterKey])` and is reviewed.
- **P20** The `company_id` of a new row is stamped from the session when it is saved, never taken from a request. Saving a new row with no company in the session is refused (`403 core.company_required`). A row's company never changes.
- **P21** The audit entry of a company-owned row carries the row's company ([specs/core.md](core.md) A5).
- **P22** A uniqueness rule that is "per company" (a school code, an item barcode) is a unique index that includes `company_id`, so two companies can use the same value.

**Company code series (Approved)**
- **P23** A company's generated document and master codes (`SCH-0001`, `VEN-0001`, later invoice and receipt numbers) come from one shared mechanism, the **code series**: one counter per company and per prefix, increased under a lock inside the creating transaction, so a number is never given twice (also after the row is deactivated or its transaction is retried after a conflict) and two rows created at the same moment get different numbers. The number has at least four digits; the prefix is fixed by the module that asks. A module never keeps a counter of its own. Proposed by the assistant on 2026-11-14 (vendor spec Q6, decision D2); Approved by abhishek in chat on 2026-11-14.

**Database lifecycle**
- **P16** The API applies pending EF Core migrations at startup when `Database:MigrateOnStartup` is `true` (the shipped configuration sets it to `true` for every environment; owner decision 2026-10-26, so Development and production both migrate on start). The option defaults to `false`, so a host without the section never migrates. The log names the pending migration ids (a warning naming the environment outside Development), a failed migration stops startup - the host never serves requests against a half-migrated schema - and only forward migrations are applied, never a drop, `update 0` or schema reset. An environment that must keep the step with a person sets the flag to `false`.
- **P17** Startup order is schema first, data second: the migration runner starts before the seeding hosted services, so a fresh database is migrated and then seeded in one start.

**Writes of domain event handlers**
- **P24** What a domain event handler writes (a new row, a changed row, its own events) is saved in the **same transaction** as the change that raised the event: after the events of a save are dispatched, any change they left is saved again and its events dispatched in turn, at most five rounds (more means a loop of events and is refused as a bug, `500`). A handler that fails rolls the whole request back. Added on 2026-11-14 for the finance spec (opening balances, finance R6); Approved by abhishek on 2026-11-14 (chat: "Platform fix: handler ka kaam bhi usi transaction mein save").

## 4. Data

Shared columns as in P2/P3/P4. No tables of its own.

## 5. API contract

No endpoints. Error shape: RFC 9457 problem details plus `code` (P14) and `traceId`.

## 6. Events

Aggregate roots raise domain events, dispatched after save inside the transaction; what a handler writes is saved in that same transaction (P24).

## 7. Dependencies

None.

## 8. Acceptance criteria

- **PAC1** A new entity gets a UUID v7 id before it is saved, and the id is time-ordered across entities created in sequence (P1).
- **PAC2** `created_*` are stamped on insert and `updated_*` on update from `ICurrentUser`; a caller-set `updated_by` is respected (P2).
- **PAC3** The generated PostgreSQL DDL uses the module schema, singular snake_case table names, `uuid` keys, `timestamptz` audit columns and no `row_version` column (xmin) (P3, P5, P6).
- **PAC4** An enum property is stored as UPPER_SNAKE text and the DDL contains a matching `CHECK` constraint (P7).
- **PAC5** Foreign keys are `RESTRICT` unless configured otherwise (P8).
- **PAC6** An action marked `[PersistOnFailure]` that returns 401 leaves its database changes committed; the same action throwing an exception rolls back; an unmarked action returning 4xx rolls back (P9, P10).
- **PAC7** With a bearer token containing `sub`, `role`, `sid`, `ICurrentUser` exposes them, and `[Authorize(Roles = "X")]` allows/denies accordingly (P11, P12).
- **PAC8** A thrown `ValidationFailureException` with a code produces problem details containing `code` and the catalogue text; an unknown code falls back to a generic text (P14).
- **PAC9** An invalid configuration value (for example a token lifetime of 0) stops startup with a clear error (P15).
- **PAC10** The solution builds with 0 warnings and every criterion above has been run and its observed result recorded in the plan (no automated test project, owner decision).
- **PAC11** With `Database:MigrateOnStartup=true` and `ASPNETCORE_ENVIRONMENT=Development`, starting against an empty database creates the full schema before the first request and the log lists the applied migration ids; with the flag `false` nothing is applied (P16, P17).
- **PAC12** With `Database:MigrateOnStartup=true` and `ASPNETCORE_ENVIRONMENT=Production`, starting against an empty database applies the same migrations, logs a warning naming the environment and lists the ids; a failing migration stops startup (P16).
- **PAC13** A company-owned row saved in company A has `company_id` A; the same request body carrying company B's id still stores A (P20).
- **PAC14** A caller of company B reading company A's row by id gets `404`, identical to an unknown id, and A's rows never appear in B's lists or counts (P19).
- **PAC15** A Super Admin with no company entered reads no company-owned rows and a write is refused with `403 core.company_required`; after entering A they read and write A's rows (P19, P20).
- **PAC16** A saved row's company cannot be changed through any behaviour of the entity (P20).
- **PAC17** Two companies can each hold a row with the same value of a per-company unique field, and one company cannot hold it twice (P22); the audit entry of a company-owned row names its company (P21).
- **PAC18** Two companies each get `X-0001` from the same prefix; ten parallel requests for one company get ten different numbers; the school codes of existing schools do not change when school is moved onto the series (P23).
- **PAC19** A handler that adds a row on an event is saved with the change that raised it (one commit); a handler that throws leaves neither the change nor its row; an event chain longer than five rounds is refused (P24).

## 9. Open questions

- **PQ1** Reference tables with `int` identity (ranks, rules per ERD §0.3) are deferred until `mlm` needs them; they will get their own small plan.

## 10. UI conventions (book-inventory-ui)

The Angular UI (Nx workspace, three apps: `public-site`, `admin`, `member`, plus shared libraries) is the product's real, previously working UI. **It is kept in full** and adapted to the new API; nothing is deleted (owner decision). Business pages come back to life module by module, when that module's API exists.

- **U1** Structure and flow stay: login lives only in `public-site`; `admin` and `member` redirect unauthenticated users to it with a hard browser navigation; the shared layout, ui and form-control libraries stay. Only the backend-facing layer changes (environment, auth, interceptors, auth models, guards, message catalogue).
- **U2** The access token is held in memory only. Every app bootstraps its session at start-up by calling the refresh endpoint (the refresh cookie is same-origin), so a hard navigation between apps keeps the user signed in. Nothing secret is written to `localStorage`/`sessionStorage`; only a non-secret "session hint" flag is allowed. The same holds in development: the dev-server proxies `/api` to the API, because the refresh cookie is `SameSite=Strict` and `http://localhost` to `https://localhost` is cross-site (Plan 0010).
- **U3** All HTTP goes through the shared `ApiService`/`HttpClient` with the API base URL defined in one place per environment (`/api/v1` when served by the API, the API's dev URL otherwise).
- **U4** There is no response envelope: services receive the payload directly. One error interceptor turns RFC 9457 problem details (`code`, `errors`, `title`) into user messages through the catalogue (U5); 401 triggers a single-flight refresh and retry.
- **U5** No user-facing text is hard-coded in new or changed components: texts come from one message catalogue (ngx-translate `en.json`), keyed by the API's message `code` for errors and by feature keys for labels.
- **U6** Roles are the API's strings (`SUPER_ADMIN`, `SALES_CRM`, `ACCOUNTS`, `MEMBER`). Staff land in `admin`, `MEMBER` in `member`; route guards and menu items are driven by role. The existing `admin`/`member` role buckets used by legacy code keep working through a mapping.
- **U7** **No UI business code is deleted** - except the legacy property/inventory, leads and commission code, which the owner allows to be replaced or removed in the plans of their modules (`realty`, `mlm`/`finance`); nothing else without an explicit answer. A module whose new API does not exist yet is hidden from navigation through a feature switch and enabled when its adaptation plan is done. The legacy code is also preserved in the git tag `legacy-snapshot`.
- **U8** UI changes are verified by `nx build` and `nx lint` of the affected projects plus a real browser/HTTP check (no UI test projects, same rule as the API).
- **U9** The `admin` and `member` apps share one app shell (sidebar, header bar, layout) and one colour theme taken from the owner's own project `relator-rj-ui` (owner confirmed ownership on 2026-11-14): a dark charcoal sidebar with a brand mark, a "Search menu" box (Ctrl K), menu sections, collapsible sub-menus, an icon-only collapsed state and a user card with Sign out at the bottom; a 56 px header bar with the menu button, theme switch and user menu; the charcoal and gold colour palette, in light and dark mode. Both apps show the same shell and differ only in their menu entries (the Super Admin's in `admin`, the company's Owner and Staff in `member`, by role, U6). The page content (tables, forms, dialogs) keeps its components and takes the new colours only through the shared palette.
- **U10** **List and detail.** The list-and-detail pages (Schools, Staff, Companies, Users) open as a full-width list; choosing a row narrows the list to one side (compact rows) and opens the record beside it, with a Close button (X) that returns to the full list; on a phone the record takes the whole screen. The open record's id is in the URL (`<list>/:id`), so Back, refresh and a shared link work, and an old `<list>/:id` URL opens the same record. Adding and editing a record open in the same panel (`<list>/new`, `<list>/:id/edit`), not in a dialog; short confirmations (a reason, deactivate) may stay dialogs. Schools do this since platform#03; the other lists still use dialogs for add and edit. The record's header shows its code or name, its status and its actions, with the Close button last. Layout and Close button are taken from the owner's own project `tulas-international-school-ui` (owner confirmed ownership on 2026-11-14).
- **U11** The sign-in screens of `public-site` (login, forgot password, reset password, welcome) share one frame taken from the owner's own project `relator-rj-ui` (owner confirmed ownership on 2026-11-14): on desktop a charcoal brand panel (logo, name, headline, three feature points, footer line) on the left and the page's own form on the right on an ivory background; on a phone only the form with a small logo. Only the look changes: fields, validation, redirects and API calls of these pages stay as they are (U1, U2).
- **U12** The company's screens (the `member` app: Owner and Staff) live at `/<company slug>/...` (company R15), not under `/member`: `/<slug>/schools`, `/<slug>/staff`, `/<slug>/change-password`. The app reads the slug from the address at start (its base address), so every route inside it stays as it is. After the session is known, a slug that is not the signed-in company's sends the user to the same page under their own company's slug (the server already answers other companies' data with 404, so this is only a convenience); a Super Admin or any user without a company is sent to `/admin`. Signing in as a company account opens `/<slug>/`. The application's own first segments (`admin`, `login`, `auth`, `error`, `welcome`, `api`, `assets`, `unauthorized`) are never company slugs (company R15). The API host and the single-port dev server serve any other first segment as the member app; the production IIS rules do the same.
- **U13** The `member` app is usable on a phone: from 360 px wide, every screen fits without sideways scrolling of the page, controls are reachable with a thumb (buttons and inputs at least 40 px high, menu in a drawer), tables scroll inside their own box or become stacked rows, dialogs and panels fit the screen. Desktop layouts are unchanged.

- **UAC1** The three apps build and lint (no new lint findings), and `git diff --diff-filter=D legacy-snapshot` lists no business page, library or service (only auth plumbing replaced by the new flow).
- **UAC2** No token or credential is ever stored in browser storage (inspected after login).
- **UAC3** Reloading any deep link in `admin`/`member` while logged in stays logged in (refresh bootstrap); after logout it redirects to login.
- **UAC4** A problem-details error from the API shows the catalogue text for its `code`; an unknown code shows a generic text.
- **UAC5** After login, `admin` and `member` show the same dark sidebar: the menu filters as the user types in Search menu, Ctrl K focuses it, a sub-menu opens and closes, the collapsed sidebar shows icons only and opens on hover, the user card shows the name and role and Sign out ends the session; the menu entries match the role (U9, U6).
- **UAC6** On Schools, Staff, Companies and Users: clicking a row narrows the list and opens the record beside it with the id in the URL; the X closes it and shows the full list; a reload on `<list>/:id` shows the same record beside the list; on a phone width only the record shows.
- **UAC7** `/login`, `/auth/forgot-password`, `/auth/reset-password` and the welcome page show the brand panel and form frame (desktop) or the form with a small logo (phone); signing in still redirects by role exactly as before (U11).
- **UAC8** Signing in as an Owner opens `/abc-book-center/`; `/abc-book-center/schools`, `/staff` and `/change-password` work and survive a reload; opening `/children-book-center/schools` as the ABC Owner lands on `/abc-book-center/schools`; `/member/...` still reaches the app and lands on the company's own address; signing in as Super Admin still opens `/admin` (U12).
- **UAC9** At 360 px width no member screen scrolls sideways (login, Schools list and panel, Staff, Settings, Change password, Audit log); the menu opens as a drawer; tables stay inside their box (U13).
