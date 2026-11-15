# Plan sales#09: Order payment type and the Cash/Credit memo title

- **File:** `docs/plans/sales/2026-11-15-09-order-payment-type.md`
- **Kind:** Backend
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R31 (the title), R34; AC21, AC25
- **Owner:** abhishek
- **Status:** Done (2026-11-15)
- **Base branch:** `feature/school-07` (branch `feature/sales-09`)
- **Depends on:** `sales#07`, `school#07`
- **UI plan:** sales#10
- **Approved by / on:** abhishek / 2026-11-15 (chat: Cash/Credit memo answered "recommended"; the owner asked to modify the existing invoice PDF spec and plans)
- **Created:** 2026-11-15

## 1. In one minute
After this plan an order has a payment type (Cash or Credit) that starts from the school and can be changed until something is invoiced, the invoice keeps it, and the PDF of a GST-off invoice is titled CASH MEMO or CREDIT MEMO accordingly. Try it: `PUT /api/v1/school-orders/{id}` with `"paymentType": "CREDIT"`, make the invoice, `GET /api/v1/invoices/{id}/pdf`.

## 2. Touches / leaves out
Touches: `Domain/Sales` (the order's payment type and its lock, the invoice copy), `Application/Sales` (the composer reads the school default through the directory; the invoice read model; the title in the PDF template), persistence (`sales.school_order.payment_type`, `sales.invoice.payment_type`, migration), catalogue.
Leaves out: screens (sales#10). Schema change: yes (two columns, additive).

## 3. Approach
The order takes the school's default when created and an explicit value when sent; it changes only while nothing is invoiced (the lock of R20, `sales.totals_locked`); the invoice copies it like its other snapshot values (R13). The PDF template chooses TAX INVOICE / CASH MEMO / CREDIT MEMO from GST and the payment type.

## 4. Tasks (at most 8)
- [x] 1. Domain: the order's payment type, set from the school and locked after an invoice; the invoice copy (R34, R13).
- [x] 2. Persistence: both columns, migration; show the SQL and target before applying.
- [x] 3. Composer and requests: default from the school, explicit value on save (R34).
- [x] 4. Invoice read model and the PDF title and Payment line (R31).
- [x] 5. Catalogue; build at 0 warnings.
- [x] 6. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R34`, `AC25` | 1-3 | **PASS** (13 of 13, scratch `check_sales09.py`, real `book_inventory`). Existing orders and invoices took `CREDIT`; a new order of a CASH school starts `CASH`; an explicit value on create wins; the confirmed order `CASH` -> `CREDIT` before an invoice: 204; a PUT that leaves `paymentType` out keeps it (not reset to the school's CASH); the invoice made keeps `CREDIT`; after the invoice a PUT with `CASH` -> 409 `sales.totals_locked`, the same value is accepted; the school default changed afterwards, the invoice still `CREDIT`. |
| `R31`, `AC21` | 4 | **PASS.** GST off + CREDIT: `CREDIT MEMO`, `GST Exempted`, Payment Credit; GST off + CASH: `CASH MEMO`, `GST Exempted`, Payment Cash; GST on + CASH: `TAX INVOICE`, no memo, Payment Cash (PDF text read per page). The 23 checks of sales#05 (editing, class sets, bulk, layouts) pass again after the change. |

## 6. Not verified
The directory reference carried the school default into the order (checked through the new order of a CASH school). A draft order whose school default changes before it is confirmed keeps the value it was made with (not checked).

## 7. Risks and notes
Additive migration `Sales_PaymentType` (`payment_type text NOT NULL DEFAULT 'CREDIT'` and a CHECK on `sales.school_order` and `sales.invoice`) applied to the local `book_inventory` after its SQL was shown; rollback: drop the constraints and the columns. As in school#07 the default lives only in the migration. An amendment now keeps GST and payment type when the request leaves them out (before, GST fell back to the school's default). `SchoolOrder.cs` stays at 300 lines (the amend header check moved to `OrderRules`).
