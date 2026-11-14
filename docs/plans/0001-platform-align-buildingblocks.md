# Plan 0001: Align BuildingBlocks with the shared conventions

- **Number:** 0001
- **Module:** `platform` (primary) ; also touches: none (no `iam` code yet)
- **Spec:** [docs/specs/platform.md](../specs/platform.md) - covers P1-P15, PAC1-PAC10 ; motivated by [iam.md](../specs/iam.md) D1, D2, D6, D17
- **Status:** Done
- **Created:** 2026-09-26   **Completed:** 2026-09-26

## 1. Goal

BuildingBlocks and the `src` skeleton follow the platform spec: UUID v7 keys, ERD audit column names, module schemas with singular snake_case tables, enums as text + CHECK, `[PersistOnFailure]`, claim-agnostic identity, versioned routes, code-carrying errors, validated configuration. A test project proves it. After this plan, the first `iam` entity can be written without touching BuildingBlocks.

## 2. Out of scope

Any `iam` entity, endpoint or migration (Plan 0002+); `int`-key reference tables (PQ1); serving the UI from the API; the UI.

## 3. Approach

- **Keys:** `DomainBase.Id` becomes `Guid`, assigned in the constructor with `Guid.CreateVersion7()` and configured `ValueGeneratedNever`. Because ids exist before saving, "is this new?" can no longer be `Id <= 0`; the repository uses `AddAsync` for new aggregates and `Update` (only when detached) for existing ones. `TrackGraph` (relied on `Id <= 0`) is removed; nothing uses it.
- **Audit:** rename `CreatedDate/ModifiedDate/ModifiedBy` to `CreatedAt/UpdatedAt/UpdatedBy`; `CreatedBy/UpdatedBy` become `Guid` (`Guid?` for update). `ICurrentUser.UserId` is `Guid`.
- **Concurrency:** rename `Version` to `RowVersion` (still `uint` -> xmin).
- **Naming/schema:** a `ModuleEntityConfiguration<T>` base in Persistence takes the schema name and calls `ToTable(snake_case(typeof(T).Name), schema)`. `BuildingBlocksDbContext` also applies a fallback so an entity with no configuration still gets a singular snake_case table name (never a `DbSet` name). Snake-case of columns/keys stays with `UseSnakeCaseNamingConvention`.
- **Enums:** `ConfigureConventions` registers a generic `UpperSnakeEnumConverter<T>` for every enum, and the model loop adds a `CHECK (col IN (...))` per enum column.
- **`[PersistOnFailure]`:** attribute in Configuration; `TransactionFilter` commits instead of rolling back when the result is 4xx and the action (or controller) carries it. Exceptions still roll back.
- **Identity:** `ICurrentUser` reads `sub`/`role`/`sid`/`name` (no numeric ids); JWT options set `MapInboundClaims = false`, `RoleClaimType = "role"`, `NameClaimType = "name"`. `AdminOnlyAttribute`, the numeric-claim helpers in `ClaimsExtension` and `AppClaimTypes` numeric ids are removed; role checks use `[Authorize(Roles = ...)]`.
- **Routes:** a route-prefix convention (registered in `AddMvcWithFilters`) prepends `api/v{version:apiVersion}` to every controller route, so controllers declare only `[Route("auth")]`; unversioned controllers get the default v1.
- **Errors:** `ValidationFailureException` (and a small `AppException(code)` base) carry a `Code`; `GlobalExceptionHandler` writes it as the `code` extension and the text from `IMessageCatalog` (default implementation: dictionary registered by modules; fallback text when unknown).
- **Configuration:** `AddValidatedOptions<T>(section)` helper (data annotations + `ValidateOnStart`); `JwtSetting` uses it.
- **Environment (owner decision):** everything runs natively on Windows; **no Docker**. The real-PostgreSQL checks use the PostgreSQL 17 Windows service already on this machine (`localhost:5432`) through a throw-away database `book_inventory_check_<guid>` that the check creates and drops; the application database is never touched. The connection string is read from the environment variable `BOOK_INVENTORY_TEST_PG` (for example `Host=localhost;Port=5432;Username=postgres;Password=...`); when it is not set those checks are reported as "NOT verified".
- **No test project and no new packages** (owner decision: no automated tests). Verification is by build plus scripted scratch checks, listed in section 5.

## 4. Tasks

- [x] 1. Domain: `Guid` ids with UUID v7, audit renames, `RowVersion`, `ICurrentUser` (Guid, role string, session id), remove `TransactionDomainBase` numeric assumptions
- [x] 2. Persistence: DbContext updates (Guid audit, `ValueGeneratedNever`, table-name fallback), `ModuleEntityConfiguration<T>`, enum converter + CHECK convention, remove `TrackGraph`, repository (`GetByIdAsync(Guid)`, add/update semantics)
- [x] 3. Configuration: `[PersistOnFailure]` + `TransactionFilter`, claims/`HttpCurrentUser`/JWT options, remove `AdminOnly`, route-prefix convention (`api/v{version}`), `code` in exceptions + handler + `IMessageCatalog`, `AddValidatedOptions<T>`
- [x] 4. `src` skeleton: adjust `AppDbContext`/DI to the new APIs; still builds and starts
- [x] 5. Verify by hand (no test project): run one scripted check per acceptance criterion PAC1-PAC9 in a scratch console app / `curl` / SQL, record inputs and outputs in section 5
- [x] 6. Docs: update `CLAUDE.md` (Guid ids, naming, schema, enum, `[PersistOnFailure]`, claims), `README.md`, ERD note (`row_version` implemented by xmin; `is_deleted`)
- [x] 7. Verify and close: build 0 warnings; run the API and hit `/health`; set spec/plan status; add plan link to `platform.md`

## 5. Verification

`dotnet build book-inventory.slnx` -> 0 warnings, 0 errors (run after every task and at the end).

Checks were run with a throw-away console app (not committed) that references the real BuildingBlocks projects, a sample `Widget` (aggregate root, enum property) / `Part` (child) model in schema `iam`, SQLite in memory for runtime behaviour, Npgsql for DDL generation, and a real Kestrel host on `localhost:5291` for the HTTP pipeline. Observed output (all lines PASS):

| Criterion | Observed |
|---|---|
| PAC1 | ids `01a0de3b-4371-7b14-...` < `01a0de3b-437a-7be6-...`: version 7, time ordered for entities created 3 ms apart |
| PAC2 | insert: `created_by` = actor, `created_at` UTC, `updated_*` empty; update: `updated_at/updated_by` set; a caller-set `updated_by` is kept; `Remove` keeps the row with `is_deleted` |
| PAC3 | DDL: `CREATE TABLE iam.widget` and `iam.part` (DbSet was named `Widgets`), `id uuid NOT NULL` (no GENERATED), `created_at timestamp with time zone`, `created_by uuid`, `updated_at`, `updated_by uuid`, no `row_version`/`xmin` column |
| PAC4 | `status text NOT NULL` and `CONSTRAINT ck_widget_status CHECK ("status" IN ('ACTIVE', 'PENDING_APPROVAL'))` |
| PAC5 | `CONSTRAINT fk_part_widget_widget_id FOREIGN KEY (widget_id) REFERENCES iam.widget (id) ON DELETE RESTRICT`; `price numeric(18,2)` |
| PAC6 | `POST api/v1/probe/marked-401` -> 401 and its row exists; `unmarked-401` -> 401 and no row; `marked-throw` (row saved, then exception) -> 400 and no row; `ok` -> 200 and row exists |
| PAC7 | token with `sub/role/sid/name` -> `GET api/v1/probe/whoami` 200 with `{"userId":"...","userName":"Asha","role":"SUPER_ADMIN","sessionId":"...","isAuthenticated":true}`; role `SALES_CRM` -> 403; no token -> 401 |
| PAC8 | `{"status":400,"detail":"This is the catalogue text for probe.coded.","code":"probe.coded","traceId":"00-..."}`; unknown code -> `"detail":"The request could not be processed."` with its `code` |
| PAC9 | short JWT key: `Invalid JwtSetting configuration: The field IssuerSigningKey ... minimum length of '32'`; option `Lifetime=0` with `[Range(1,10)]`: host start fails with `OptionsValidationException` |
| PAC10 | build 0 warnings; the API (`BookInventory.Api`, Development) starts, `/health` 200 and `/openapi/v1.json` 200 |

**Real PostgreSQL 17 (Windows service, throw-away database `book_inventory_check_<guid>`, created and dropped by the check; credentials from the local user-secrets):**

| Criterion | Observed |
|---|---|
| PAC4 | `UPDATE iam.widget SET status = 'BOGUS'` fails with SQLSTATE 23514, constraint `ck_widget_status`; the saved enum is stored as `PENDING_APPROVAL` |
| PAC3 | the real table has columns `id, name, status, is_deleted, created_at, created_by, updated_at, updated_by` (no `row_version`); `xmin` is readable |
| P3 / xmin | two contexts edit the same widget: the second `SaveChanges` throws `DbUpdateConcurrencyException`; the first writer's value survives |

**NOT verified:** nothing left open for this plan.

## 6. Risks and notes

- Breaking change for anything built on the old `long` ids; nothing in this repo does yet.
- `xmin` concurrency and the enum CHECK are proven on the real PostgreSQL 17 service, not only through DDL text. I need the test connection string (server user/password) as `BOOK_INVENTORY_TEST_PG` on this machine; I will not guess credentials.
- `[PersistOnFailure]` is deliberately explicit: forgetting it on a future security-logging endpoint would silently drop its writes, so `iam` acceptance criteria AC4 tests it end to end.
- **Deviations from the plan text (all recorded here, spec updated):**
  - The business exceptions (`AppException` family) and `IMessageCatalog` live in `BuildingBlocks.Domain` so that domain code can throw coded failures without referencing the web layer; the catalogue implementation and `AddMessages` are in `BuildingBlocks.Application`.
  - Beyond the plan: constraint names are normalised after table names are final (`pk_<table>`, `fk_<table>_<principal>_<cols>`, `ix_<table>_<cols>`); found because a foreign key name had picked up the temporary plural DbSet name.
  - The primary key column is `id` in every table (the ERD's `<table>_id` naming is not derivable per entity); the ERD conventions were updated.
  - `ChangeTrackerConfigurationExtensions` (`TrackGraph`, relied on `Id <= 0`) was removed; a `Guid` id exists before saving, so use `AddAsync`/`Update` explicitly.
