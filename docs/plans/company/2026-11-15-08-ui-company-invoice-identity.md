# Plan company#08: UI company invoice identity

- **File:** `docs/plans/company/2026-11-15-08-ui-company-invoice-identity.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/company.md](../../specs/company.md) - rules covered: R17-R20 (the screen side); AC18-AC20
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `company#07`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-15 (the invoice PDF approval, Q17-Q18 with the recommended answers)
- **Created:** 2026-11-15
- **Base branch:** `main` of `book-inventory-ui` (to be cut as `feature/company-ui-08` when approved)

## 1. In one minute
After this plan the Owner sees a **Company profile** screen: upload, replace or remove the **logo** and the **signature image** with a preview, fill the one **bank account** (account holder, bank, account number, IFSC, branch, UPI id) and the **invoice settings** (terms and conditions, footer, print GSTIN). A Super Admin inside an entered company sees the same screen. Open it from the company menu.

## 2. Touches / leaves out
Touches: `apps/member` (`pages/company/profile`: the screen, the two image pickers with preview and remove, the bank form, the settings form), the service, models and i18n.
Leaves out: number series; the Super Admin console's company create/edit (company#02). Nothing of the invoice itself (sales#08).

## 3. Approach
Following the existing member screens and the owner's document pattern: one page with a header card, the two image fields (preview, replace, remove), the bank form and the settings form, each saving through the endpoints of company#07. Image validation is shown before upload (PNG/JPEG, up to 2 MB). The screen is visible to the Owner and to an entered Super Admin only. Every state shows loading, empty, error with retry and success.

## 4. Tasks (at most 8)
- [ ] 1. Service and models for the identity, the logo and the signature (company#07 endpoints).
- [ ] 2. Company profile screen: the logo and the signature (upload, replace, remove, preview).
- [ ] 3. Bank account and invoice settings forms with save and validation.
- [ ] 4. Menu entry and roles: the screen for the Owner (and an entered Super Admin); hidden for Staff.
- [ ] 5. Build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R17`, `R18`, `AC18` | 2 | Owner uploads a logo, sees the preview, replaces and removes it; a `.txt` shows the invalid-type error |
| `R19`, `R20`, `AC19`, `AC20` | 1, 3, 4 | Owner saves the bank account and settings and sees them on reopening; a company with nothing set shows empty fields; a Staff login does not see the screen |

## 6. Not verified
To be filled when the plan is closed (the owner's look in the browser).

## 7. Risks and notes
Written without a browser.
