# Book Inventory API - project specifics

Imported by `CLAUDE.md`. Everything that names this project, its stack, modules or tooling lives here; `CLAUDE.md` stays generic and is copied unchanged to other projects.

.NET 10 monolith API on PostgreSQL. Reply to the user in Hinglish (Hindi in Latin script) unless asked otherwise; code, identifiers and commit messages stay in English.

## Modules, plans and commits

- Modules are the database schemas: `iam`, `finance`, `core`, plus `platform` for cross-cutting/BuildingBlocks work. Specs: `docs/specs/<module>.md`; plans: `docs/plans/<module>/`. Legacy plans `0001`-`0016` stay at the root of `docs/plans/`.
- Dependency direction between modules is written in the module specs (no ERD document is in the repository yet: ask before relying on one).
- Build command: `dotnet build book-inventory.slnx` -> 0 warnings, 0 errors.

## Identifiers

- Every entity id is a UUID v7 (`Guid.CreateVersion7()`), including log and audit tables. Never `bigint`/identity columns, even when a business document says so.

## Money and ledger (finance, payment, ledger)

- All amounts are `decimal` (`numeric(18,2)` unless the spec says otherwise); `MidpointRounding.ToEven` vs `AwayFromZero` is a business decision - ask.
- The ledger is double-entry: every transaction balances (debits = credits). A party balance (school, vendor) changes only through ledger postings made by the ledger aggregate. Payment receive or pay, return and refund are the money-moving operations of the generic Money rules.

## Security specifics

- Authorization includes ownership: the ownership rule of each module (for example the company a user belongs to) is written in its spec.
- Never commit or print PAN/Aadhaar, bank details, phone or email of real customers.

## Safety of the environment

- Never run `dotnet ef database update` against a shared/prod database.
- Migrations: owner decision - I may apply them myself, but **only** to the local development database `book_inventory` on the local PostgreSQL service (never to a shared, staging or production database; those are applied by the user). Apply with an explicit connection string, confirm the target database name before running, and never run `database drop`/`update 0` or reset the schema without asking.

## Environment (Windows, no Docker)

- Everything runs natively on Windows: PostgreSQL as a Windows service (local dev has PostgreSQL 17 on `localhost:5432`), the API on Kestrel/IIS. Do not introduce Docker, docker-compose, Testcontainers or Linux-only tooling.
- The local dev connection string (with the password) lives in **user-secrets** of `BookInventory.Api` (`dotnet user-secrets`), never in a committed file; `appsettings.Development.json` holds it without the password. Checks may read it from there for their own process.
- Checks that need a real database use a throw-away database on the local PostgreSQL service, created and dropped by the check itself, with the connection string taken from the `BOOK_INVENTORY_TEST_PG` environment variable. Never touch the application database and never guess credentials: ask.
- Use Windows-safe paths and PowerShell/Git Bash commands; the UI dev server and the API are started natively (`dotnet run`, `npm`/`nx`).

## Verification without test projects

Owner decision: **no automated test projects or test cases are written for now** (token budget). Do not add xunit/NUnit/MSTest projects, test packages or test files. Verification is instead:
- `dotnet build book-inventory.slnx` at 0 warnings, 0 errors.
- For each acceptance criterion in the plan, run a small throw-away script or command (a scratch console app under the session scratchpad, `curl` against the running API, or SQL against the local database) and record the exact input and observed output in the plan's "Spec coverage" table. Scratch code is never committed.
- Prefer the cheapest check that really proves the criterion (generate the PostgreSQL DDL, run one request, run one SQL query) over broad exploration.
- Anything that could not be checked is listed as "NOT verified" in the plan and in the hand-over summary.
This raises the human review burden (see `docs/Developer_Guide.md`, "Reviewing the agent's work"): money, authorization and migrations are always reviewed by a person.

## Layout

```
library/BuildingBlocks/   shared, reusable across ALL projects (BookInventory.BuildingBlocks.*)
  Domain -> Application -> Persistence -> Configuration ;  Logger (standalone)
src/                      this application
  BookInventory.Domain          entities, enums, domain events
  BookInventory.Application     use cases, services, DTOs, event handlers
  BookInventory.Infrastructure  AppDbContext, EF configurations, migrations, external integrations
  BookInventory.Api             Program.cs, controllers, appsettings (composition root)
book-inventory.slnx           every project must be listed here
```

- Dependencies point one way only: `Api -> Infrastructure -> Application -> Domain`. Domain references nothing but BB.Domain.
- Do not put app/business code in `library/`, and do not put app-specific names (BookInventory, Invoice, School...) there. Anything generic that a second project would need goes in `library/`.
- New project: add it to `book-inventory.slnx`, keep `net10.0`; package versions go in `Directory.Packages.props` only (central management - no `Version=` in csproj).
- The shared library the generic rules mean is `library/BuildingBlocks`; prefer what it provides. The UI code lives in the sibling repository `book-inventory-ui`.
- Method parameter count: `CancellationToken` counts toward the limit of 4.

## Coding standards (naming)

**Class and file naming** (PascalCase, English, file name = type name)
- Aggregate root and entity: singular noun (`AppUser`, `LedgerEntry`); enum: singular noun, members PascalCase; events: past tense (`InvoiceIssued`).
- Suffixes by role: `...Controller`, `...Service` (application service), `...Repository` (only if BuildingBlocks lacks it), `...Configuration` (EF `IEntityTypeConfiguration`), `...Options` (bound settings), `...Handler` (domain event handler), `...Validator`, `...Exception` (only if a new exception type is needed), `I...` for interfaces.
- DTOs and inputs are records named by purpose: `...Request` (input), `...Response` / `...Dto` (output), `...Query` (list filters). No `Manager`, `Helper`, `Utils`, `Data` or `Info` classes; name what the class does.
- Namespace = folder path (`BookInventory.Application.Iam`); module folders match the schema names.
- Methods: verb + object (`IssueInvoice`), async methods end in `Async`; booleans read as a question (`IsActive`, `HasPassword`). Private fields `_camelCase`.

**Table and column naming** (see also "Database")
- Table = singular snake_case of the class name in the module schema (`AppUser` -> `iam.app_user`); never plural, never prefixed with the module name.
- Columns are snake_case, foreign keys `<entity>_id` (`school_id`), booleans `is_`/`has_` prefix, timestamps `..._at` (UTC), money `..._amount` as `numeric(18,2)`. Constraint/index names come from the convention (`pk_`, `fk_`, `ix_`, `uq_` for unique).

## Domain: aggregates (DDD)

- **DDD is mandatory, not a suggestion.** Every module is built the DDD way; code that puts business rules anywhere but the domain is rejected in review.
  - Business rules and state transitions live in the aggregate root (or a value object / domain service when a rule spans no single aggregate), never in controllers, Application services, EF configurations or handlers. Application services only orchestrate: load the aggregate, call one behaviour, save.
  - Use the language of the business (spec) for class, method and event names (`Approve`, `Suspend`, `InvoiceIssued`), not CRUD verbs (`SetStatus`, `UpdateX`) on the public surface.
  - Values that carry a rule (money, phone, email, percentage) are value objects or validated in one place, not raw primitives checked ad hoc at each call site.
  - A rule exists in exactly one place. Before adding code, search for an existing behaviour, guard or helper and reuse it; a second copy of a rule, mapping, query or message is a defect.
  - Every new class is placed by its DDD role (root, child entity, value object, domain event, domain service in Domain; use case, query service, port in Application; adapter in Infrastructure) and follows the pattern already used by its neighbours in the module.

- Model the domain as aggregates. An **aggregate root** derives from `AggregateRoot` (audited, soft-deletable, has an optimistic-concurrency `RowVersion` = PostgreSQL `xmin`). Everything that must stay consistent with it lives inside it as **child entities** (`DomainBase` / `TransactionDomainBase` / `AuditableDomainBase`), e.g. `Ledger` (root) owns its `LedgerEntry` rows; `Invoice` owns its lines.
- Only roots have repositories: `IRepository<T>` / `IUnitOfWork.Repository<T>()` are constrained to `AggregateRoot`, so a child entity can never be loaded or saved on its own. Reach children through the root (`IncludeMultiple(x => x.Entries)`).
- Enforce invariants inside the root. Properties have `private set`; state changes go through root methods that validate (`ledger.Post(entry)`, `invoice.Issue(schoolId)`). No anemic setters, no business rules in controllers or services that could live in the root. Child collections are exposed as `IReadOnlyList<T>` over a private backing field (map it with `Navigation(..).HasField(..)` in the entity configuration).
- Keep aggregates small. Do one transaction per aggregate; consistency across aggregates is eventual, via domain events.
- Aggregates reference each other by id (`Guid SchoolId`), never by object/navigation. Cross-aggregate reads go through the Application layer (query services), not by navigating.
- Only the root raises domain events (`AddDomainEvent` is `protected`); handlers in Application react (`IDomainEventHandler<T>`). Events are named in the past tense (`MoneyCredited`, `InvoiceIssued`) and carry ids/values, not entities.
- Pick the base deliberately: `AggregateRoot` for anything mutable (Invoice, School, Vendor, Ledger, Payment); `TransactionDomainBase` children for insert-only history (ledger entries, audit trail rows) that is never updated or deleted.
- Table names: every entity configuration derives from `ModuleEntityConfiguration<T>(schema)`, which puts the entity in its module schema (`iam`, `finance`, `core`) with a **singular** snake_case table name taken from the class (`AppUser` -> `iam.app_user`, `LedgerEntry` -> `finance.ledger_entry`). Never call `ToTable` yourself and never rely on a `DbSet` name for the table name.

## Database (PostgreSQL only)

- snake_case for tables, columns, keys, indexes, constraints. This is automatic (`UseSnakeCaseNamingConvention` plus the context's name normalisation: `pk_<table>`, `fk_<table>_<principal>_<cols>`, `ix_<table>_<cols>`); never add `[Table]`/`[Column]`/`HasColumnName` with non-snake_case names.
- Entities derive from the bases described under "Domain: aggregates". The key is a `Guid` UUID v7 assigned in the base constructor (`Guid.CreateVersion7()`), never generated by the database, so a new aggregate is saved with `AddAsync` and an existing one with `Update` (never `Update` for a new entity). `RowVersion` is the concurrency token: never set it, and let `DbUpdateConcurrencyException` surface (it becomes 409).
- Enum properties need no configuration: they are stored as UPPER_SNAKE `text` (`SuperAdmin` -> `SUPER_ADMIN`) with a database `CHECK` constraint listing the values. Do not store enums as numbers.
- Delete = soft delete (`repo.Remove`). Do not add manual `IsDeleted` filters; the global query filter does it. Use `IgnoreQueryFilters()` deliberately and only where needed.
- Audit columns and domain events are handled by `BuildingBlocksDbContext.SaveChangesAsync`; never set `CreatedAt/CreatedBy/UpdatedAt/UpdatedBy` by hand. Timestamps are UTC (`DateTime.UtcNow`/`TimeProvider`), never `DateTime.Now`. `ICurrentUser.UserId` is a `Guid` (`Guid.Empty` for background work).
- Configure entities with one `IEntityTypeConfiguration<T>` class each in Infrastructure; override `ConfigureModel`, never `OnModelCreating` (it is sealed).
- FKs default to RESTRICT. Money is `numeric` with explicit precision (`HasPrecision`), never float/double.
- Pure EF Core only: **no EFCore.BulkExtensions or other third-party persistence add-ons** (no Dapper either unless asked). Use `AddRangeAsync`, `UpdateRange`, `RemoveRange`, `ExecuteUpdateAsync`, `ExecuteDeleteAsync`, `ExecuteSqlAsync` (parameterised `FormattableString`).
- Never build SQL by string concatenation.
- Migrations: `dotnet ef migrations add <Name> -p src/BookInventory.Infrastructure -s src/BookInventory.Api`. Review generated SQL; never edit an applied migration.

## Data access and transactions

- Application code uses `IRepository<T>` / `IUnitOfWork` only, never `AppDbContext` directly. Repositories do not call `SaveChanges`.
- Write requests (POST/PUT/PATCH/DELETE) are wrapped in one transaction by `TransactionFilter`; do not open nested transactions in controllers. Outside HTTP (jobs) use `StartTransactionAsync` ... `CommitAsync`.
- An action whose failure must leave a trace (failed login attempt, lockout counter) is marked `[PersistOnFailure]`: it commits the work done before it returned an error status. It must return a result, not throw (an exception always rolls back). Unmarked actions roll back on any 4xx/5xx.
- Every async method takes and forwards a `CancellationToken`.
- Configuration sections are bound with `AddValidatedOptions<T>(configuration, section)` (data annotations + `ValidateOnStart`) so invalid settings stop startup.
- Reads that will not be modified use `TableNoTracking`. List endpoints must page (`GetPagedListAsync`; page size is capped at 200) and sort only through `OrderBy(string)`/`SortHelper` (whitelists columns).
- Cross-aggregate side effects go through domain events (raised by the root + `IDomainEventHandler<T>`), not direct calls between modules. Handlers run inside the same transaction; a failing handler rolls the request back.

## API

- **Controller folders mirror the modules.** Controllers live in `src/BookInventory.Api/Controllers/<Module>/` (`Controllers/Iam/AuthController.cs`), the same module folder names as in Application (`BookInventory.Application/Iam`), Domain and Infrastructure. Never put a controller directly in `Controllers/`; namespace = folder (`BookInventory.Api.Controllers.Iam`). Request/response DTOs of a module follow the same rule in their own `<Module>` folder.
- Controllers derive from `ApiControllerBase` (JWT required by default; `[AllowAnonymous]` must be a conscious choice) and return plain results (`Ok(dto)`, `NotFound()`, `CreatedAtAction(..)`). Controllers declare only a short route (`[Route("auth")]`); the prefix `api/v{version}` is added by convention, so the full route is `api/v1/auth`. There is no response envelope: errors are RFC 9457 `ProblemDetails`, model-validation errors are the standard `ValidationProblemDetails`. Restrict by role with `[Authorize(Roles = "...")]` using role names defined by the application (BuildingBlocks contains no role names or ids). Get the caller by injecting `ICurrentUser` (claims `sub`, `role`, `sid`, `name`).
- Return DTOs, never domain entities. Controllers stay thin: bind, call one Application service/use case, return.
- Throw the typed business exceptions with a **message code**, never a text: `ValidationFailureException` (400), `NotFoundException` (404), `ConflictException` (409), `ForbiddenException` (403), `UnauthenticatedException` (401). The code (for example `iam.user.invalid_transition`) is returned in the problem details `code` field, and its human text lives in the module's message catalogue (`services.AddMessages(...)`); an unknown code shows a generic text. BCL exceptions still map: `KeyNotFoundException` 404, `UnauthorizedAccessException` 403. Do not catch-and-swallow in controllers. `InvalidOperationException` is NOT mapped to 400 (it means a bug and returns 500).
- Do not leak exception messages/stack traces to clients; `GlobalExceptionHandler` already returns a generic 500 outside Development.
- Secrets (JWT key, connection strings) never go in committed appsettings; `appsettings.Development.json` holds local dev values only.
- Register services with `[AutoRegister]` (scoped by default) instead of hand-written `AddScoped` lists.
- Nullable reference types are on: no `#nullable disable`, no `!` to silence a real null.
