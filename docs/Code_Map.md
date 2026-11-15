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

Modules present today: `Iam`, `Core`, `Company`, `School`, `Vendor`, `Customer`, `Item`, `Price`, `Stock`, `Finance`, `Sales`, `Planning` (folders match the schemas `iam`, `core`, `company`, `school`, `vendor`, `customer`, `item`, `price`, `stock`, `finance`, `sales`, `planning`). The aggregate class `Company` shares its name with its namespace, so files that use it add `using CompanyRoot = BookInventory.Domain.Company.Company;`.

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
| A money calculation (exact decimal, rounded once) | copy `src/BookInventory.Domain/Price/PriceChange.cs` (`Apply` returns the new value or why it was skipped); amounts are `numeric(18,2)` with `HasPrecision(18, 2)` |
| Read another module's items | inject `src/BookInventory.Application/Item/IItemDirectory.cs` (exists/active/stock managed, a selection limited to stock-managed items, codes and names by id, a page of ids for a search and a selection, all ids of a selection up to 5000); never join the item tables (item R16); implementation `src/BookInventory.Application/Item/ItemDirectory.cs` |
| Move stock from a document | inject `src/BookInventory.Application/Stock/IStockPosting.cs` (`PostAsync` a document's lines, `ReverseAsync` on cancel); never write stock entries yourself (stock R11); implementation `src/BookInventory.Application/Stock/StockPosting.cs` |
| Aggregate that appends insert-only children without loading them | `src/BookInventory.Domain/Stock/ItemStock.cs` with `src/BookInventory.Domain/Stock/StockEntry.cs` (company-owned `TransactionDomainBase` child); store `src/BookInventory.Infrastructure/Stock/StockStore.cs` reads children through the root (`SelectMany`) |
| Read an Excel file / write a template | inject `src/BookInventory.Application/Core/ISpreadsheetReader.cs` (first sheet as text cells, header skipped, a row cap) and `src/BookInventory.Application/Core/ISpreadsheetWriter.cs`; adapters `src/BookInventory.Infrastructure/Core/ClosedXmlSpreadsheetReader.cs` and `ClosedXmlSpreadsheetWriter.cs` (the only users of ClosedXML); an all-or-nothing import with a row report: `src/BookInventory.Application/Stock/OpeningStockImportService.cs` and `src/BookInventory.Api/Controllers/Stock/OpeningStockImportController.cs` (multipart, body limit) |
| Render HTML to a PDF | inject `library/BuildingBlocks/Application/Documents/IHtmlToPdfRenderer.cs` (`RenderAsync(html, HtmlToPdfOptions, ct)` -> bytes; A4, margins, a repeating header/footer, `Page x of y`); implementation `src/BookInventory.Infrastructure/Documents/ChromiumHtmlToPdfRenderer.cs` (headless Chrome, else Edge; settings `Documents:Pdf`, an empty `BrowserPath` auto-detects); a failure raises `document.render_failed` (platform P25, PQ2, PQ3) |
| A sales document (amounts, discounts, GST, round-off) | the one rule `src/BookInventory.Domain/Sales/SaleCalculation.cs` with `SaleMoney.cs`; an order `SchoolOrder.cs`, an invoice `Invoice.cs` and its posting `InvoiceLedgerEntry.cs`; stock and ledger posted together in `src/BookInventory.Application/Sales/InvoicePostings.cs` |
| A school's class-wise book list per session (and reading it from another module) | the aggregate `src/BookInventory.Domain/Planning/ClassBookList.cs`; use cases `src/BookInventory.Application/Planning/ClassBookListService.cs`; read capability `IClassBookLists.cs`; sales uses it in `src/BookInventory.Application/Sales/OrderLineAdditionService.cs` |
| Change the lines of a confirmed order | `SchoolOrder.Amend` with the line rules in `src/BookInventory.Domain/Sales/OrderRules.cs`; a refusal names its line through `AppException.Subject` |
| An A4 PDF document (invoice) | the model `src/BookInventory.Application/Sales/InvoicePdfModelBuilder.cs` (pre-formatted, HTML-encoded values), the Scriban templates `src/BookInventory.Application/Sales/Templates/*.sbn` (body, repeating header, footer; embedded resources), the use case `InvoicePdfService.cs` calling `ITemplateRenderer` and `IHtmlToPdfRenderer`; the endpoint `GET invoices/{id}/pdf` in `InvoicesController.cs` |
| The session of a school on a date | inject `src/BookInventory.Application/School/ISchoolSessions.cs` (rule `src/BookInventory.Domain/School/SchoolSession.cs`) |
| A document number per year or session | `ICompanyCodes.NextNumberAsync("INV/2026-27/")` and format it (`Invoice.NumberOf`) |
| Post money to the ledger | inject `src/BookInventory.Application/Finance/ILedgerPosting.cs` with a balanced `LedgerPosting` (`src/BookInventory.Domain/Finance/LedgerPosting.cs`, accounts in `LedgerAccount.cs`); `ReverseAsync` on cancel; never write ledger rows yourself (finance R5); balances: `ILedgerBalances.cs` |
| Read another module's schools | inject `src/BookInventory.Application/School/ISchoolDirectory.cs` (reference by id, references, a class of the school's list); adapter `src/BookInventory.Infrastructure/School/SchoolDirectory.cs` |
| Read another module's vendors | inject `src/BookInventory.Application/Vendor/IVendorDirectory.cs` (exists/active, references with code, names by id, ids matching a text); never join the vendor tables (vendor R15); adapter `src/BookInventory.Infrastructure/Vendor/VendorDirectory.cs` |
| Suggest or read customers | `src/BookInventory.Application/Customer/CustomerQueryService.cs` (`SuggestAsync`, customer R8); the school and class of a customer are checked by `CustomerSchoolGuard` through `ISchoolDirectory` |
| Read the session company's printed identity (logo, signature, one bank account, invoice settings) | inject `src/BookInventory.Application/Company/ICompanyInvoiceIdentity.cs` (`CurrentAsync` -> `CompanyPrintedIdentity`, `null` without a company context); never join the company tables (company R20); implementation `src/BookInventory.Application/Company/CompanyInvoiceIdentity.cs`; the two images are `bytea` on `company.company` (set by the Owner through `src/BookInventory.Api/Controllers/Company/CompaniesController.cs`) |
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

Domain event handler: copy `src/BookInventory.Application/Finance/SchoolOpeningBalanceHandler.cs` (registered by scanning the Application assembly); what a handler writes is saved in the same transaction (platform P24, `BuildingBlocksDbContext.SaveChangesAsync` save rounds with the `OnSavingRound` hook).
