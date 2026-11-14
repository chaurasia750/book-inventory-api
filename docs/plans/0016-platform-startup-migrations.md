# Plan 0016: platform - apply pending migrations at startup

- **Number:** 0016
- **Module:** `platform` (primary) ; also touches: none
- **Spec:** [docs/specs/platform.md](../specs/platform.md) - sections/rules covered: P16, P17, PAC11, PAC12
- **Status:** Done
- **Created:** 2026-10-26   **Completed:** 2026-10-26

> A plan says HOW one deliverable slice of the spec will be built. Several plans can serve one spec.
> Once Done, a plan is history: do not rewrite it. New work or a changed approach is a new plan.

## 1. Goal

The API brings its own database schema up to date before Kestrel listens and before the seeders run, in Development and
in production (owner decision 2026-10-26), so deploying a release no longer needs a separate migration step. An
environment that must keep the step with a person switches the flag off.

## 2. Out of scope

- `dotnet ef database drop`, `update 0`, schema reset or any destructive step; migrations stay forward-only.
- Seeding, which keeps its own hosted services (unchanged order: after the migration runner).
- Migrations themselves: this plan adds no migration and changes no model.

## 3. Approach

The host, not `Program.cs` inline code, owns the step - same reason as `IamSeedHostedService`: `dotnet ef` builds the
host but never starts hosted services, so generating a migration still needs neither a database nor the dev secrets.

1. **Option** `DatabaseMigrationOptions` (`src/BookInventory.Infrastructure/Persistence/Configuration/`), section
   `Database`, one validated boolean `MigrateOnStartup` whose code default is `false` (a host without the section never
   migrates). Bound with `AddValidatedOptions<T>` in `Program.cs` like every other section (P15).
2. **Runner** `DatabaseMigrationHostedService` (`src/BookInventory.Api/Hosting/`, next to the other `IHostedService`
   classes, 4 constructor dependencies):
   - flag off -> informational log, nothing applied;
   - flag on -> resolve `AppDbContext` from a scope, read `GetPendingMigrationsAsync`, log the pending ids (a warning
     that names the environment outside Development), `MigrateAsync`, log how many were applied. A failure propagates,
     so startup fails and the host never serves requests against a half-migrated schema.
3. **Registration**: `AddHostedService<DatabaseMigrationHostedService>()` immediately before
   `IamSeedHostedService` (`Program.cs`), because hosted services start in registration order and the seeders need the
   schema (P17).
4. **Configuration**: `"Database": { "MigrateOnStartup": true }` in `appsettings.json`, so every environment including
   the IIS publish output migrates on start; `Database__MigrateOnStartup=false` in the application-pool environment
   variables is the documented opt-out.
5. **Docs**: platform spec P16/P17 + PAC11/PAC12, plan link, and `docs/Release_Checklist.md` line 15 reworded: the
   schema is now brought up to date by the start itself, so the person reviews the generated SQL of the release and
   confirms that the automatic run is acceptable, instead of running `database update` by hand.

No new package, no new project, no change to `AppDbContext` or to any migration.

## 4. Tasks

- [x] 1. Infrastructure: `DatabaseMigrationOptions` (`Database:MigrateOnStartup`, code default false)
- [x] 2. Api: `DatabaseMigrationHostedService` (pending-migration log, fail-fast on error)
- [x] 3. Api: register the options and the hosted service before `IamSeedHostedService` in `Program.cs`
- [x] 4. Config: `Database:MigrateOnStartup=true` in `appsettings.json`
- [x] 5. Verify by hand: section 5 checks C1-C4, results recorded here
- [x] 6. Update `docs/Release_Checklist.md` line 15 (spec P16/P17 and PAC11/PAC12 are written)
- [x] 7. `dotnet build book-inventory.slnx` at 0 warnings, 0 errors; commit as `Plan 0016: ...`

## 5. Verification

Every check runs against a **throw-away** database on the local PostgreSQL service (created and dropped by the check,
never the application database), with the connection string passed through the
`ConnectionStrings__Default` environment variable. No automated test project (owner decision).

| # | Criterion | Input | Expected | Observed |
| --- | --- | --- | --- | --- |
| C1 | PAC11 - fresh Development database is migrated before the first request | empty database `book_inventory_migrate_check`, `ASPNETCORE_ENVIRONMENT=Development` | start log lists the pending migration ids then `Applied N migration(s)`; `iam`/`core` schemas exist | `Applying 6 pending migration(s): 20260926155042_Iam_Initial, ...`; `Applied 6 migration(s); the schema is up to date.`; `Now listening`; 9 app tables (`core.notification`, `core.notification_log`, `core.notification_template`, `iam.app_user`, `iam.password_reset_token`, `iam.user_login_log`, `iam.user_role_history`, `iam.user_session`, `iam.user_status_history`) |
| C2 | PAC11 - flag off applies nothing | same empty database, `Database__MigrateOnStartup=false` | start log says the runner is off; no table created | `Database:MigrateOnStartup is off: pending migrations are not applied.`; app started; 0 `iam`/`core` tables (seeders/workers logged their own "schema missing" messages, pre-existing behaviour) |
| C3 | PAC12 - a non-Development environment migrates too, with a warning | empty database, `ASPNETCORE_ENVIRONMENT=Production`, `Auth__Reset__LinkUrl` set so the host starts | warning naming `Production` with the ids; `iam`/`core` schemas exist; no table before the start | `WRN Applying 6 pending migration(s) to the Production database: ...`; `Applied 6 migration(s); the schema is up to date.`; `Hosting environment: Production`; `Now listening`; 9 app tables, 6 migrations |
| C4 | Definition of done | `dotnet build book-inventory.slnx` | 0 warnings, 0 errors | 0 warnings, 0 errors |

**NOT verified:** a real IIS deployment start against the production database (no production database is reachable from
this machine).

## 6. Risks and notes

- **Owner decision 2026-10-26:** migrations run in production too. The review moves from "apply it yourself" to "read
  the generated SQL of the release before deploying" (`docs/Release_Checklist.md`); a migration that drops or rewrites
  data still needs approval and a rollback note before it ships, because nobody will be sitting in front of the database
  when it runs.
- A failed migration leaves the application down (startup fails) rather than serving on an old schema. That is the
  intended trade-off: the deploy is visibly failed instead of quietly inconsistent. A broken migration therefore needs
  the previous build or a forward fix, not a retry.
- EF `MigrateAsync` takes a PostgreSQL advisory lock, so two instances starting at once cannot apply the same migration
  twice; the second waits. On IIS with a single app pool this is a non-issue.
- Backlog candidate: expose "schema behind" through the health check so monitoring sees a skipped automatic run without
  reading logs.
