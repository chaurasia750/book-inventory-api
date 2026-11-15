# Plan company#07: Company invoice identity

- **File:** `docs/plans/company/2026-11-15-07-company-invoice-identity.md`
- **Kind:** Backend
- **Spec:** [../../specs/company.md](../../specs/company.md) - rules covered: R17-R20; AC18-AC20
- **Owner:** abhishek
- **Status:** Done (2026-11-15)
- **Depends on:** none
- **UI plan:** company#08
- **Approved by / on:** abhishek / 2026-11-15 (the invoice PDF approval, Q17-Q18 with the recommended answers)
- **Created:** 2026-11-15
- **Base branch:** `docs/invoice-pdf` (to be cut as `feature/company-07` when approved)

## 1. In one minute
After this plan a company has its printed identity: a **logo**, a **signature image**, **one bank account** (account holder, bank, account number, IFSC, branch, UPI id) and **invoice settings** (terms and conditions, footer, whether the GSTIN prints). The Owner (or a Super Admin inside the company) sets them; other modules read them through one capability. Try it: `PUT /api/v1/companies/{id}/invoice-identity`, `POST /api/v1/companies/{id}/logo`, then `GET /api/v1/companies/current/invoice-identity`.

## 2. Touches / leaves out
Touches: `Domain/Company` (the identity values and their validation on the `Company` aggregate), `Application/Company` (set identity, upload/replace/remove logo and signature, the read capability), the company controller, the message catalogue, persistence and migration, `docs/Code_Map.md`.
Leaves out: number series (a later feature, Q11); any use of the identity by a document (sales#07). Schema change: yes (columns on `company.company`, additive).

## 3. Approach
The identity lives on the existing `Company` aggregate (D12: one logo, one signature, one bank account); the two images are `bytea` columns with their content type, stored in the database so one backup covers them (Q18). The images are validated by content type and size (PNG/JPEG up to 2 MB) at the boundary and again in the domain; the read capability returns URLs and values, empty when nothing is set, never an error (R20, AC20). The `GET .../logo` and `GET .../signature` routes stream the bytes with the stored content type and a cache header. Roles: the Owner of the company or an entered Super Admin (D11); Staff is `403`.

## 4. Tasks (at most 8)
- [x] 1. Domain: the identity values on `Company` (bank account, terms, footer, print GSTIN) with their validation (R19, R20).
- [x] 2. Domain: set, replace and remove the logo and the signature, with the content-type and size rules (R17, R18).
- [x] 3. Persistence and migration (columns on `company.company`); show the SQL and target before applying.
- [x] 4. Use cases and controller: `PUT companies/{id}/invoice-identity`, `POST`/`DELETE`/`GET` for logo and signature, roles (R17-R20, D11).
- [x] 5. The read capability `GET companies/current/invoice-identity` and the company capability other modules call (R20).
- [x] 6. Catalogue; build (0 warnings, 0 errors); `docs/Code_Map.md`.
- [x] 7. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R17`, `AC18` | 2, 4 | Owner uploads a PNG logo -> 200; `GET companies/current/invoice-identity` returns its `logoUrl`; `GET companies/{id}/logo` returns `image/png`; a `.txt` -> 400 `company.logo_invalid`; DELETE -> 204 and an empty `logoUrl` **Observed (2026-11-15, dev DB `book_inventory`, temp API on :5199):** `POST companies/{id}/logo` (1x1 PNG) -> 200; `GET current/invoice-identity` -> `"logoUrl":"/api/v1/companies/01a1d9cb-.../logo"`; `GET companies/{id}/logo` -> 200 `Content-Type: image/png`, 70 bytes; `POST` a `.txt` -> 400 `company.logo_invalid` ("The logo must be a PNG or JPEG image up to 2 MB."); `DELETE` -> 204 and `"logoUrl":null` |
| `R19`, `R20`, `AC19` | 1, 4, 5 | Owner sets bank + settings -> 200 and `GET companies/current/invoice-identity` returns them; Staff caller -> 403; Owner of A on B's id -> 404 **Observed:** `PUT companies/{id}/invoice-identity` -> 200; `GET` -> the bank account back with `ifsc` upper-cased (`sbin0001234` -> `SBIN0001234`) and `printGstin:false`; Staff `childrenbookcenter.one` on its own company -> 403; Owner A on B's id -> 404 `company.not_found`; Owner A `GET companies/{B}/logo` -> 404 |
| `R20`, `AC20` | 5 | A company with nothing set -> 200 with empty values and `printGstin` true, never 404/500 **Observed:** before anything was set, `GET companies/current/invoice-identity` -> 200 `{"logoUrl":null,"signatureUrl":null,"bank":{all null},"invoice":{"terms":null,"footer":null,"printGstin":true}}` |
| `R18` (task 2, 4) | 2, 4 | Same round-trip as the logo for the signature **Observed:** `POST companies/{id}/signature` (PNG) -> 200; `GET companies/{id}/signature` -> 200 `image/png`, 70 bytes; a `.txt` -> 400 `company.signature_invalid`; `DELETE` -> 204; `GET` after delete -> 404 `company.image_not_found` |
| `R20` (no company context) | 5 | `GET companies/current/invoice-identity` with no company context -> 403 `core.company_required` (never 500) **Observed (Application layer, `CompanyQueryService` with a null `ICurrentCompany`):** `ForbiddenException code=core.company_required` |

## 6. Not verified
Nothing outstanding. The only check not run over HTTP is the no-company-context `403`, because the seeded Super Admin's password is not recorded (B27); the guard is the same `RequireCurrentAsync` the already-verified `GET companies/current` uses, and it was exercised directly against `CompanyQueryService` (see section 5). Storing the images in the database grows the `company.company` row (bounded by the 2 MB cap) - accepted for Phase 1, noted under Risks.

## 7. Risks and notes
Additive migration. Storing images in the database keeps backups simple (Q18) but grows the row; a later move to disk or object storage changes only the storage, not the rules. The terms length (4000) and the 2 MB image cap are the spec's; larger limits need an owner decision.
