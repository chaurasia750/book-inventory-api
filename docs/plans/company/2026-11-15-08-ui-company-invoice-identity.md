# Plan company#08: UI company invoice identity

- **File:** `docs/plans/company/2026-11-15-08-ui-company-invoice-identity.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/company.md](../../specs/company.md) - rules covered: R17-R21 (the screen side); AC18-AC20, AC22
- **Owner:** abhishek
- **Status:** In progress (code done; waiting for the owner's look in the browser)
- **Depends on:** `company#07`, `company#10`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-15 (the invoice PDF approval, Q17-Q18 with the recommended answers)
- **Created:** 2026-11-15
- **Base branch:** `main` of `book-inventory-ui` (branch `feature/company-profile-ui`)

## 1. In one minute
After this plan the Owner sees a **Company profile** screen: the company's **address, mobile, email, GSTIN, PAN and tagline** to edit (the name and the state shown, not editable), upload, replace or remove the **logo** and the **signature image** with a preview, fill the one **bank account** (account holder, bank, account number, IFSC, branch, UPI id) and the **invoice settings** (terms and conditions, footer, print GSTIN). A Super Admin inside an entered company sees the same screen. Open it from the company menu.

## 2. Touches / leaves out
Touches: `apps/member` (`pages/company/profile`: the screen, the two image pickers with preview and remove, the bank form, the settings form), the service, models and i18n.
Leaves out: number series; the Super Admin console's company create/edit (company#02). Nothing of the invoice itself (sales#08).

## 3. Approach
Following the existing member screens and the owner's document pattern: one page with a header card, the two image fields (preview, replace, remove), the bank form and the settings form, each saving through the endpoints of company#07. Image validation is shown before upload (PNG/JPEG, up to 2 MB). The screen is visible to the Owner and to an entered Super Admin only. Every state shows loading, empty, error with retry and success.

## 4. Tasks (at most 8)
- [x] 1. Service and models for the identity, the logo and the signature (company#07 endpoints).
- [x] 2. Company profile screen: the logo and the signature (upload, replace, remove, preview).
- [x] 3. The profile form (address, mobile, email, GSTIN, PAN, tagline), the bank account form and the invoice settings form, each with save and validation.
- [x] 4. Menu entry and roles: the screen for the Owner (and an entered Super Admin); hidden for Staff.
- [ ] 5. Under the terms box a hint lists the placeholders the invoice fills (sales R35).
- [ ] 6. Build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R17`, `R18`, `AC18` | 2 | Owner uploads a logo, sees the preview, replaces and removes it; a `.txt` shows the invalid-type error |
| `R21`, `AC22` | 1, 3 | Owner edits the address, GSTIN and tagline and sees them on reopening; the name and the state are shown read-only; an invalid email shows the server's message |
| `R19`, `R20`, `AC19`, `AC20` | 1, 3, 4 | Owner saves the bank account and settings and sees them on reopening; a company with nothing set shows empty fields; a Staff login does not see the screen |

## 6. Not verified
Every row of section 5 is **NOT verified** in a browser (the assistant has none): the owner opens SETTINGS > Company profile, edits the address, GSTIN and tagline, uploads a logo and a signature (and tries a `.txt` and a file over 2 MB), removes one, saves the bank account (and tries 3 of 5 values, an 8-digit number, a wrong IFSC), saves the terms, the footer and the print-GSTIN box, reopens the page, and logs in as Staff (no menu entry; the URL is refused).

Done and observed: models and `CompanyService` (profile, edit profile, invoice identity, set identity, upload, remove, image as a blob); `pages/company`: `company-profile` (loads the session company, the profile and the identity together; loading, error with retry), `company-details-form` (name and state read-only, six editable values), `company-images` (preview from an authenticated fetch, choose with PNG/JPEG and 2 MB checked before the upload, remove), `company-bank-form` (five values all or none, account number 9-18 digits, IFSC pattern, UPI), `company-invoice-settings` (terms, footer, print GSTIN); route `/company-profile` for the Owner and the menu entry SETTINGS > Company profile. `npx nx run-many -t build` -> "Successfully ran target build for 3 projects"; `npx nx run member:lint` -> only the three known findings. The calls the screen makes were exercised through the API: the identity read, the authenticated logo download (200 `image/png`, bytes start with the PNG signature) and the company id from `companies/current`.

## 7. Risks and notes
Written without a browser.
