# Plan sales#05: Wholesale orders: editable confirmed orders, class lines, class sets, bulk add

- **File:** `docs/plans/sales/2026-11-15-05-wholesale-orders.md`
- **Kind:** Backend
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R20-R23, R25, R26 (data and the school default); AC12-AC17
- **Owner:** abhishek
- **Status:** Done (2026-11-15)
- **Base branch:** `docs/orders-wholesale` (branch `feature/planning-01`, on the owner's instruction: planning#01 and sales#05 on one branch)
- **Depends on:** `sales#01` (Done), `planning#01`
- **UI plan:** sales#06
- **Approved by / on:** abhishek / 2026-11-15 (chat: the wholesale order questions answered one by one, all with the recommended answers)
- **Created:** 2026-11-15

## 1. In one minute
After this plan a confirmed order can still be changed above what is invoiced, every line can carry its class, whole class sets are added from the class book lists with students x per-student quantities, and many items are added at once. Try it: `POST /api/v1/school-orders/{id}/class-sets` with `[{className: "5th", students: 40}]`.

## 2. Touches / leaves out
Touches: `Domain/Sales` (the order's change rules for confirmed orders, the line class, adding lines in bulk), `Application/Sales` (use cases for class sets and bulk lines, through `IClassBookLists`, `IPriceDirectory`, `IItemDirectory`), persistence (line class column; unique key with class), migration, controller, catalogue.
Leaves out: screens (sales#06), class-wise invoices. Schema change: yes (nullable class columns on order and invoice lines, a small per-school layout table; additive).

## 3. Approach
`SchoolOrder.Replace` accepts a confirmed order under the R20 limits (the order checks each line against its invoiced quantity and version), so one PUT covers every edit. Class sets and bulk add are use cases that build line inputs and call the same order behaviour that merges by item, version and class (R21). Refusals name the line (R25).

## 4. Tasks (at most 8)
- [x] 1. Domain: line class, merge by item+version+class, confirmed-order change rules (R20, R21, R25).
- [x] 2. Persistence: class column, migration; show the SQL and target before applying.
- [x] 3. Use case: add class sets from the planning lists (R22).
- [x] 4. Use case: bulk add (R23).
- [x] 5. Invoice line keeps the class; the school's invoice layout default (`GET/PUT schools/{id}/invoice-layout`, Owner) and the invoice read in either layout with the R26 merge (R26).
- [x] 6. Controller endpoints and catalogue; build at 0 warnings.
- [x] 7. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R20`, `AC12` | 1 | PASS. Draft order A 10 + B 5 (ITM-0014, ITM-0017), confirmed, invoice for A 4. PUT with line ids: A -> 6 = 204 and grand total 1523.00 -> 1139.00 (subtotal equals the sum of the line amounts); A -> 3 = 400 `sales.quantity_below_invoiced`, subject `ITM-0014`; A left out = 409 `sales.line_invoiced`; C added = 204, C left out = 204; A on another price version = 409 `sales.line_invoiced`; B on another version = 204; A -> 4 (= invoiced) and B left out -> status CLOSED. Original plan row: | Confirmed order, A 10 with 4 invoiced: A->6 ok, A->3 400, delete A 409, add/delete C ok, A version 409, B version ok, totals recomputed, all remaining to zero closes |
| `R21`, `AC13` | 1 | PASS. A 40 for 4th and A 38 for 5th -> two lines with their classes; a line with class `13th` -> 400 `sales.invalid_class`, subject `13th`. Original plan row: | Class 4 and Class 5 dictionary as two lines; unknown class 400 |
| `R22`, `AC14` | 3 | PASS. Lists 4th (A 1, B 2), 5th (A 1, C 1), 6th (ITM-0001 without price 1, D 1). POST `class-sets` 4th 45 + 5th 40 -> A 45, B 90 (4th), A 40, C 40 (5th); 5th 40 again -> A 80, C 80, still 4 lines; 6th 10 -> D 10 added and ITM-0001 left out `sales.no_price`; on the confirmed order 4th 5 -> A 4th 50. Original plan row: | Class sets 4 (45) and 5 (40) -> X 45, Y 90, X 40, Z 40; Class 5 again -> 80; no-price book left out and reported |
| `R23`, `AC15` | 4 | PASS. POST `bulk-lines` 12 priced items, quantity 30, class 6th -> 12 lines, each 30 and 6th. Original plan row: | 12 items x 30 for Class 6 -> 12 lines |
| `R26`, `AC17` | 5 | PASS. Invoice of A 40 (4th), A 38 (5th), C 38 (5th): default `BOOK_LIST`; `?layout=BY_CLASS` -> 4th [A 40], 5th [A 38, C 38]; `?layout=BOOK_LIST` -> A 78 and C 38 without classes; grand total 11763.00 in both and equal group subtotals; PUT school default `BY_CLASS` -> the invoice opens by class; Staff PUT -> 403. Original plan row: | Invoice with Class 4 dictionary 40, Class 5 dictionary 38 and atlas 38: by class -> two groups with subtotals; book list -> dictionary 78, atlas 38; same grand total; default by class opens by class; Staff PUT default 403 |
| `R25`, `AC16` | 1 | PASS. Refusals return the problem details `subject`: `ITM-0014` (below invoiced), `ITM-0014 / 4th` (class line), `13th` (unknown class). Original plan row: |
| `R27`, `AC18` | UI plan | PASS (server side): POST `school-orders` with only the school -> 201 DRAFT, order date 2026-11-15, session 2026-27, GST choice = the school default (false). The screen is sales#06. Original plan row: | A refused save names the item code and class |

## 6. Not verified
Run: 23 of 23 checks passed on the real local database `book_inventory` (scratch script, not committed); invoices made by the run were cancelled and the test lists and draft orders removed. The planning#01 checks were re-run after the changes: 19 of 19.

NOT verified: (1) the total discount / GST lock of R20 (`sales.totals_locked`) once something is invoiced; (2) an invoice cancel giving a quantity back to an amended order; (3) a rupee item discount on an amended line with partial invoicing; (4) the older sales checks (35 of 35 earlier) were not re-run: that script needs a scratch-only endpoint that is not in the repository; the new run covers confirm, invoice, cancel and the money totals of the changed paths.

Deviations to confirm: `AppException.Subject` (library, commit f8e902a) was added so that a refusal can name its line (R25); it is additive and belongs in its own PR. R25's `subject` is only on the refusals of the order lines; others keep the code only.

## 7. Risks and notes
Additive migration `Sales_WholesaleOrders` (two nullable `class_name` columns and the table `sales.school_invoice_layout`) applied to the local `book_inventory` after its SQL was shown; rollback: drop the two columns and the table. Money: the recomputation is the same R7 rule. `SchoolOrder.cs` was kept at 300 lines by moving its value rules to `OrderRules.cs`.
