# Plan item#06: UI Excel import of items

- **File:** `docs/plans/item/2026-11-15-06-ui-excel-import.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/item.md](../../specs/item.md) - rules covered: R18-R22 (the screen side); AC14, AC15
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `item#05`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-15 (chat: Bulk Upload option asked; Q13-Q15 answered with the recommended answers)
- **Created:** 2026-11-15

## 1. In one minute
After this plan the item list has an **Import** button (Owner): it opens a dialog with the template download, the file picker and the result: how many were created and updated, or the table of wrong rows (row, column, reason). Open Items > Item list > Import.

## 2. Touches / leaves out
Touches: `apps/member` (`pages/items`: the import dialog, the list's Import button), the service, models, i18n.
Leaves out: schools, vendors, prices.

## 3. Approach
The dialog follows the stock import dialog: download the template, choose an `.xlsx`, upload, then show the success counts or the report table; a wrong file or an over-size file shows the server's message; the list reloads after a successful import. Staff do not see the button.

## 4. Tasks (at most 8)
- [ ] 1. Service and models: template download, upload, result and report.
- [ ] 2. The import dialog (file choice, loading, success counts, report table, error).
- [ ] 3. The Import button on the item list for the Owner; the list reloads after an import.
- [ ] 4. i18n; build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R18`-`R22`, `AC14`, `AC15` | 1-3 | Owner downloads the template, uploads a good file (the counts show, the list has the new books) and a file with wrong rows (the table lists them, nothing changed); Staff has no Import button |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Written without a browser.
