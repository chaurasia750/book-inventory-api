# Plan company#10: Owner edits the company profile

- **File:** `docs/plans/company/2026-11-15-10-owner-company-profile.md`
- **Kind:** Backend
- **Spec:** [../../specs/company.md](../../specs/company.md) - rules covered: R21; AC22
- **Owner:** abhishek
- **Status:** Approved
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
- [ ] 1. Domain: `Company.EditProfile` keeping the name and the state (R21).
- [ ] 2. Use case and request with the own-company guard; the profile read (R21, R6).
- [ ] 3. Controller: `PUT companies/{id}/profile` for the Owner and an entered Super Admin, `GET companies/current/profile`; build at 0 warnings.
- [ ] 4. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R21`, `AC22` | 1-3 | Owner PUT with address, mobile, email, GSTIN, PAN, tagline -> 200; GET shows them and the same name and state; an invalid email -> 400; Staff PUT -> 403; Owner of A on B's id -> 404; Super Admin's own `PUT companies/{id}` still edits the name and the state |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
No migration. The GSTIN and PAN are stored as the master data does today (upper-case, length limits); a stricter GSTIN shape check is not part of this plan.
