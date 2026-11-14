# Code map: where to look and what to copy

For the agent and new developers. Every path below exists; copy the neighbouring file of the same kind when adding a new class. Dependencies point one way: `Api -> Infrastructure -> Application -> Domain` (`CLAUDE.project.md`).

## Layers

| Folder | Holds |
|---|---|
| `library/BuildingBlocks/` | Shared, project-neutral base types (Domain, Application, Persistence, Configuration, Logger) |
| `src/BookInventory.Domain/<Module>/` | Aggregates, child entities, enums, domain events |
| `src/BookInventory.Application/<Module>/` | Use cases, ports (interfaces), DTOs, message catalogues |
| `src/BookInventory.Infrastructure/` | EF configurations, stores (port adapters), options, migrations |
| `src/BookInventory.Api/` | `Program.cs`, controllers, hosted services, auth wiring |

Modules present today: `Iam`, `Core` (folders match the schemas `iam`, `core`).

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
| Domain events | `src/BookInventory.Domain/Iam/UserEvents.cs` |
| Message codes | `src/BookInventory.Domain/Iam/IamMessages.cs`, catalogue `src/BookInventory.Application/Iam/IamMessageCatalog.cs` |
| Application service (use case) | `src/BookInventory.Application/Iam/Accounts/UserAccountService.cs` |
| Port implemented in Infrastructure | `src/BookInventory.Application/Iam/Accounts/IAccountStore.cs` with `src/BookInventory.Infrastructure/Iam/AccountStore.cs` |
| EF configuration | `src/BookInventory.Infrastructure/Persistence/Configurations/Core/NotificationConfiguration.cs` |
| Options (`AddValidatedOptions`) | `src/BookInventory.Infrastructure/Iam/Configuration/AuthOptions.cs` |
| Controller | `src/BookInventory.Api/Controllers/Iam/AuthController.cs` |
| Background job | `src/BookInventory.Api/Hosting/PeriodicSweeper.cs`, used by `src/BookInventory.Api/Hosting/ExpiredSessionSweeper.cs` |
| Seed | `src/BookInventory.Api/Hosting/IamSeedHostedService.cs` |

No domain event handler class (`IDomainEventHandler<T>`) exists in `src/` yet; the port is in the library.
