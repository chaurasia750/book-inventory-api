# Book Inventory API

.NET 10 monolith API on PostgreSQL for the **Book Inventory, Billing & Ledger System**. It started as a copy of the HautInHousing API; the identity (`iam`), notification (`core`) and hosting (`platform`) parts are carried over, and every new feature follows the spec -> plan -> approval -> code flow in `CLAUDE.md`.

## Layout

| Path | What it holds |
|---|---|
| `library/BuildingBlocks/` | Shared building blocks (see its own [README](library/BuildingBlocks/README.md)) |
| `src/BookInventory.Domain` | Entities, enums, domain events |
| `src/BookInventory.Application` | Use cases, services, DTOs, event handlers |
| `src/BookInventory.Infrastructure` | `AppDbContext`, EF configurations, migrations, integrations |
| `src/BookInventory.Api` | `Program.cs`, controllers, appsettings (composition root) |
| `docs/` | Index in [docs/README.md](docs/README.md), specs, plans, backlog |

## Build

```
dotnet build book-inventory.slnx
```

The UI lives in the sibling repository `book-inventory-ui`. Local setup (database `book_inventory`, user-secrets) is described in `CLAUDE.project.md`.
