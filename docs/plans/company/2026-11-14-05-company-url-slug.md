# Plan company#05: The company's URL segment (slug)

- **File:** `docs/plans/company/2026-11-14-05-company-url-slug.md`
- **Kind:** Backend
- **Spec:** [../../specs/company.md](../../specs/company.md) - rules covered: R16; AC16 (and R10, R3)
- **Owner:** abhishek
- **Status:** Done
- **Depends on:** `company#01`, `company#03`
- **UI plan:** platform#05 (the company's screens under `/<slug>/...`)
- **Approved by / on:** abhishek (owner) / 2026-11-14 (chat answers: shape `/<slug>/...`, slug derived from the name with nothing stored, wrong slug redirects)
- **Created:** 2026-11-14   **Completed:** 2026-11-14
- **Base branch:** main (worked on `docs/school-master` by the owner's standing instruction)

## 1. In one minute
After this plan every company has a web-address segment made from its name, `companies/current` returns it, and a name that would give an empty or reserved segment, or the same segment as another company, is refused. Send `POST companies` with `Admin` and see `400`; `GET companies/current` shows `slug`.

## 2. Touches / leaves out
Touches: `BookInventory.Domain/Company` (slug rule, reserved list, message code), `BookInventory.Application/Company` (store port, create and edit checks, `CurrentCompanyResponse`, catalogue text), `BookInventory.Infrastructure/Company/CompanyStore.cs`.
Leaves out: a stored slug column or migration, redirects from an old slug, the UI (platform#05).

## 3. Approach
The slug is computed in one place, `CompanyRules.Slug`, and exposed as `Company.Slug`. The name-taken check compares slugs, so two names that give the same address are one name (it subsumes the old normalised-name check). The reserved list lives next to the rule.

## 4. Tasks (at most 8)
- [x] 1. `CompanyRules.Slug`, reserved list, `RequireSlug` and `Company.Slug` (R16).
- [x] 2. Refuse an empty or reserved slug on create and edit (`company.slug_unavailable`) (R16).
- [x] 3. `IsNameTakenAsync` compares slugs; create and edit use it (R3, R16).
- [x] 4. `CurrentCompanyResponse` carries `Slug` (R10, R16).
- [x] 5. Build at 0 warnings, 0 errors; verify AC16 on the real local database.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R16`, `AC16` | 1, 2 | `POST companies` with `Admin` and with `!!` -> `400` `company.slug_unavailable` **Observed (2026-11-14, real `book_inventory`, API on `http://localhost:5100`):** both `400` with that code. |
| `R16`, `R3`, `AC16` | 3 | `Slug Check Co` -> `201`, then `SLUG-check  co` -> `409` `company.name_taken` **Observed:** `201` (Owner `slugcheckco`), then `409` `company.name_taken`. |
| `R10`, `R16`, `AC16` | 4 | Super Admin enters `Slug Check Co`, `GET companies/current` **Observed:** `company.slug` = `slug-check-co`. Rename of that company to `Login` -> `400` `company.slug_unavailable`. |

## 6. Not verified
- The existing companies were not renamed; their slugs (`abc-book-center`, `children-book-center`) were not read back through the API in this check.
- The test company `Slug Check Co` stays in the local development database, deactivated (no company delete exists); it adds to the test data of B27.

## 7. Risks and notes
A rename changes the address and the old one stops working (the owner chose this: D of R16). Two names that differ only in punctuation or case are now the same name.
