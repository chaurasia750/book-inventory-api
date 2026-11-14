# Spec: platform - Shared conventions and BuildingBlocks

- **Module:** `platform` (cross-cutting; code in `library/BuildingBlocks`)
- **Status:** Approved
- **Last updated:** 2026-10-26
- **Source docs:** [ERD §0 Conventions](../BookInventory_ERD.md), [specs/iam.md §9 decisions D1, D2, D6, D17](iam.md), `CLAUDE.md`
- **Plans:** [0001](../plans/0001-platform-align-buildingblocks.md) (BuildingBlocks), [0002](../plans/0002-platform-ui-foundation.md) (UI foundation), [0015](../plans/0015-platform-cors-off-without-origins.md) (no CORS headers without configured origins), [0016](../plans/0016-platform-startup-migrations.md) (Development applies its own pending migrations at startup), [0010](../plans/0010-platform-hosting.md) (hosting, done: the API serves the apps from `wwwroot` only in the IIS publish output; locally the API and each UI app run on their own ports, the UI dev servers proxy `/api`)

> Every module builds on these rules. They exist so that `iam`, `mlm`, `finance`, `realty` and `core` look and behave the same, and so that a second project can reuse the library unchanged.

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

**Database lifecycle**
- **P16** The API applies pending EF Core migrations at startup when `Database:MigrateOnStartup` is `true` (the shipped configuration sets it to `true` for every environment; owner decision 2026-10-26, so Development and production both migrate on start). The option defaults to `false`, so a host without the section never migrates. The log names the pending migration ids (a warning naming the environment outside Development), a failed migration stops startup - the host never serves requests against a half-migrated schema - and only forward migrations are applied, never a drop, `update 0` or schema reset. An environment that must keep the step with a person sets the flag to `false`.
- **P17** Startup order is schema first, data second: the migration runner starts before the seeding hosted services, so a fresh database is migrated and then seeded in one start.

## 4. Data

Shared columns as in P2/P3/P4. No tables of its own.

## 5. API contract

No endpoints. Error shape: RFC 9457 problem details plus `code` (P14) and `traceId`.

## 6. Events

Unchanged: aggregate roots raise domain events, dispatched after save inside the transaction.

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

- **UAC1** The three apps build and lint (no new lint findings), and `git diff --diff-filter=D legacy-snapshot` lists no business page, library or service (only auth plumbing replaced by the new flow).
- **UAC2** No token or credential is ever stored in browser storage (inspected after login).
- **UAC3** Reloading any deep link in `admin`/`member` while logged in stays logged in (refresh bootstrap); after logout it redirects to login.
- **UAC4** A problem-details error from the API shows the catalogue text for its `code`; an unknown code shows a generic text.
