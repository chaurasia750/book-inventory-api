# Spec: report - Dashboard (and, later, reports)

- **Module:** `report` (no database schema: it reads, it stores nothing)
- **Status:** R1-R13 and AC1-AC9 (the dashboard) built 2026-11-16, Approved by abhishek in chat on 2026-11-16 (module, who sees, session, cheque, cards chosen and Q1-Q4 answered one by one; D1-D6 with them)
- **Last updated:** 2026-11-16 (new module, dashboard)
- **Source docs:** [BRD §4.16 Reports aur Dashboard (the Dashboard (Home) block), §3 (who sees what), §5.1 BR-SES, §5.6 BR-LED, FR-STK-06, FR-PAY-07](../bussiness/BRD-Book-Inventory-Phase1.md); [specs/platform.md](platform.md) P18-P22; [specs/sales.md](sales.md) R6, R8, R11, R42, R57, R64; [specs/payment.md](payment.md) R5, R6, R10, R11, R14; [specs/finance.md](finance.md) R7, R11, R13; [specs/stock.md](stock.md) R9, R20; [specs/price.md](price.md) R19; [specs/purchase.md](purchase.md) R7, R8; [specs/school.md](school.md) R18
- **Plans:** [sales#29](../plans/sales/2026-11-16-29-dashboard-figures.md) (Done 2026-11-16, R7, R8), [payment#11](../plans/payment/2026-11-16-11-dashboard-figures.md) (Done 2026-11-16, R4-R6), [stock#06](../plans/stock/2026-11-16-06-dashboard-figures.md) (Done 2026-11-16, R10), [purchase#05](../plans/purchase/2026-11-16-05-dashboard-figures.md) (Done 2026-11-16, R8), [report#01](../plans/report/2026-11-16-01-dashboard-api.md) (Done 2026-11-16, R1-R3, R9, R12, R13, AC1-AC9), [report#02](../plans/report/2026-11-16-02-ui-dashboard.md) (Done 2026-11-16, UI, R11)

> One spec per module. It says WHAT the module does and WHY, never implementation steps (those live in plans).

## 1. Purpose and scope

`report` shows the company's figures: the **Dashboard**, the Home page of the Owner and Staff, built only by reading the other modules through their capabilities (never by joining their tables). It owns no table and posts nothing. Reports (BRD §4.16 table: stock report, sales report, payment report, ...) are later features of this module.

Feature **Dashboard** (R1-R13): one screen with a **session** chosen at the top and these parts: payments received (total, by way of payment, what came in for the **earlier** sessions' invoices, pending cheques), orders and invoices of the session, today's and the session's sales, returns and refunds, what is owed (schools) and owed to (vendors), stock, and the quick buttons.

Out of scope for this feature: the reports of BRD §4.16 as such (Excel, PDF, filters); the **sample** card ("30 din se zyada sample") and the **vendor order pending** card, because their modules (Sample, Vendor Order, Maal Aaya) are not built (R12); charts beyond a day-wise line of sales (Q4 of the UI plan); the Super Admin's screens in the `admin` app.

## 2. Concepts and glossary

- **Dashboard session**: a label `2026-27`, the April-March year it starts in (D2). Its **window** is 1 April 2026 to the earlier of today and 31 March 2027, in the business time zone (`Asia/Kolkata`).
- **Session start year**: the first four digits of any session label: `2026-27` and a Jan-Dec school's `2026` both start in 2026 (school R18).
- **Earlier-session invoice**: an invoice whose session start year is **before** the dashboard session's.
- **Received** (R4): money that has really come in: Cash, UPI and Bank transfer, and a cheque once it has cleared.

## 3. Business rules

**Dashboard** (BRD §4.16; Approved, proposed by the assistant 2026-11-16 from the BRD and the owner's list)
- **R1** The dashboard is **read-only and live**: every call reads the figures again through the capabilities of the modules (sales, payment, finance, stock, purchase, price) and keeps nothing. It belongs to the company of the session (platform P18-P21); another company's data is never read. Each part is read on its own: a part that fails shows its error and the others still show (R13).
- **R2** **Who sees what** (BRD §3, D3): the Owner and an entered Super Admin see every part; **Staff** see the orders and invoices (R7), today's and the session's sales (R8) and the stock counts without value (R10), and the quick buttons (R11); they never see payments (R4-R6), what is owed (R9), returns and refunds, or the stock value (finance R13, payment R2). Anonymous `401`.
- **R3** **Session choice** (D2): `GET dashboard?session=2026-27`; the label must be `YYYY-YY` with the second part the next year (`400 report.invalid_session` otherwise); with no label the session whose April-March year contains today is used. The screen offers the current session and the four before it (Q3). Everything dated is cut by the **window** of the glossary; a window that has not started (a future session) shows zeros.
- **R4** **Payments received** (FR-PAY-02, FR-PAY-07; Q1): the sum of the amounts of the modes of `RECORDED` receipts: Cash, UPI and Bank transfer **dated by the receipt date** inside the window, and cheques **cleared** with the clearing date inside the window; a bounced cheque and a cancelled receipt count for nothing. Shown as a total and split by way of payment (Cash, UPI, Bank transfer, Cheque) and with the number of receipts.
- **R5** **Pending cheques** (BRD "pending cheque" card): the number and the amount of cheque modes of `RECORDED` receipts that are still `PENDING`, of any date (a cheque does not stop being pending at the end of a session); the list stays the payment list filtered to pending (payment R16).
- **R6** **Earlier sessions' payment that came in this session** (the owner's first question, D4, Q2): the net of the allocation rows (payment R6, R11, R12; positive when allocated, negative for a bounce, a cancelled invoice or a cancelled receipt) **made inside the window** to invoices that are **earlier-session invoices** (glossary), shown as one amount and as the number of schools, with the amount still due on those invoices today (their allocated total against their grand total).
- **R7** **Orders and invoices of the session** (the owner's third question): for the orders whose session start year equals the dashboard session's: the number and the grand total of orders **by status** (`DRAFT`, `CONFIRMED`, `CLOSED`); the number and the grand total of invoices made (live) and the number cancelled; and the books **still to invoice** (the remaining quantity of the lines of `CONFIRMED` orders, sales R6, R8).
- **R8** **Sales and returns**: **today's sale** and the **session's sale** (window) are the live school invoices dated in it plus the live counter bills dated in it (sales R8, R42), shown as one total with the two parts; a day-wise list of the last 30 days of the window for the line chart; **returns** are the live credit notes (sales R53), the refunds (R64) and the vendor returns (purchase R2) dated in the window: their count and total each.
- **R9** **What is owed** (Owner; BRD cards "school outstanding" and "vendor dena"; Q3): the total balance of the schools that owe the company (finance R7 above zero) and how many, the five largest, the total of the schools that hold an advance (below zero); the vendors the company owes: total and the five largest. These are today's balances, whatever the session (finance R10).
- **R10** **Stock** (FR-STK-06): the number of stock-managed items whose Main stock is **zero or below** (stock Q4), the number below zero, the Damaged stock in pieces; and, for the Owner only, the **stock value**: the Main quantity of each item above zero times its purchase price in its default version today (price R19; an item without a price counts as zero and is counted in a note).
- **R11** **Quick buttons** (BRD): New order, Payment (Owner only), Counter sale, each opening its screen; Maal Aaya and Sample Dena come with their modules (R12).
- **R12** **Not on the dashboard yet**: the sample card and the vendor order pending card (their modules are not built) and the BRD's "Maal Aaya" button; each appears with its module, never as a placeholder.
- **R13** The answer of `GET dashboard` is `200` with one object per part (`payments`, `orders`, `sales`, `returns`, `owed`, `stock`), each either its figures or `{error: <code>}` when that part failed; a part the caller may not see (R2) is absent. A figure is money `numeric(18,2)` or a whole count, never a float.

## 4. Data

None: the module owns no table. It adds read capabilities to other modules' plans (sales#29, payment#11, stock#06, purchase#05), each returning plain figures for the session's company.

## 5. API contract (`/api/v1`)

| Method + route | Who | Notes |
|---|---|---|
| `GET dashboard` | `OWNER`, `STAFF`, `SUPER_ADMIN` (entered) | R1-R13: `?session=2026-27` (default: the current session); the parts of R2 for the caller; `400 report.invalid_session` |

## 6. User interface

The **Home** page (`/`) of the member app replaces the Welcome page: the session dropdown, the quick buttons, then cards (payments, orders and invoices, today's and the session's sales with a small line, returns and refunds, what is owed, stock). A card that fails shows its message and a retry; empty figures show zero, never a blank. Written in the UI plan.

## 7. Events

None raised or handled.

## 8. Dependencies

`report` depends on `sales` (figures capability, plan sales#29), `payment` (plan payment#11), `finance` (the outstanding list, R11), `stock` (plan stock#06), `price` (purchase price, R19), `purchase` (plan purchase#05), `school` (the session rule R18 where a label is needed) and the platform (P18-P25). None of them depends on `report`.

## 9. Acceptance criteria

- **AC1** For session `2026-27` with receipts of Cash 5,000 and UPI 2,000 on 2026-11-10, a cheque of 3,000 cleared on 2026-11-12 and another of 1,000 still pending: received 10,000 (Cash 5,000, UPI 2,000, Cheque 3,000), pending cheque 1 for 1,000 (R4, R5).
- **AC2** A receipt of 4,000 allocated to an invoice of session `2025-26` on 2026-11-11 and 1,500 allocated to an invoice of `2026-27` the same day: the earlier-session amount is 4,000 (R6); after the receipt is cancelled it is 0 (R6, payment R14).
- **AC3** Orders of the session: 3 `DRAFT` (2,000), 2 `CONFIRMED` (9,000) and 1 `CLOSED` (4,500) show those counts and totals; the invoices made show their count and total and the cancelled count; the books still to invoice equal the remaining quantity of the confirmed orders (R7).
- **AC4** Today's sale is the live invoices plus live counter bills dated today; a cancelled one is left out; the session sale is the same over the window (R8).
- **AC5** A credit note of 900, a refund of 300 and a vendor return of 360 inside the window show as three counts and totals; one dated before the window is left out (R8).
- **AC6** The schools that owe the company total the sum of their positive balances and the five largest are listed largest first; a school with an advance is in the advance total only (R9).
- **AC7** Items with Main stock 0 or below are counted; one at -3 is also in the below-zero count; the stock value is the Main quantity above zero times the purchase price (R10).
- **AC8** As Staff the answer has `orders`, `sales` and `stock` without value, and no `payments`, `owed` or `returns`; anonymous is `401` (R2).
- **AC9** `session=2026` is `400 report.invalid_session`; no session gives the current one; a future session shows zeros; a failing part shows its error while the others show (R3, R13).

## 10. Decisions

| # | Decision |
|---|---|
| D1 | Chosen by abhishek in chat 2026-11-16: **a new module `report`** (BRD RPT) holds the dashboard and the later reports; it reads only through capabilities. |
| D2 | Chosen by abhishek in chat 2026-11-16: **one session is chosen on the dashboard** (default: the current), written `2026-27`, and everything dated is cut by its April-March window; schools on Jan-Dec sessions join through the session start year. |
| D3 | Chosen by abhishek in chat 2026-11-16: **the Owner sees everything, Staff only sales, orders and stock** (no payments, owed amounts or stock value). |
| D4 | Chosen by abhishek in chat 2026-11-16: **"earlier session's payment that came this session"** is counted **per invoice** (the net allocated inside the window to invoices of an earlier session), not from the school ledger. |
| D5 | Chosen by abhishek in chat 2026-11-16: **pending cheques are not "received"** until they clear; they are shown apart (R4, R5). |
| D6 | Chosen by abhishek in chat 2026-11-16: **version 1 has all the parts the data allows** (payments, orders and invoices, sales, returns and refunds, owed, stock, quick buttons). |

## 11. Open questions

- **Q1** A cheque counts as received on which date? Recommended: **its clearing date** (that is when the money is in), alternative: its receipt date. Answered by abhishek 2026-11-16: **its clearing date**.
- **Q2** "Made inside the window" for an allocation (R6): which date? Recommended: **the date the allocation row was made in the business time zone** (the day the receipt was recorded or the bounce/cancel happened), alternative: the receipt's date. Answered by abhishek 2026-11-16: **the date the allocation row was made in the business time zone**.
- **Q3** How many rows in "the largest" lists and how many sessions in the dropdown? Recommended: **five largest and five sessions** (current and four before), alternative: ten and three. Answered by abhishek 2026-11-16: **five largest and five sessions (the current and four before)**.
- **Q4** The day-wise sales line (R8): last 30 days of the window, or the whole window by month? Recommended: **the last 30 days of the window, by day**, alternative: by month over the session. Answered by abhishek 2026-11-16: **the last 30 days of the window, by day**.
