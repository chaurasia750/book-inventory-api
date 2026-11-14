# Plan core#02: UI - the audit screen for the Super Admin and the Owner

- **File:** `docs/plans/core/2026-11-14-02-ui-audit-screen.md`
- **Kind:** UI
- **Spec:** [../../specs/core.md](../../specs/core.md) - rules covered (screens): A7, A8, A9; AAC6, AAC8, AAC9
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `core#01`
- **UI plan:** - (this is the UI plan)
- **Approved by / on:** abhishek (owner) / 2026-11-14
- **Created:** 2026-11-14   **Completed:** -

## 1. In one minute
The audit screen that already exists in the `admin` app (Audit log, Super Admin) now matches the API: it filters by company and by date range and opens an entry to show the old and new value field by field. The Owner gets the same screen in the company workspace (`member` app), limited to their own company. Repository: `book-inventory-ui` (build: `npx nx run-many -t build`, lint: `npx nx run-many -t lint`).

## 2. Touches / leaves out
Touches: the existing audit page in `apps/admin` (moved into a shared screen under `libs/layout`, the way the account screens are shared), `apps/member` (route and menu item for the Owner), `libs/shared-models` and `libs/api-services` (`companyId` filter, the entry type), the i18n catalogue. Endpoints: `GET audit`, `GET audit/entity-types`. Leaves out: export (AQ5), human wording per entity (AQ4), any change to the API.

## 3. Approach
One shared screen component takes a flag for the Super Admin (shows the company filter, which uses `GET companies`) and is otherwise identical for the Owner; the server ignores an Owner's `companyId`, so the UI never offers one. The detail dialog shows old and new value as a table of changed fields plus the raw JSON. States for the list and the detail: loading, empty, error with retry, success.

## 4. Tasks (at most 8)
- [ ] 1. Move the admin audit page into a shared screen in `libs/layout`; keep the admin route and menu item working.
- [ ] 2. Models and `CoreAuditService`: `companyId` filter, the entry's company, date filters in the business time zone (A8).
- [ ] 3. Super Admin filter by company (reads `GET companies`) and the date range.
- [ ] 4. Entry detail dialog: changed fields table and raw JSON (AQ4).
- [ ] 5. `member` app: route and menu item "Audit log" for `OWNER` only.
- [ ] 6. i18n keys (no hard-coded text); `npx nx run-many -t build` and `-t lint`: 0 new findings against `main`.
- [ ] 7. Verify every row of section 5 against the running API and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (what was seen) |
|---|---|---|
| `A7`, `AAC6` | 1, 3, 5 | Super Admin sees all companies and can filter by one; Owner sees only the own company and has no company filter; Staff has no menu item and a direct URL gives the unauthorized page |
| `A8`, `AAC8` | 2, 3 | Newest first, paging, entity-type filter, date range |
| `A9`, `AAC9` | 1, 4 | Loading, empty, error with retry and success are visible; opening an entry shows old and new value |

## 6. Not verified
To be filled when the plan is closed (mobile widths and other browsers are not part of this plan).

## 7. Risks and notes
Lint findings in files this plan does not touch are not part of it. The existing page was written against the API contract of A8 before the API existed; differences found while moving it are fixed here, not in the API.
