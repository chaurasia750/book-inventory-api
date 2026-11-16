# Plan sales#17: School counter bill: make, read and cancel

- **File:** `docs/plans/sales/2026-11-16-17-counter-bill.md`
- **Kind:** Backend
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R42-R47, R49; AC35-AC41
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Depends on:** `sales#15`
- **UI plan:** sales#18
- **Approved by / on:** abhishek / 2026-11-16 (chat: "all plan approve hai, ek hi branch pe")
- **Created:** 2026-11-16

## 1. In one minute
After this plan a bill can be made at a counter for a student: lines at MRP with discounts and GST, paid in Cash and/or UPI, numbered `CNT/2026-27/0001`, with the stock taken out and no ledger entry; the Owner can cancel it, and the bills can be listed and opened. Try: as Staff `POST /api/v1/counter-bills` with a counter, a student, two lines and the payments, then `GET /api/v1/counter-bills`.

## 2. Touches / leaves out
Touches: `Domain/Sales` (`CounterBill` with its lines and payments, events), `Application/Sales` (make, cancel and query use cases, the contracts, the catalogue), the shared amounts rule of R7 (reused, not copied), `Infrastructure/Sales` and `Persistence` (three tables, migration), `Api/Controllers/Sales` (`counter-bills`), `docs/Code_Map.md`.
Leaves out: the day summary (sales#19), the receipt PDF (sales#21), every screen (sales#18, 20, 22), returns, the Dukaan (Direct) sale.

## 3. Approach
The bill is a root that owns its lines and payments; the amounts come from the one R7 domain rule that orders and invoices already use, fed the MRP as the price; the payments must equal the grand total (R45). The use case loads the counter, finds or creates the student (sales#15), reads the items through the item directory and the prices through the price directory (default version on the bill date), takes the number from the company code series (`CNT/<financial year>/`), posts the stock out through `IStockPosting` (`COUNTER_SALE`, document type `COUNTER_BILL`) and nothing to the ledger; a failure rolls everything back. Cancel reverses the stock through the same capability. Item discounts follow the company setting (R36).
Schema change: yes (three new tables in the existing schema `sales`, additive, after sales#15 is merged).

## 4. Tasks (at most 8)
- [x] 1. Domain: `CounterBill` (make with lines, payments and the shared amounts; cancel), events `CounterBillMade` and `CounterBillCancelled`, message codes (R42-R47).
- [x] 2. Persistence: configurations and migration `Sales_CounterBill`; show the SQL and the target `book_inventory` before applying.
- [x] 3. Make-bill use case: counter open, student, items and prices, amounts, number, stock out, the low-stock list (R42-R46).
- [x] 4. Cancel use case with the stock reversal (R47).
- [x] 5. Query service: the list, the filters, one bill, a student's bills (R49).
- [x] 6. `CounterBillsController` with the roles of R48, catalogue and registration.
- [x] 7. Build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [x] 8. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R42`, `R44`, `R46`, `AC35` | 1, 3 | the worked bill (33.33 x 3 and 100.00 x 1, 5 percent total discount, GST off): subtotal 199.99, discount 10.00, grand total 190, round-off 0.01; numbers 0001 then 0002, company B starts at 0001; one `COUNTER_SALE` entry per stock item; no ledger transaction; the items at zero are listed |
| `R43`, `R44`, `AC36` | 1, 3 | GST applied with equal states: CGST and SGST per line as R7; item discount refused when the company setting is off |
| `R45`, `AC37` | 1, 3 | Cash 100 + UPI 90 on 190 gives 201; 100 + 80 gives 400 with no bill, stock entry, student or number used; two Cash lines, zero and another mode 400 |
| `R43`, `AC38` | 3 | a barcode that matches nothing adds nothing; an item without a price version is refused; a low-stock line warns and the bill is made (the class book list is the screen's, sales#18) |
| `R47`, `AC39` | 4 | Owner cancels with a reason: stock reversed, status CANCELLED, number kept, no ledger transaction; 3-character reason 400; Staff 403; twice 409 |
| `R48`, `AC40` | 6 | Staff make and read; counters and cancel are the Owner's; anonymous 401; B opening A's bill 404 identical to unknown |
| `R49`, `AC41` | 5 | paged and capped at 200, filters, search by number and student name, `studentId` lists a student's bills, unknown sort ignored |

## 6. Not verified
Observed (2026-11-16, scratch API on the real `book_inventory`; migration `Sales_CounterBill` applied: tables `sales.counter_bill`, `counter_bill_line` and `counter_bill_payment`, additive, two foreign keys inside the aggregate only; fake data: Owner and Staff of Children Book Center, Owner of ABC; school SCH-0097 with classes Nursery and LKG; item X BOOK MRP 33.33 (sale price 30.00) with stock 2, item Y COPY GST 12 MRP 100.00 with stock 50):
- AC35: X x 3 and Y x 1, total discount 5 percent, GST off, payments Cash 100 + UPI 90 (reference UPI123) -> 201, grand total 190; the detail has subtotal 199.99, total discount 10.00, taxable 189.99, round-off 0.01, the line of X at MRP 33.33 (not the 30.00 sale price), the student snapshot and the counter name; the number `CNT/2026-27/0001` style and the next bill is the next number; X is listed as at or below zero (stock 2, sold 3); X's stock card has a `COUNTER_SALE` entry; the school ledger has the same number of rows before and after (no posting).
- AC36: GST applied on Y x 2 (200.00): CGST 12.00, SGST 12.00, IGST 0, grand total 224 (company and school in the same state); an item discount -> 400 `sales.item_discount_disabled` while the company setting is off.
- AC37: Cash 100 + UPI 80 on 190 -> 400 `sales.counter_bill.payment_mismatch`; no bill saved, no student created (`ghost` is not suggested) and the next successful bill took the next number (none used); two Cash lines, a zero amount and the mode `CARD` -> 400.
- AC38: an item without a price version -> 400 `sales.no_price`; an unknown item refused; the same item twice becomes one line of quantity 3; no lines 400; a future date 400 `invalid_date`; a past date accepted.
- AC39: Staff cancel 403; a 3-character reason 400; Owner cancel 204 with the reason; again 409; the bill is `CANCELLED` with its number kept; X's stock card has a `REVERSAL` entry; the school ledger still has no counter row.
- AC40: Staff make a bill and read; company B opening A's bill and an unknown id answer 404 with the same body; anonymous 401.
- AC41: page size 500 -> 200; newest bill date first; `studentId` lists the student's bills together; filters by class and status (CANCELLED) work; search by number finds the bill, by a student name that was refused finds none; an unknown sort is ignored.
NOT verified: company B's first bill being `0001` and two bills made at the same moment getting different numbers (the code series is the platform's, platform P23, checked there); a Super Admin without a company getting 403 `core.company_required` on a bill; more than 200 lines; the low-stock warning in the bill detail (the spec's R49 now says the warning is given when the bill is made, R46).

## 7. Risks and notes
Money rule: the grand total, the GST split and the payment match are checked with exact numbers worked out first (recorded in section 5). Two schema-changing sales plans (15, 17) are sequential through `Depends on`.
