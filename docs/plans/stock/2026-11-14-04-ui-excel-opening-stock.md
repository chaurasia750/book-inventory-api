# Plan stock#04: UI Excel opening stock

- **File:** `docs/plans/stock/2026-11-14-04-ui-excel-opening-stock.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/stock.md](../../specs/stock.md) - rules covered: R17, R18 (the screen side); AC9, AC10
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `stock#03`, `stock#02`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-14 (chat: the open questions answered one by one)
- **Created:** 2026-11-14

## 1. In one minute
After this plan the Owner has an "Import opening stock" button on the stock list: a dialog to download the template, choose the `.xlsx` file and the opening date, and upload; afterwards it shows either how many openings were imported or a table of every wrong row with its reason. Open `/<company>/stock`.

## 2. Touches / leaves out
Touches: `libs/shared-models`, `libs/api-services` (`StockService` template download and upload), `apps/member` (the import dialog under `pages/stock`, the button on the stock list for the Owner), the three `en.json` catalogues.
Leaves out: imports of other masters, the Super Admin's screens.

## 3. Approach
A dialog with three parts: "Download template", the file input (accepting `.xlsx` only, size checked before upload) with the opening date (both required, `*`), and the result: a success message with the count, or the report table (row number, column, the catalogue text of the reason). The list reloads after a success. Nothing is computed on the page; the report is the server's.

## 4. Tasks (at most 8)
- [ ] 1. `StockService` template download and upload (multipart) and the report models.
- [ ] 2. The import dialog: template link, file and date inputs, upload with a busy state (R17).
- [ ] 3. The result view: count on success, report table of wrong rows on refusal (R18).
- [ ] 4. The button on the stock list for the Owner; i18n keys.
- [ ] 5. `npx nx run-many -t build` and `-t lint`: 0 new findings against the base; verify every row of section 5 against the running API and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R17`, `AC9` | 2, 3 | Download the template, fill three rows, upload with a date: the count shows and the stock list has the openings |
| `R18`, `AC10` | 3 | Upload a file with wrong rows: the report lists each with its reason; the stock list is unchanged |
| `R17` | 2 | A `.csv` or a too-large file is refused before or by the upload with a message |

## 6. Not verified
To be filled when the plan is closed (no browser tool for the assistant: the owner looks at the screens).

## 7. Risks and notes
Written without a browser; expect small visual fixes after the owner's first look.
