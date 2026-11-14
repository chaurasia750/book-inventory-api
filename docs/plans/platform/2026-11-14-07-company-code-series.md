# Plan platform#07: Company code series (one shared counter for generated codes)

- **File:** `docs/plans/platform/2026-11-14-07-company-code-series.md`
- **Kind:** Backend
- **Spec:** [../../specs/platform.md](../../specs/platform.md) - rules covered: P23; PAC18
- **Owner:** abhishek
- **Status:** Done
- **Depends on:** `platform#01` (Done), `school#01` (Done)
- **UI plan:** none, it adds no screen or endpoint: it is the mechanism every generated code uses
- **Approved by / on:** abhishek / 2026-11-14 (chat: "approved hai kro implement")
- **Created:** 2026-11-14   **Completed:** 2026-11-14
- **Base branch:** `docs/vendor-master` (cut from it; the spec and plans live there until that docs PR is merged)

## 1. In one minute
After this plan one mechanism hands out `PREFIX-0001`, `PREFIX-0002`, ... per company, and the school master uses it instead of its own counter table. Nothing visible changes: the codes of existing schools stay the same and the next school still gets the next number. The vendor master (vendor#01) is its first new user.

## 2. Touches / leaves out
Touches: `BookInventory.Domain/Core` (the `CompanyCodeSeries` counter), `BookInventory.Application/Core` (a port `ICompanyCodes` with one method that returns the next code for a prefix), `BookInventory.Infrastructure` (the store with the advisory lock moved out of `SchoolStore`, the migration), `BookInventory.Domain/School` and `Application/School` (the school asks the port instead of its own counter; `SchoolCodeCounter` removed), `docs/Code_Map.md`.
Leaves out: financial-year number series for invoices and receipts (BRD `NumberSeries` with a financial year): they come with the first document feature and extend this mechanism; any screen.

## 3. Approach
`CompanyCodeSeries` is a company-owned row per `(company, prefix)` holding the last number; `ICompanyCodes.NextAsync(prefix)` takes the same advisory lock the school store takes today (keyed by company and prefix), increases the row inside the caller's transaction and returns `prefix-NNNN`. The migration creates the new table, copies each company's school counter into it with prefix `SCH`, and drops `school.school_code_counter`. The school's create use case asks the port; its behaviour and codes do not change.

## 4. Tasks (at most 8)
- [x] 1. Domain: `CompanyCodeSeries` (company-owned, last number, prefix) with the format rule (P23).
- [x] 2. Port `ICompanyCodes` and its store with the advisory lock keyed by company and prefix (P23).
- [x] 3. Migration: new table `core.company_code_series`, data copy of the school counters as prefix `SCH`, drop of `school.school_code_counter`; show the SQL and the target database `book_inventory` before applying; rollback note below.
- [x] 4. School uses the port; remove `SchoolCodeCounter` and the counter code of `SchoolStore` (R15 of school unchanged).
- [x] 5. Build at 0 warnings, 0 errors; check size limits; update `docs/Code_Map.md`.
- [x] 6. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `P23`, `PAC18` | 1, 2 | Two companies each create a school: both get `SCH-<next>` from their own series; ten parallel `POST schools` in one company -> ten different codes (SQL `select code ... order by code`) **Observed (2026-11-14, real `book_inventory`):** ABC's next school `SCH-0002`, Children's `SCH-0016`; ten parallel `POST schools` in Children -> ten created, ten different codes `SCH-0017` to `SCH-0026` (26 schools, 26 distinct codes); `Slug Check Co` (no series before) -> first school `SCH-0001`. |
| `P23`, `PAC18` | 3, 4 | Before the migration note the schools and codes of the dev database; after it the same schools keep their codes, the next school gets the next number, `school.school_code_counter` is gone (SQL) **Observed:** before: ABC `SCH-0001`, Children `SCH-0001`..`SCH-0015`, counters 1 and 15; after the migration the same 16 codes, `core.company_code_series` rows (SCH, 1) and (SCH, 15), `to_regclass('school.school_code_counter')` is null. |

## 6. Not verified
- The migration `Core_CompanyCodeSeries` (new table `core.company_code_series`, data copy of the school counters as prefix `SCH`, drop of `school.school_code_counter`) was applied to the local development database `book_inventory` by the app at its start, on the owner's approval of this plan. Its SQL was shown in the session (target `book_inventory` on `localhost:5432`). It is **not** applied to any shared database; the owner applies it there. The `Down` method (recreate the table, copy `SCH` rows back, drop the new table) was not run.
- Two parallel creations from two different companies at once were not exercised; ten in one company were.
- The test data of this check (12 extra schools `P07 ...` in the two companies, 1 in `Slug Check Co`, which was activated and deactivated again) stays in the dev database (B27).

## 7. Risks and notes
The migration changes data (copies counters) and drops a table, so it needs explicit approval and shows its SQL first. Rollback: recreate `school.school_code_counter` from `core.company_code_series` rows with prefix `SCH`, then drop the new table. The advisory lock key changes from company to company-and-prefix; no request holds both at once.
