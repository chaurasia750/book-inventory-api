# Backlog: things not verified or left open

Every item a plan could **not verify**, and every known gap or follow-up a review leaves open, is added here when that plan is
closed. The whole list is reviewed **after all planned work is done** (each item then becomes a check, a fix in a new plan, an
accepted limit, or is dropped). Status: `open` until reviewed.

Kinds: **NV** not verified · **GAP** known gap · **DEC** decision the owner has to take · **DEBT** cleanup.

| # | Module | Kind | Item | Source | Status |
| --- | --- | --- | --- | --- | --- |
| B1 | iam | DEC | Migration `Iam_CompanyAccounts` (two nullable columns, two indexes, wider role, login-result and logout-reason check constraints; additive) was applied only to a throw-away database. The SQL was shown; the owner has not confirmed it yet. The app applies it to `book_inventory` at its next start | iam#01 | open |
| B2 | iam | NV | The capabilities (Owner creation, session revocation, entered company) were driven by a temporary probe controller and a fake `ICompanyStatusReader`; the real callers and the real status reader arrive with company#01 and company#03 | iam#01 | open |
| B3 | iam | NV | Two parallel creations of the same Owner user id (unique index as the guard) were not exercised | iam#01 | open |
| B4 | iam | GAP | The user id of a company account is not yet visible in any UI; role routing, the inactive-company message and the company name in the header are iam#02 | iam#01 | open |
| B5 | iam | DEC | iam#01 was built on `docs/company-accounts` (the owner's instruction), not on `feature/iam-01`; the close step stopped before push and PR because the branch does not match the plan. Decide how the branch is pushed and reviewed | iam#01 | open |
| B6 | iam | DEBT | `AppUser.cs` is over 300 lines (about 347, partly before this plan); company behaviour went into the new partial `AppUser.Company.cs`. Split the aggregate further when it is next touched | iam#01 | open |
| B7 | company | DEC | Migration `Company_Initial` (new schema `company`, tables `company` and `company_status_history`, indexes, check constraints; additive) was applied only to a throw-away database. The SQL was shown; the owner has not confirmed it yet. The app applies it to `book_inventory` at its next start | company#01 | open |
| B8 | company | NV | Two parallel creations with the same company name or the same generated user id (unique indexes as the guards) were not exercised | company#01 | open |
| B9 | company | GAP | R13 (a Super Admin enters a company), R14 (Staff) and the screens are later plans (company#03, iam#03, company#02, company#04) | company#01 | open |
| B10 | company | DEBT | The aggregate class `Company` shares its name with the namespace `BookInventory.Domain.Company`, so files that use it need a `CompanyRoot` alias; consider the namespace name `Companies` in a later cleanup | company#01 | open |
| B11 | iam | DEC | Migration `Iam_StaffAccounts` (`app_user.user_name` widened to 64, new table `iam.user_password_reset`; additive) was applied only to throw-away databases. The SQL was shown; the owner has not confirmed it yet. The app applies it to `book_inventory` at its next start | iam#03 | open |
| B12 | iam | NV | Reset by mail (R17) for a company account that has an e-mail, and two parallel creations of the same Staff id, were not exercised | iam#03 | open |
| B13 | iam | GAP | What Staff may do in business modules (create orders and sell to customers, company Q14) is decided when those endpoints are built; today Staff can call only self-service and `companies/current` | iam#03 | open |
| B14 | company | DEC | Migration `Company_Entries` (insert-only table `company.company_entry`; additive) was applied only to a throw-away database. The SQL was shown; the owner has not confirmed it yet. Also confirm decision D10 (the enter and leave record lives in the company module until the `core` audit trail exists) | company#03 | open |
| B15 | company | GAP | A Super Admin session keeps an entered company that is deactivated later; the business modules must refuse writes to an inactive company, and a check of the entered company on use may be needed | company#03 | open |
| B16 | company | NV | "Work with its data" is proven only through `companies/current`, because no company-owned business endpoint exists yet | company#03 | open |
| B17 | company | NV | The four UI plans (iam#02, iam#04, company#02, company#04) are built, build and lint clean, but no screen was opened in a browser (no browser tool in the session): the plans stay `In progress` until each screen is checked against the running API | UI plans | open |
| B18 | iam | GAP | The Super Admin's user detail page has no action for the temporary password of a company account (`POST users/{id}/temporary-password`, iam R67); only the Owner's Staff screen has it. Needs a small UI plan | iam#04 | open |
| B19 | iam | DEBT | `app-header` and `app-layout` gained a content slot (`headerExtra`) and `libs/ui` a shared `CredentialsDialogComponent`; both are outside the plans' "Touches" lists and should be reviewed | company#04, company#02 | open |
| B20 | core | DEC | Migration `Core_AuditEntry` (new insert-only table `core.audit_entry`, three indexes; additive) was applied only to throw-away databases. The SQL was shown; the owner has not confirmed it yet. Also confirm decision AD5 (an anonymous request is recorded as "System") | core#01 | open |
| B21 | core | GAP | A6 at database level: the application role should have no `UPDATE` or `DELETE` on `core.audit_entry`; this is a deployment step (the check ran as a superuser) | core#01 | open |
| B22 | core | NV | The suspension-lift job and the load behaviour of the audit table (size, retention, partitioning) were not exercised; the `iam#03` and `company#03` checks were not repeated after the save override | core#01 | open |
