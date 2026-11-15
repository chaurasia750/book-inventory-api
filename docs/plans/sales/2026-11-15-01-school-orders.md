# Plan sales#01: School orders

- **File:** `docs/plans/sales/2026-11-15-01-school-orders.md`
- **Kind:** Backend
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R1-R7, R13-R18; AC1-AC4, AC10
- **Owner:** abhishek
- **Status:** Done (2026-11-15)
- **Depends on:** `school#05`, `price#05`, `stock#05`, `company#06`
- **UI plan:** sales#02
- **Approved by / on:** abhishek / 2026-11-15 (chat: sales Q1-Q14 answered one by one)
- **Created:** 2026-11-15
- **Base branch:** `docs/sales-order-invoice` (one branch `feature/sales` for company#06, price#05, stock#05, school#05, sales#01 and sales#03, the owner's instruction "do it all")

## 1. In one minute
After this plan a school order can be drafted, changed, confirmed, closed or deleted, with prices from the chosen price versions, item and total discounts, GST and round-off computed by one rule, and the low-stock warning per line. Try it: `POST /api/v1/school-orders` with a school and lines, then `GET` it.

## 2. Touches / leaves out
Touches: new `BookInventory.Domain/Sales` (the amounts rule of R7 as a domain service used by order and invoice, `SchoolOrder` with its lines, events, message codes), `Application/Sales` (use cases and queries), `Infrastructure` (schema `sales`, configurations, migration), `Api/Controllers/Sales`, `CLAUDE.project.md` (module `sales`, own commit), `docs/Code_Map.md`.
Leaves out: invoices (sales#03), screens (sales#02). Schema change: yes (new schema `sales`, additive).

## 3. Approach
`SchoolOrder` is a company-owned root with its lines; its behaviours (replace lines, confirm, close) enforce R2-R6 and call the R7 amounts rule (a pure domain service, half-away-from-zero rounding), so the computed totals are stored with the order. The use case resolves the school and its session (school R16, R18), the items (item R16), the price versions (price R18), the stock quantities (stock R20) and the company state (company R16) before calling the root. The number comes from the code series with the prefix `ORD/<session>/` (P23).

## 4. Tasks (at most 8)
- [x] 1. Domain: the R7 amounts rule with its rounding (checked against AC2's worked numbers).
- [x] 2. Domain: `SchoolOrder` and lines, lifecycle, events, message codes (R2, R3, R6).
- [x] 3. Persistence: schema `sales`, configurations, migration; module list in `CLAUDE.project.md` (own commit); show the SQL and target before applying.
- [x] 4. Use cases: create, replace, delete draft, confirm, close (R2-R6, R14).
- [x] 5. Queries: list and detail with remaining quantities and the stock warning (R5, R15).
- [x] 6. Controller with the roles of R14; catalogue; build at 0 warnings.
- [x] 7. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R2`, `R6`, `AC1` | 2, 4 | Draft for S today -> `ORD/2026-27/0001`, session `2026-27`, GST on from S; empty draft saved and soft-deleted; B's first -> `ORD/2026-27/0001` (own counter); previous session `2025-26` accepted -> `ORD/2025-26/0001`, two back -> 400 `sales.invalid_session`. **PASS** |
| `R7`, `AC2` | 1, 4 | The AC2 order: subtotal 2719.50, total discount 54.39 shared 47.50/6.17/0.72, taxable 2665.11, CGST = SGST 21.32, grand 2708.00, round-off +0.25; GST off -> 2665.00, -0.11; company in Delhi (other state) -> IGST 42.63, grand 2708.00, +0.26. **PASS** |
| `R3`, `R18`, `AC3` | 2, 4 | Quantity 0 and 2.5, 101 %, Rs 250.01 on a 250.00 line, a total Rs above the subtotal, an item with no price (`sales.no_price`), an unknown item, a future date, 501 lines -> 400; A twice -> one line of 3. **PASS** |
| `R5` | 5 | Order line A ordered 10 with Main 12 -> no warning; B with Main 0 -> warning; the order still confirms. **PASS** |
| `R6`, `R14`, `AC4` | 2, 4 | Confirm empty -> 400 `sales.order_no_lines`; confirm -> 204; PUT and DELETE confirmed -> 409, 409; Owner close with a reason -> 204, a 2-character reason -> 400; Staff close -> 403. **PASS** |
| `R1`, `R15`, `AC10` | 5, 6 | Owner of B on A's order -> 404 `sales.order_not_found` = unknown id; list `pageSize=500` -> 200, school and status filters; anonymous 401. **PASS** |

## 6. Not verified
Run by `check_sales.py` and `check_igst.py` (scratch, not committed; a temporary scratch controller for the capabilities was deleted before the commit) on the real `book_inventory`: 35/35 passed (the first IGST run failed only because an earlier check of the same script had changed the book's price; it was run again on fresh items and passed).

Migration `Sales_OrdersAndInvoices` (new schema `sales`: `school_order`, `school_order_line`, `invoice`, `invoice_line`; additive) holds the tables of sales#01 and sales#03 together, because both were built on one branch before the first migration: SQL shown in the session, target the local `book_inventory`, applied by the app's start. Rollback: drop schema `sales`.

Notes: the item directory gained `SaleReferencesAsync` (HSN code, GST rate) and the school directory reference gained state, GST default and GSTIN (backlog); the company code series gained `NextNumberAsync` for numbers formatted by the module (platform P23).

## 7. Risks and notes
Money: AC2's numbers were worked out by hand and checked with Python before the code. Additive migration; rollback: drop schema `sales`.
