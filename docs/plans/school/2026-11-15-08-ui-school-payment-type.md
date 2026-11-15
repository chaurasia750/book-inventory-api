# Plan school#08: UI school payment type

- **File:** `docs/plans/school/2026-11-15-08-ui-school-payment-type.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/school.md](../../specs/school.md) - rules covered: R20 (the screen side); AC16
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `school#07`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-15 (chat: Cash/Credit memo answered "recommended"; the owner asked to modify the existing invoice PDF spec and plans)
- **Created:** 2026-11-15

## 1. In one minute
After this plan the school form has a Payment field (Cash / Credit) and the school profile shows it. Open Schools, add or edit a school.

## 2. Touches / leaves out
Touches: `apps/member` school form and profile, models, i18n.
Leaves out: the order editor (sales#10).

## 3. Approach
One select beside the sale type in the shared school form (add and edit); the profile prints the value in its details.

## 4. Tasks (at most 8)
- [ ] 1. Model and request carry `paymentType`.
- [ ] 2. The Payment select in the school form (required `*`, default Credit) and its text in the profile.
- [ ] 3. i18n.
- [ ] 4. Build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R20`, `AC16` | 1-3 | Add a school with Cash, open its profile: Cash; edit to Credit |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Written without a browser.
