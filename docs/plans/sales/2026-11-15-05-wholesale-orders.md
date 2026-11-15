# Plan sales#05: Wholesale orders: editable confirmed orders, class lines, class sets, bulk add

- **File:** `docs/plans/sales/2026-11-15-05-wholesale-orders.md`
- **Kind:** Backend
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R20-R23, R25; AC12-AC16
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `sales#01` (Done), `planning#01`
- **UI plan:** sales#06
- **Approved by / on:** abhishek / 2026-11-15 (chat: the wholesale order questions answered one by one, all with the recommended answers)
- **Created:** 2026-11-15

## 1. In one minute
After this plan a confirmed order can still be changed above what is invoiced, every line can carry its class, whole class sets are added from the class book lists with students x per-student quantities, and many items are added at once. Try it: `POST /api/v1/school-orders/{id}/class-sets` with `[{className: "5th", students: 40}]`.

## 2. Touches / leaves out
Touches: `Domain/Sales` (the order's change rules for confirmed orders, the line class, adding lines in bulk), `Application/Sales` (use cases for class sets and bulk lines, through `IClassBookLists`, `IPriceDirectory`, `IItemDirectory`), persistence (line class column; unique key with class), migration, controller, catalogue.
Leaves out: screens (sales#06), class-wise invoices. Schema change: yes (one nullable column, additive).

## 3. Approach
`SchoolOrder.Replace` accepts a confirmed order under the R20 limits (the order checks each line against its invoiced quantity and version), so one PUT covers every edit. Class sets and bulk add are use cases that build line inputs and call the same order behaviour that merges by item, version and class (R21). Refusals name the line (R25).

## 4. Tasks (at most 8)
- [ ] 1. Domain: line class, merge by item+version+class, confirmed-order change rules (R20, R21, R25).
- [ ] 2. Persistence: class column, migration; show the SQL and target before applying.
- [ ] 3. Use case: add class sets from the planning lists (R22).
- [ ] 4. Use case: bulk add (R23).
- [ ] 5. Controller endpoints and catalogue; build at 0 warnings.
- [ ] 6. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R20`, `AC12` | 1 | Confirmed order, A 10 with 4 invoiced: A->6 ok, A->3 400, delete A 409, add/delete C ok, A version 409, B version ok, totals recomputed, all remaining to zero closes |
| `R21`, `AC13` | 1 | Class 4 and Class 5 dictionary as two lines; unknown class 400 |
| `R22`, `AC14` | 3 | Class sets 4 (45) and 5 (40) -> X 45, Y 90, X 40, Z 40; Class 5 again -> 80; no-price book left out and reported |
| `R23`, `AC15` | 4 | 12 items x 30 for Class 6 -> 12 lines |
| `R25`, `AC16` | 1 | A refused save names the item code and class |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Additive migration. Money: the recomputation is the same R7 rule, already verified.
