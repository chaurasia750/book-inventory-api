# Plan company#11: One contact rule for vendors, schools and companies

- **File:** `docs/plans/company/2026-11-15-11-one-contact-rule.md`
- **Kind:** Backend
- **Spec:** [../../specs/company.md](../../specs/company.md) - rules covered: R22; AC23
- **Owner:** abhishek
- **Status:** Done (2026-11-15)
- **Base branch:** `feature/company-10` (branch `feature/company-11`)
- **Depends on:** `company#10` (Done), `vendor#01` (Done)
- **UI plan:** none, the forms show the server's message (the texts are added in the UI repository with this change)
- **Approved by / on:** abhishek / 2026-11-15 (chat: "gap fix kro", the GSTIN and email gap of the company backlog item)
- **Created:** 2026-11-15

## 1. In one minute
After this plan a company's mobile, email, GSTIN and PAN are checked, by the same rule a vendor's and a school's already use for their mobile and email. Before, the company checked only their length, the vendor and the school each kept their own copy of the mobile and email rule, and only the vendor checked the GSTIN shape. Try it: `PUT /api/v1/companies/{id}/profile` with a GSTIN `123` -> 400 `company.invalid_gstin`.

## 2. Touches / leaves out
Touches: `Domain/Core` (one `ContactRules`: mobile, email, GSTIN, PAN, each taking the module's message code), `Domain/Vendor` (the vendor's mobile, email and GSTIN call it; its own copies are removed, behaviour unchanged), `Domain/Company` (the company calls it for the four values; four new message codes), the message catalogues.
Leaves out: the school (school#09), the screens. No schema change.

## 3. Approach
Move the vendor's checks (the same as the school's) into `ContactRules`, each method `Value(string?, string invalidCode)`; the modules keep their own message codes and catalogue texts. The company adds the PAN rule, which no module had. Existing rows are not touched; the rules apply when a value is written.

## 4. Tasks (at most 8)
- [x] 1. `ContactRules` in `Core`; the vendor uses it and its copies are removed.
- [x] 2. The company uses it for mobile, email, GSTIN and PAN, with its codes and catalogue texts (R22).
- [x] 3. Build at 0 warnings; check the company rows and the vendor on the real local database.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R22`, `AC23` | 2, 3 | **PASS.** Owner PUT `companies/{id}/profile`: GSTIN `123` -> 400 `company.invalid_gstin`; PAN `ABC` -> `company.invalid_pan`; email `x@` -> `company.invalid_email`; mobile `12345` -> `company.invalid_mobile`; GSTIN `09abcde1234f1z5`, PAN `abcde1234f` and mobile `+91 98111 22233` -> 200 and the read shows `09ABCDE1234F1Z5`, `ABCDE1234F`, `9811122233`; the Super Admin's `PUT companies/{id}` with GSTIN `123` -> 400 and with the valid current values -> 204. |
| vendor `R3` (regression) | 1, 3 | **PASS.** POST vendor with GSTIN `123` -> 400 `vendor.invalid_gstin`; email `x@` -> `vendor.invalid_email`; mobile `12345` -> `vendor.invalid_mobile`; valid values -> 201 with the GSTIN in upper case and the mobile as 10 digits. |

## 6. Not verified
Run on the real local database `book_inventory` (scratch `check_contact.py`, 16 of 16); the test vendor and schools were soft-deleted and the ABC company's values put back. The older module checks (company#01, vendor#01 and others) were not re-run; the vendor behaviour was re-checked above.

## 7. Risks and notes
Moved, not rewritten: the vendor's mobile, email and GSTIN checks are now `ContactRules` (the school used identical copies of the mobile and email ones, moved by school#09). The company's PAN rule is new (no module had one). No migration. Values saved earlier are not re-checked until the record is edited.
