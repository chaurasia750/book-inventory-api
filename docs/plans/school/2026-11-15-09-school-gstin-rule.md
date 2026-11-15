# Plan school#09: School GSTIN rule

- **File:** `docs/plans/school/2026-11-15-09-school-gstin-rule.md`
- **Kind:** Backend
- **Spec:** [../../specs/school.md](../../specs/school.md) - rules covered: R21; AC17
- **Owner:** abhishek
- **Status:** Done (2026-11-15)
- **Base branch:** `feature/company-10` (branch `feature/company-11`)
- **Depends on:** `company#11`
- **UI plan:** none, the school form shows the server's message
- **Approved by / on:** abhishek / 2026-11-15 (chat: "gap fix kro", the GSTIN and email gap of the company backlog item)
- **Created:** 2026-11-15

## 1. In one minute
After this plan a school's GSTIN is checked for the GST shape like a vendor's and a company's, and the school's own copies of the mobile and email checks are gone (it uses the shared rule). Try it: `POST /api/v1/schools` with a GSTIN `123` -> 400 `school.invalid_gstin`.

## 2. Touches / leaves out
Touches: `Domain/School` (the school's GSTIN, mobile and email call the shared `ContactRules` of company#11; `SchoolRules`' copies are removed; one new message code), the school catalogue.
Leaves out: the screens. No schema change.

## 3. Approach
Replace the school's length-only GSTIN check with `ContactRules.Gstin` and its mobile and email checks with the shared ones (the same results as before).

## 4. Tasks (at most 8)
- [x] 1. The school uses `ContactRules` for GSTIN, mobile and email; `school.invalid_gstin` and its text (R21).
- [x] 2. Build at 0 warnings; check on the real local database.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R21`, `AC17` | 1, 2 | **PASS.** POST school with GSTIN `123` -> 400 `school.invalid_gstin`; `10abcde1234f1z7` -> 201 and stored as `10ABCDE1234F1Z7`; no GSTIN -> 201; a mobile of 5 digits -> 400 `school.invalid_mobile` and an email `x@` -> 400 `school.invalid_email` (unchanged). |

## 6. Not verified
Run on the real local database `book_inventory` (scratch `check_contact.py`, 16 of 16); the test vendor and schools were soft-deleted and the ABC company's values put back.

## 7. Risks and notes
No migration. `SchoolRules.Mobile` and `Email` stay as one-line calls to the shared rule so the school keeps its own message codes.
