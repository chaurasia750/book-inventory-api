# Plan company#09: One bank account rule for vendors and companies

- **File:** `docs/plans/company/2026-11-15-09-one-bank-account-rule.md`
- **Kind:** Backend
- **Spec:** [../../specs/company.md](../../specs/company.md) - rules covered: R19; AC21
- **Owner:** abhishek
- **Status:** Done (2026-11-15)
- **Base branch:** `feature/sales-07`
- **Depends on:** `company#07` (Done), `vendor#01` (Done)
- **UI plan:** none, the screen is company#08 (it shows the 400 message)
- **Approved by / on:** abhishek / 2026-11-15 (the owner's remark that the bank account information looked duplicated, with his standing instruction to change spec and plans where needed)
- **Created:** 2026-11-15

## 1. In one minute
After this plan a company's bank account is checked by the same rule as a vendor's: all five values or none, an account number of 9-18 digits, a valid IFSC. Before, the company's account had no format check and its fields were a second copy of the vendor's. Try it: `PUT /api/v1/companies/{id}/invoice-identity` with an account number of 8 digits -> 400 `company.invalid_bank`.

## 2. Touches / leaves out
Touches: `Domain/Core` (the shared `BankAccount` value object with its rule and mask), `Domain/Vendor` (the vendor uses it; `VendorBank` and its copy of the rule are removed), `Domain/Company` (the company uses it), the two message catalogues. No schema change, no API shape change.
Leaves out: the invoice snapshot columns (a deliberate copy of what was printed); the screens.

## 3. Approach
Move the vendor's five-value bank account and its validation (R5) into one value object in `Core` and call it from both aggregates with their own message code; the company keeps its separate optional UPI id. The vendor's behaviour is unchanged except that a too-long text is the module's bank code rather than its field code.

## 4. Tasks (at most 8)
- [x] 1. `BankAccount` in `Core` with the rule and the mask; the vendor uses it and its own copy is removed.
- [x] 2. The company uses it, with `company.invalid_bank` and its catalogue text.
- [x] 3. Build at 0 warnings; check vendor R5 and company R19 on the real local database (12 of 12 checks passed, scratch script, not committed; the test vendor was soft-deleted and the ABC company's account left empty as before).

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R19`, `AC21` | 2, 3 | PASS. PUT `companies/{id}/invoice-identity` as the Owner of ABC Book Center: 3 of the 5 values -> 400 `company.invalid_bank`; account number of 8 digits -> 400; with letters -> 400; IFSC `SBIN1001234` -> 400; a complete account (IFSC sent in lower case) + UPI -> 200 and the read returns `SBIN0001234` and the UPI; an empty bank -> 200 and the read is empty |
| vendor `R5` (regression) | 1, 3 | PASS. POST vendor with 4 of 5 values -> 400 `vendor.invalid_bank`; number with letters -> 400; wrong IFSC -> 400; a complete account -> 201; the detail shows the full account; the list shows `XXXXXXXX4567` and never the number |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
No migration. The company's earlier free-text account fields (none in production) now need a valid account number and IFSC to be saved.
