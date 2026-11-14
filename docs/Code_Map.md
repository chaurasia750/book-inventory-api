# Code map: where to look and what to copy

For the agent and new developers. Every path below exists; copy the neighbouring file of the same kind when adding a new class. Dependencies point one way: `Api -> Infrastructure -> Application -> Domain` (`CLAUDE.project.md`).

## Layers

| Folder | Holds |
|---|---|
| `library/BuildingBlocks/` | Shared, project-neutral base types (Domain, Application, Persistence, Configuration, Logger) |
| `src/BookInventory.Domain/<Module>/` | Aggregates, child entities, enums, domain events |
| `src/BookInventory.Application/<Module>/` | Use cases, ports (interfaces), DTOs, message catalogues |
| `src/BookInventory.Infrastructure/` | EF configurations, stores (port adapters), options, migrations |
| `src/BookInventory.Api/` | `src/BookInventory.Api/Program.cs`, controllers, hosted services, auth wiring |

Modules present today: `Iam`, `Core`, `Company`, `School`, `Vendor` (folders match the schemas `iam`, `core`, `company`, `school`, `vendor`). The aggregate class `Company` shares its name with its namespace, so files that use it add `using CompanyRoot = BookInventory.Domain.Company.Company;`.

## Base types of the shared library

| Kind | File |
|---|---|
| Aggregate root | `library/BuildingBlocks/Domain/AggregateRoot.cs` |
| Child entity bases | `library/BuildingBlocks/Domain/DomainBase.cs`, `library/BuildingBlocks/Domain/AuditableDomainBase.cs` |
| Domain event base | `library/BuildingBlocks/Domain/DomainEventBase.cs` |
| Repository and unit of work | `library/BuildingBlocks/Persistence/Contracts/IRepository.cs`, `library/BuildingBlocks/Persistence/Contracts/IUnitOfWork.cs` |
| Entity configuration base | `library/BuildingBlocks/Persistence/Naming/ModuleEntityConfiguration.cs` |
| Controller base | `library/BuildingBlocks/Configuration/Controller/ApiControllerBase.cs` |
| Domain event handler port | `library/BuildingBlocks/Application/IDomainEventHandler.cs` |

## One file to copy per kind

| Kind | Copy this |
|---|---|
| Aggregate root | `src/BookInventory.Domain/Iam/AppUser.cs` (also `src/BookInventory.Domain/Core/Notification.cs`) |
| Aggregate with an append-only child timeline | `src/BookInventory.Domain/Company/Company.cs` with `src/BookInventory.Domain/Company/CompanyStatusHistory.cs` |
| Domain events | `src/BookInventory.Domain/Iam/UserEvents.cs` |
| Audit marks on an entity | `[AuditIgnore]` and `[AuditSecret]` (`src/BookInventory.Domain/Core/AuditIgnoreAttribute.cs`, `src/BookInventory.Domain/Core/AuditSecretAttribute.cs`) used on `src/BookInventory.Domain/Iam/AppUser.cs`; company scope: `src/BookInventory.Domain/Core/IAuditCompanyScope.cs` |
| Generated code `PREFIX-0001` (P23) | inject `src/BookInventory.Application/Core/ICompanyCodes.cs` and call `NextAsync("VEN")`; never keep a counter of your own; copy the use of it in `src/BookInventory.Application/School/SchoolRegistrationService.cs` (adapter: `src/BookInventory.Infrastructure/Core/CompanyCodes.cs`) |
| Company-owned aggregate (P18-P22) | derive from `src/BookInventory.Domain/Core/CompanyOwnedRoot.cs` (a child entity: `src/BookInventory.Domain/Core/CompanyOwnedChild.cs`); copy `src/BookInventory.Domain/School/School.cs` with `src/BookInventory.Domain/School/SchoolClass.cs`; the filter and the stamping are `src/BookInventory.Infrastructure/Persistence/CompanyScope.cs`; a per-company unique index starts with `company_id` (`builder.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique()`) |
| Audit capture (the save override) | `src/BookInventory.Infrastructure/Persistence/AppDbContext.cs` with `src/BookInventory.Infrastructure/Core/AuditCapture.cs` |
| Message codes | `src/BookInventory.Domain/Iam/IamMessages.cs`, catalogue `src/BookInventory.Application/Iam/IamMessageCatalog.cs` |
| Application service (use case) | `src/BookInventory.Application/Iam/Accounts/UserAccountService.cs` |
| Port implemented in Infrastructure | `src/BookInventory.Application/Iam/Accounts/IAccountStore.cs` with `src/BookInventory.Infrastructure/Iam/AccountStore.cs` |
| EF configuration | `src/BookInventory.Infrastructure/Persistence/Configurations/Core/NotificationConfiguration.cs` |
| Options (`AddValidatedOptions`) | `src/BookInventory.Infrastructure/Iam/Configuration/AuthOptions.cs` |
| Controller | `src/BookInventory.Api/Controllers/Iam/AuthController.cs`; role per action: `src/BookInventory.Api/Controllers/Company/CompaniesController.cs` |
| Store port with paging and a whitelisted sort | `src/BookInventory.Application/Company/ICompanyStore.cs` with `src/BookInventory.Infrastructure/Company/CompanyStore.cs` |
| Background job | `src/BookInventory.Api/Hosting/PeriodicSweeper.cs`, used by `src/BookInventory.Api/Hosting/ExpiredSessionSweeper.cs` |
| Seed | `src/BookInventory.Api/Hosting/IamSeedHostedService.cs` |

No domain event handler class (`IDomainEventHandler<T>`) exists in `src/` yet; the port is in the library.
