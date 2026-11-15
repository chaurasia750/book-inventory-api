# Plan finance#09: UI links from the ledger rows

- **File:** `docs/plans/finance/2026-11-16-09-ui-ledger-links.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/finance.md](../../specs/finance.md) - rules covered: R19; AC12
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `payment#02` (Done, the receipt in the address), `sales#02` (Done, the order and invoice screens)
- **UI plan:** this is itself a UI plan (the feature has no backend change: the rows already carry the document id)
- **Approved by / on:** abhishek / 2026-11-16 (chat: "next pending plan kro")
- **Created:** 2026-11-16

## 1. In one minute
After this plan the Ledger tab of a school links its rows: the invoice number opens the invoice, the order number opens the order, a receipt row opens the receipt panel beside the Payments tab.

## 2. Touches / leaves out
Touches: `apps/member` (`pages/ledger` statement component, a small lookup of an order and of a receipt by its number), the existing sales and payment services, i18n.
Leaves out: the Orders and Invoices lists, the vendor ledger (no vendor documents exist yet), any change of the statement or the PDF.

## 3. Approach
A school invoice row links to `/invoices/<document id>` and a payment receipt row to `/schools/<school id>/payments/<document id>`; the order number in the invoice row's detail and the receipt number of a cheque row are found by their number through the existing searchable lists (`school-orders?search=`, `receipts?search=`) when clicked, and shown as plain text when nothing matches; the opening row has no link.

## 4. Tasks (at most 8)
- [ ] 1. Link rules for the row types (invoice, receipt, cheque result, opening balance).
- [ ] 2. Lookup by number for an order and for a receipt, with the not-found case.
- [ ] 3. The links in the statement table (keyboard and mouse).
- [ ] 4. i18n; build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R19`, `AC12` | 1-3 | a school with an invoice and a payment: the invoice number, its order number and the receipt number each open their screen; the opening balance row has no link |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
The order and cheque-row links need one extra request when clicked. Seen through headless Chrome only.
