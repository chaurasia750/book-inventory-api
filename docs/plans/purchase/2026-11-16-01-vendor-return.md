# Plan purchase#01: Vendor return (Debit Note): make, cancel, read

- **File:** `docs/plans/purchase/2026-11-16-01-vendor-return.md`
- **Kind:** Backend
- **Spec:** [../../specs/purchase.md](../../specs/purchase.md) - rules covered: R1-R10, R12, R13; AC1-AC8, AC10
- **Owner:** abhishek
- **Status:** Approved
- **Base branch:** `main` (one branch `feature/purchase-vendor-return` for price#06, sales#27, sales#28 and purchase#01-#04 in both repositories, the owner's instruction "sare plan ek hi branch pe implement krna")
- **Depends on:** `price#06`, `vendor#03` (Done, the vendor directory), `item#03` (Done), `stock#01` (Done, posting R11), `finance#01` (Done, posting R5), `platform#07` (Done, code series)
- **UI plan:** purchase#02
- **Approved by / on:** abhishek / 2026-11-16 (chat: Q1-Q7, Q41, Q42 answered one by one; "sare plan ek hi branch pe implement krna")
- **Created:** 2026-11-16

## 1. In one minute
After this plan a vendor return can be recorded: `POST /api/v1/vendor-returns` with the vendor and the items sent back, each from Main or Damaged stock. It takes the books out of stock, lowers what the company owes the vendor in the vendor ledger and gets a number `DN/2026-27/0001`. The Owner can cancel it. The list and the detail can be read.

## 2. Touches / leaves out
Touches: a new module `purchase`: `Domain/Purchase` (`VendorReturn` root, `VendorReturnLine`, enums, events, ledger entry), `Application/Purchase` (a make service, a cancel service, a query service, DTOs, the message catalogue), `Infrastructure` (configurations, one migration `Purchase_VendorReturn`), `Api/Controllers/Purchase`, `docs/Code_Map.md`. Leaves out: the Debit Note PDF (purchase#03), the screens (purchase#02), GST, discounts, the purchase itself.

## 3. Approach
The vendor return is its own aggregate: the amounts (line amount, total, round-off) are computed in the domain with the rounding helper of the sales module's money rule (moved to a shared place only if it can be done in its own small change; otherwise a reference to the same rounding written once in this module), the stock out goes to Main or Damaged through the stock capability, the ledger transaction (debit `VENDOR_PAYABLE`, credit `PURCHASE_RETURN`, round-off) through the finance capability, all in the request transaction; the cancel posts the exact opposite of both.

## 4. Tasks (at most 8)
- [ ] 1. Domain: `VendorReturn` and `VendorReturnLine` with the amounts, the snapshot, `Cancel`, the events and the ledger entry (R2-R5, R7, R8, R10).
- [ ] 2. Infrastructure: configurations and the migration `Purchase_VendorReturn`; show the SQL and the target database `book_inventory` before applying (R13).
- [ ] 3. Application: make service: vendor check, items and default purchase price (price R19), number from the code series, stock and ledger posting, low-stock list (R2-R7).
- [ ] 4. Application: cancel service (reverse stock and ledger, Owner only) (R8, R9).
- [ ] 5. Application: query service: list with filters and the detail (R12).
- [ ] 6. Api: the three endpoints with their roles; message catalogue codes; the module registered (R9, R13).
- [ ] 7. Build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [ ] 8. Verify AC1 to AC8 and AC10 on the local database `book_inventory` with a scratch check.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R1`-`R3`, `AC6`, `AC8` | 1, 3 | numbers `DN/2026-27/0001`, `0002` and the restart for company B; inactive or unknown vendor, date tomorrow, no lines refused; another company's record `404` |
| `R4`, `R5`, `AC1`-`AC3` | 1, 3 | 5 x 180 = 900.00; rate 175.50 x 3 = 526.50, total 527, round-off +0.50 (figures worked out first, independently) |
| `R6`, `R7`, `AC1`, `AC2`, `AC4`, `AC5` | 3 | Main and Damaged stock move as the line says; an amount-only line moves no stock; the vendor's balance falls by the total; more than the stock accepted with the zero-or-below list; the ledger transaction balances |
| `R8`, `R9`, `AC7` | 4 | the Owner cancels: stock and balance back, number stays; Staff `403`; twice `409` |
| `R10`, `R12`, `R13`, `AC10` | 1, 5, 6 | the snapshot after a price change; list filters; the detail |

## 6. Not verified
(filled when the plan is closed)

## 7. Risks and notes
Money and migration: a person reviews it (CLAUDE.md "Money"). A new module: `CLAUDE.project.md` lists it (changed with the spec). Q1-Q7 were answered by abhishek on 2026-11-16.
