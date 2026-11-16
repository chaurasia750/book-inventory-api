# Plan sales#23: School return (credit note): make, cancel, read

- **File:** `docs/plans/sales/2026-11-16-23-school-return.md`
- **Kind:** Backend
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R53-R63; AC44-AC52
- **Owner:** abhishek
- **Status:** Approved
- **Base branch:** `main` (one branch `feature/sales-school-return` for sales#23-#26 in both repositories, the owner's instruction)
- **Depends on:** `sales#05` to `sales#07` (the invoice, Done), stock R11 (`SCHOOL_RETURN_GOOD`, `SCHOOL_RETURN_DAMAGED`), finance R5 (`SALES_RETURN`), platform P23
- **UI plan:** sales#24
- **Approved by / on:** abhishek / 2026-11-16 (chat: "all approved hai, ek branch pe sab kro UI and api")
- **Created:** 2026-11-16

## 1. In one minute
After this plan a school's return can be recorded as a **credit note**: `POST /api/v1/credit-notes` with the school, an optional invoice and the returned items (good or damaged). It takes the books into Main or Damaged stock, credits the school's ledger the whole amount at the invoice's rate and discount, and gets a number `CN/2026-27/0001`. The Owner can cancel it (everything reversed). The list, the detail and the "returnable quantity" of an invoice can be read. See it with `GET /api/v1/credit-notes/{id}` and the school's ledger statement.

## 2. Touches / leaves out
Touches: `Domain/Sales` (`CreditNote` root, `CreditNoteLine`, enums for status, reason and condition, events), `Application/Sales` (a make service, a cancel service, a query service, DTOs), `Infrastructure` (configuration, one migration `Sales_CreditNote`), `Api/Controllers/Sales`, the message catalogue, `docs/Code_Map.md`.
Leaves out: the refund (FR-RET-09, Q34), the credit note PDF (FR-RET-07, Q38), the counter return (FR-CNT-13), the vendor return, the UI (sales#24), school profile screens.

## 3. Approach
The credit note is its own aggregate: it computes its amounts in the domain with the same rounding code as the invoice (R7, one rule in one place: the existing amount calculation is reused, never copied) and shares the invoice's total discount in proportion (R55). The make service loads the invoice (when named), the school and the items through their capabilities, builds the aggregate, posts the stock (good to Main, damaged to Damaged) and one ledger transaction (credit `SCHOOL_RECEIVABLE`, debit `SALES_RETURN` and `GST_OUTPUT`, round-off opposite to the invoice's) in the request transaction. The cancel posts the exact opposite of both. The returnable quantity of an invoice line is the sold quantity minus the quantities on live credit notes (R54).

## 4. Tasks (at most 8)
- [ ] 1. Domain: `CreditNote` and `CreditNoteLine` with the amounts, the proportional discount share, the snapshot, `Cancel`, the events (R53-R56, R58, R59, R62).
- [ ] 2. Infrastructure: entity configurations and the migration `Sales_CreditNote`; show the SQL and the target database `book_inventory` before applying (R63).
- [ ] 3. Application: make service: number from the code series, invoice and school checks, stock posting, ledger posting, warning for over-returns (R53, R54, R57).
- [ ] 4. Application: cancel service (reverse stock and ledger, Owner only) (R58, R60); and R62: the order line's remaining quantity counts the returned quantity back, a closed order reopens (changes the existing remaining-quantity rule of the order, in one place).
- [ ] 5. Application: query service: list with filters, detail, the invoice's `returnable` lines and its credit notes (R61).
- [ ] 6. Api: the four endpoints with their roles; message catalogue codes (R60, R63).
- [ ] 7. Build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [ ] 8. Verify AC44 to AC51 on the local database `book_inventory` with a scratch check.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R53`, `R54`, `AC46`, `AC47`, `AC50` | 1, 3 | number series and its restart per company, the date rules, `CANCELLED` invoice `409`, over-return warning, rate without an invoice |
| `R55`, `AC44`, `AC48` | 1 | 10 books at 100 with a 10% total discount, return 4: taxable 360.00; the GST case with CGST/SGST and IGST (figures worked out first, independently) |
| `R56`, `R59` | 1 | the reason and remark rules; the snapshot unchanged after a price change |
| `R62`, `AC52` | 4 | order of 10 invoiced in full and `CLOSED`; credit note of 4: remaining 4, order `CONFIRMED`, a second invoice for 4 accepted; cancel the credit note: remaining back; the invoice itself unchanged |
| `R57`, `AC44`, `AC45` | 3 | the school's balance falls by the grand total; Main stock +4 for `GOOD`, Damaged +4 for `DAMAGED`; the ledger transaction balances |
| `R58`, `R60`, `AC49` | 4 | the Owner cancels: balance and stock back; Staff `403`; twice `409` |
| `R61`, `R63`, `AC51` | 5, 6 | company B's credit note is `404` for A; list filters; the invoice shows its credit notes |

## 6. Not verified
(filled when the plan is closed)

## 7. Risks and notes
Money and migration: a person reviews it (CLAUDE.md "Money"). Q34-Q40 were answered by abhishek on 2026-11-16 (Q39 differs from the recommendation: R62).
