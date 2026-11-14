# Plan core#02: UI - the audit screen for the Super Admin and the Owner

- **File:** `docs/plans/core/2026-11-14-02-ui-audit-screen.md`
- **Kind:** UI
- **Spec:** [../../specs/core.md](../../specs/core.md) - rules covered (screens): A7, A8, A9; AAC6, AAC8, AAC9
- **Owner:** abhishek
- **Status:** In progress
- **Depends on:** `core#01`
- **UI plan:** - (this is the UI plan)
- **Approved by / on:** abhishek (owner) / 2026-11-14
- **Created:** 2026-11-14   **Completed:** -
- **Base branch:** `feature/company-accounts-ui` of `book-inventory-ui` (worked on `feature/audit-screen-ui`; the plan files live in this API repository)

## 1. In one minute
The audit screen that already exists in the `admin` app (Audit log, Super Admin) now matches the API: it filters by company and by date range and opens an entry to show the old and new value field by field. The Owner gets the same screen in the company workspace (`member` app), limited to their own company. Repository: `book-inventory-ui` (build: `npx nx run-many -t build`, lint: `npx nx run-many -t lint`).

## 2. Touches / leaves out
Touches: the existing audit page in `apps/admin` (moved into a shared screen under `libs/layout`, the way the account screens are shared), `apps/member` (route and menu item for the Owner), `libs/shared-models` and `libs/api-services` (`companyId` filter, the entry type), the i18n catalogue. Endpoints: `GET audit`, `GET audit/entity-types`. Leaves out: export (AQ5), human wording per entity (AQ4), any change to the API.

## 3. Approach
One shared screen component takes a flag for the Super Admin (shows the company filter, which uses `GET companies`) and is otherwise identical for the Owner; the server ignores an Owner's `companyId`, so the UI never offers one. The detail dialog shows old and new value as a table of changed fields plus the raw JSON. States for the list and the detail: loading, empty, error with retry, success.

## 4. Tasks (at most 8)
- [x] 1. Move the admin audit page into a shared screen in `libs/layout`; keep the admin route and menu item working.
- [x] 2. Models and `CoreAuditService`: `companyId` filter, the entry's company, date filters in the business time zone (A8).
- [x] 3. Super Admin filter by company (reads `GET companies`) and the date range.
- [x] 4. Entry detail dialog: changed fields table and raw JSON (AQ4).
- [x] 5. `member` app: route and menu item "Audit log" for `OWNER` only.
- [x] 6. i18n keys (no hard-coded text); `npx nx run-many -t build` and `-t lint`: 0 new findings against `main`.
- [ ] 7. Verify every row of section 5 against the running API and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (what was seen) |
|---|---|---|
| `A7`, `AAC6` | 1, 3, 5 | Super Admin sees all companies and can filter by one; Owner sees only the own company and has no company filter; Staff has no menu item and a direct URL gives the unauthorized page **Built:** the admin page moved into `AuditLogScreenComponent` (`libs/layout`), used by the admin route and a new `member` route `audit` guarded for `OWNER`, with a menu item for the Owner; the company filter and column are rendered only for the Super Admin. **NOT seen:** the screen was not opened in a browser. |
| `A8`, `AAC8` | 2, 3 | Newest first, paging, entity-type filter, date range **Built:** the existing filters (entity type, entity id, user, date range, paging) now also send `companyId` for the Super Admin; the model gained `companyId`. **NOT seen:** the screen was not opened in a browser. |
| `A9`, `AAC9` | 1, 4 | Loading, empty, error with retry and success are visible; opening an entry shows old and new value **Built:** the existing loading, empty, error-with-retry and success states and the old/new comparison are kept, and the detail also shows the raw JSON. **NOT seen:** the screen was not opened in a browser. |

## 6. Not verified
**Checked (2026-11-14):** `npx nx run-many -t build` -> all three apps build; `npx nx run-many -t lint` -> no finding in the new shared screen or any file this plan changed (the `Static imports of lazy-loaded libraries` errors in the two layouts are identical on `main`); `GET audit` and `GET audit/entity-types` with these filters were exercised against the running API in core#01. **NOT seen:** the screen was not opened in a browser (no browser tool in this session).

Still open before this plan can be closed: task 7 (open the screen as Super Admin and as Owner against the running API and record what was seen).

## 7. Risks and notes
Lint findings in files this plan does not touch are not part of it. The existing page was written against the API contract of A8 before the API existed; differences found while moving it are fixed here, not in the API.
