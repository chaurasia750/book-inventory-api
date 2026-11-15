# Plan payment#04: UI cheque clear and bounce, pending cheques

- **File:** `docs/plans/payment/2026-11-15-04-ui-cheque-status.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/payment.md](../../specs/payment.md) - rules covered: R10-R12, R16 (the screen side); AC8-AC10
- **Owner:** abhishek
- **Status:** Draft
- **Depends on:** `payment#03`, `payment#02`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** -
- **Created:** 2026-11-15

## 1. In one minute
After this plan the receipt screen has Clear and Bounce buttons on a pending cheque (the bounce asks for a reason), and the receipts list can show the pending cheques.

## 2. Touches / leaves out
Touches: `apps/member` (`pages/payment`), service, i18n.
Leaves out: adjust, cancel, PDF.

## 3. Approach
Buttons only on a `PENDING` cheque; a small dialog with the date (and the reason for a bounce); the result updates the receipt and the school balance; errors show the catalogue text.

## 4. Tasks (at most 8)
- [ ] 1. Service calls for clear and bounce.
- [ ] 2. The cheque actions and their dialogs on the receipt detail.
- [ ] 3. The pending-cheque filter on the list.
- [ ] 4. i18n; build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R10`-`R12`, `AC8`-`AC10` | 1-3 | Owner clears and bounces cheques; the status and the school balance change; the pending filter lists the rest |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Written without a full browser session.
