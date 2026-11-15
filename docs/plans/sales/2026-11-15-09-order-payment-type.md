# Plan sales#09: Order payment type and the Cash/Credit memo title

- **File:** `docs/plans/sales/2026-11-15-09-order-payment-type.md`
- **Kind:** Backend
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R31 (the title), R34; AC21, AC25
- **Owner:** abhishek
- **Status:** Approved
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
- [ ] 1. Domain: the order's payment type, set from the school and locked after an invoice; the invoice copy (R34, R13).
- [ ] 2. Persistence: both columns, migration; show the SQL and target before applying.
- [ ] 3. Composer and requests: default from the school, explicit value on save (R34).
- [ ] 4. Invoice read model and the PDF title and Payment line (R31).
- [ ] 5. Catalogue; build at 0 warnings.
- [ ] 6. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R34`, `AC25` | 1-3 | School default `CASH` -> new order `CASH`; PUT `CREDIT` before an invoice -> 204 and the invoice made prints `CREDIT`; PUT after the invoice -> 409 `sales.totals_locked`; the school default changed afterwards, the invoice still `CREDIT` |
| `R31`, `AC21` | 4 | GST on -> `TAX INVOICE`; GST off + `CASH` -> `CASH MEMO`; GST off + `CREDIT` -> `CREDIT MEMO`, each with `GST Exempted` and no CGST/SGST/IGST amounts |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Additive migration; existing orders and invoices take `CREDIT`. Rollback: drop the two columns.
