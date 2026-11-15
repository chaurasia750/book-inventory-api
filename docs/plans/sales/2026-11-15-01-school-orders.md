# Plan sales#01: School orders

- **File:** `docs/plans/sales/2026-11-15-01-school-orders.md`
- **Kind:** Backend
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R1-R7, R13-R18; AC1-AC4, AC10
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `school#05`, `price#05`, `stock#05`, `company#06`
- **UI plan:** sales#02
- **Approved by / on:** abhishek / 2026-11-15 (chat: sales Q1-Q14 answered one by one)
- **Created:** 2026-11-15

## 1. In one minute
After this plan a school order can be drafted, changed, confirmed, closed or deleted, with prices from the chosen price versions, item and total discounts, GST and round-off computed by one rule, and the low-stock warning per line. Try it: `POST /api/v1/school-orders` with a school and lines, then `GET` it.

## 2. Touches / leaves out
Touches: new `BookInventory.Domain/Sales` (the amounts rule of R7 as a domain service used by order and invoice, `SchoolOrder` with its lines, events, message codes), `Application/Sales` (use cases and queries), `Infrastructure` (schema `sales`, configurations, migration), `Api/Controllers/Sales`, `CLAUDE.project.md` (module `sales`, own commit), `docs/Code_Map.md`.
Leaves out: invoices (sales#03), screens (sales#02). Schema change: yes (new schema `sales`, additive).

## 3. Approach
`SchoolOrder` is a company-owned root with its lines; its behaviours (replace lines, confirm, close) enforce R2-R6 and call the R7 amounts rule (a pure domain service, half-away-from-zero rounding), so the computed totals are stored with the order. The use case resolves the school and its session (school R16, R18), the items (item R16), the price versions (price R18), the stock quantities (stock R20) and the company state (company R16) before calling the root. The number comes from the code series with the prefix `ORD/<session>/` (P23).

## 4. Tasks (at most 8)
- [ ] 1. Domain: the R7 amounts rule with its rounding (checked against AC2's worked numbers).
- [ ] 2. Domain: `SchoolOrder` and lines, lifecycle, events, message codes (R2, R3, R6).
- [ ] 3. Persistence: schema `sales`, configurations, migration; module list in `CLAUDE.project.md` (own commit); show the SQL and target before applying.
- [ ] 4. Use cases: create, replace, delete draft, confirm, close (R2-R6, R14).
- [ ] 5. Queries: list and detail with remaining quantities and the stock warning (R5, R15).
- [ ] 6. Controller with the roles of R14; catalogue; build at 0 warnings.
- [ ] 7. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R2`, `R6`, `AC1` | 2, 4 | Draft for S on 2026-11-15 -> session `2026-27`, `ORD/2026-27/0001`, GST from S; saved empty; deleted; dated 2027-02-10 -> `2027-28`, `ORD/2027-28/0001`; B's first of 2026-27 -> 0001 |
| `R7`, `AC2` | 1, 4 | The worked example of AC2 as an order: every number of AC2 (same state, other state, GST off) matches the response and SQL |
| `R3`, `R18`, `AC3` | 2, 4 | Each invalid input of AC3 -> 400 with its code; the same item and version again adds 1 |
| `R5` | 5 | A line above the book's Main stock carries the warning; the order is still saved and confirmed |
| `R6`, `R14`, `AC4` | 2, 4 | Confirm empty -> 400; change or delete confirmed -> 409; Owner closes with a reason; Staff close -> 403 |
| `R1`, `R15`, `AC10` | 5, 6 | B's Owner on A's order -> 404 same as unknown; list paging, search and filters; anonymous 401 |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Money: AC2's numbers were worked out by hand and checked with Python before the code. Additive migration; rollback: drop schema `sales`.
