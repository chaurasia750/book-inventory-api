# Plan company#13: UI item-wise discount switch in the company profile

- **File:** `docs/plans/company/2026-11-15-13-ui-item-discount-setting.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/company.md](../../specs/company.md) - rules covered: R23; AC25
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `company#12`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-15 (chat: "yes approve hai"; Q20, Q21 answered with the recommended answers)
- **Created:** 2026-11-15

## 1. In one minute
After this plan the Owner sees an "Item-wise discount" switch in the invoice settings of the Company profile (SETTINGS > Company profile) and can turn it on or off.

## 2. Touches / leaves out
Touches: `apps/member` (`pages/company`: the invoice settings tab), the company service and model, i18n.
Leaves out: the order and invoice screens (sales#13).

## 3. Approach
One more checkbox beside "Print GSTIN" in the invoice settings form, saved with the same button and the same call; success and error states are those of the form.

## 4. Tasks (at most 8)
- [ ] 1. Model and service field `usesItemDiscount`.
- [ ] 2. The switch and its help text in the invoice settings tab; i18n.
- [ ] 3. Build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R23`, `AC25` | 1-2 | Owner opens Company profile, switches Item-wise discount on, saves, reloads: still on; off again; Staff has no Company profile |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Written without a full browser session.
