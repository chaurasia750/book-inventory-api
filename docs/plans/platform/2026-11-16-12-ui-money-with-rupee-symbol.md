# Plan platform#12: Rupee symbol on every screen of the member app

- **File:** `docs/plans/platform/2026-11-16-12-ui-money-with-rupee-symbol.md`
- **Kind:** UI
- **Spec:** [../../specs/platform.md](../../specs/platform.md) - rules covered: P26, PAC24
- **Owner:** abhishek
- **Status:** Done (2026-11-16) - built and checked by build, lint and search; not seen in a browser (Backlog B146)
- **Depends on:** none (the API is unchanged: it answers plain numbers)
- **UI plan:** (this is the UI plan; the PDFs are sales#30, payment#12, finance#12, purchase#06)
- **Approved by / on:** abhishek / 2026-11-16 (chat: "jo plan hai wo approved hai impmemnent kro")
- **Created:** 2026-11-16   **Completed:** 2026-11-16

## 1. In one minute
After this plan every amount on a screen of the member app (the dashboard, lists, details, ledger, payments, returns, prices, counters) shows the rupee symbol by one rule: `₹ 1,200.00` when it stands alone, `(₹)` in a money column heading, a `₹` prefix inside a money input. See it: open the dashboard, a school's Ledger tab and an invoice.

## 2. Touches / leaves out
Touches (repository `book-inventory-ui`; checks `npx nx run-many -t lint` and `-t build`): a shared money pipe next to the existing `inr` pipe, `assets/i18n/en.json` (column heading texts) and the screens that show money: dashboard, counters, sales (order editor, invoice view, totals, item entry, bulk picker, school documents), returns and purchase returns, payment, ledger and outstanding, schools, vendors, customers, prices, items (about 40 files; 19 uses of the `inr` pipe and about 110 of `number:'1.2-2'`).
Leaves out: the API, the PDFs, quantities, counts and percentages, the amount in words, text and logo sizes. `book-inventory-ui` has uncommitted changes of other work: only the lines of this plan are touched.

## 3. Approach
One shared formatter (P26 f): the existing `inr` pipe keeps writing the plain Indian number for table cells; a new `money` pipe writes `₹ 1,200.00` (and `-₹ 1,200.00`) for amounts standing alone. Each use of `number:'1.2-2'` is judged by what it shows: money becomes `inr` or `money`, a quantity or a percentage stays. Column headings get `(₹)` in the catalogue text (U5). A money input gets the prefix through one shared way, not a symbol typed per screen. The work follows the screens in groups, so each group is built and looked at before the next.

## 4. Tasks (at most 8)
- [x] 1. The `money` pipe (symbol, space, Indian grouping, two decimals, negative as `-₹ x`) next to `inr`, and the shared money-input prefix.
- [x] 2. Dashboard and counters.
- [x] 3. Sales: order editor, invoice view, totals, item entry, bulk picker, school documents.
- [x] 4. Returns and purchase returns.
- [x] 5. Payment screens.
- [x] 6. Ledger, outstanding, schools, vendors, customers (balances, lists and details).
- [x] 7. Prices and items.
- [x] 8. Lint and build of the member app, then look at each group at 1280 px and 375 px and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `P26` a, d, f; `PAC24` | 1-7 | a standing amount shows `₹ 1,200.00` and a negative `-₹ 1,200.00`; a search of the screens finds no `number:'1.2-2'` left on a money value |
| `P26` b; `PAC24` | 2-7 | every money column heading ends with `(₹)`; a quantity column has none |
| `P26` c; `PAC24` | 1, 3, 5, 7 | a money input (receive payment, price, counter bill, refund) shows `₹` in the field |
| `P26` e; `PAC24` | 2-7 | quantities, GST percentages, counts and the amount in words show no symbol |
| `PAC24`, U18 | 8 | at 375 px the headings with `(₹)` and the `₹ ` amounts do not make a page or a table box scroll sideways |

## 6. Not verified
Observed (2026-11-16, repository `book-inventory-ui`):
- Build `npx nx build member --configuration=development --base-href=/member/`: succeeded, no warnings. `npx nx lint member`: 3 errors, all older and in files this plan did not touch (`member-layout.ts`, `account-pages.ts`, `change-password.ts`: static import of a lazy-loaded library), none new.
- A search of the member app finds no `number: '1.2-2'` left except one non-money one (`order-editor.html`: a line discount that is a percentage or an amount) and no typed `₹` or `&#8377;` next to a value; `money` is used 69 times for amounts standing alone, `inr` 58 times for table cells (Indian grouping, no symbol). The three typed `&#8377;` of the counter day summary were removed so the symbol is not doubled.
- Money column headings: 12 catalogue texts in `assets/i18n/en.json` now end with `(₹)` (Amount, Total, Balance, Debit, Credit, Due, MRP, Sale price, Purchase price, Total GST, a return line's Rate) and 12 files got `(₹)` after a heading whose text is shared with a non-heading (Rate, Discount, Amount, Taxable value, MRP, CGST, SGST, IGST).
- Money inputs: 15 amount inputs (receive payment, advance adjustment, refund, counter bill cash and UPI, price versions, opening balances of school, vendor and customer, customer credit limit, return rates) are wrapped in the new `app-money-prefix`, which shows `₹` inside the field.
- NOT seen: no screen was opened in a browser (no logged-in browser could be driven in this environment), so the look of the `₹` prefix inside the fields, the headings at 375 px (U18) and the dark theme are unchecked, and the heading of a table whose columns use dynamic keys (for example the schools list Balance column) was not searched.

## 7. Risks and notes
- About 130 places: a quantity wrongly given a symbol is the main risk, so each use is judged by what it shows, not by its pipe.
- The Dr/Cr letter of a ledger balance stays (finance R21).
