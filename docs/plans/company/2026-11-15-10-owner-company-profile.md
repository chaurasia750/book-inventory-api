# Plan company#10: Owner edits the company profile

- **File:** `docs/plans/company/2026-11-15-10-owner-company-profile.md`
- **Kind:** Backend
- **Spec:** [../../specs/company.md](../../specs/company.md) - rules covered: R21; AC22
- **Owner:** abhishek
- **Status:** Done (2026-11-15)
- **Base branch:** `docs/company-owner-profile` (branch `feature/company-10`)
- **Depends on:** `company#07` (Done)
- **UI plan:** company#08
- **Approved by / on:** abhishek / 2026-11-15 (chat: "company settings ka spec nahi hai kya"; the Owner edits the master data except name and state, the recommended answer)
- **Created:** 2026-11-15

## 1. In one minute
After this plan the Owner can change their own company's address, mobile, email, GSTIN, PAN and tagline through the API (the name and the state stay with the Super Admin), and read them back. Try it: `PUT /api/v1/companies/{id}/profile` as the Owner, then `GET /api/v1/companies/current/profile`.

## 2. Touches / leaves out
Touches: `Domain/Company` (one behaviour on the company that edits these values through the same validation as the master data), `Application/Company` (the request, the profile response, the use case with the "own company" guard of company#07, the query), `Api/Controllers/Company`, the message catalogue only if a code is new.
Leaves out: the screen (company#08). No schema change.

## 3. Approach
`Company.EditProfile` applies the six values through the existing master-data rules (`Apply` with the company's current name and state), so a rule is written once. The use case is the identity service's guard (`RequireOwnedAsync`): the id must be the caller's own company, else 404. The read is the session company's master values.

## 4. Tasks (at most 8)
- [x] 1. Domain: `Company.EditProfile` keeping the name and the state (R21).
- [x] 2. Use case and request with the own-company guard; the profile read (R21, R6).
- [x] 3. Controller: `PUT companies/{id}/profile` for the Owner and an entered Super Admin, `GET companies/current/profile`; build at 0 warnings.
- [x] 4. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R21`, `AC22` | 1-3 | **PASS** (10 of 10, scratch `check_company10.py`, real `book_inventory`). Owner of ABC PUT `companies/{id}/profile` with address, mobile, email, GSTIN, PAN, tagline -> 200; `GET companies/current/profile` returns the six values with the name `ABC Book Center` and the state `Bihar` unchanged (also in the database); a 16-character GSTIN -> 400; a `name` and a `state` in the body are ignored; Staff -> 403; the Owner of ABC on Children Book Center's id -> 404 and its profile untouched; anonymous 401; the Super Admin's `PUT companies/{id}` still edits the name and state (204). The ABC demo values used by the invoice checks were put back. |

## 6. Not verified
A Super Admin who has entered a company and calls the profile endpoint was not exercised (the same guard as the identity endpoints).

## 7. Risks and notes
No migration. The GSTIN and PAN are stored as the master data does today (upper-case, length limits); a stricter GSTIN or email shape check is not part of this plan (vendors check the GSTIN shape, schools and companies only its length: a gap listed in the backlog).
