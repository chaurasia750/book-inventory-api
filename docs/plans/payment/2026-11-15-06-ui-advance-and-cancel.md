# Plan payment#06: UI adjust advance and cancel receipt

- **File:** `docs/plans/payment/2026-11-15-06-ui-advance-and-cancel.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/payment.md](../../specs/payment.md) - rules covered: R13-R15 (the screen side); AC11, AC12
- **Owner:** abhishek
- **Status:** Draft
- **Depends on:** `payment#05`, `payment#02`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** -
- **Created:** 2026-11-15

## 1. In one minute
After this plan the receipt screen shows the advance with an Adjust button and a Cancel receipt button that asks for a reason.

## 2. Touches / leaves out
Touches: `apps/member` (`pages/payment`), service, i18n.
Leaves out: the PDF.

## 3. Approach
The adjust dialog repeats the oldest-first preview of the receive form; cancel is a confirm dialog with the reason; a cancelled receipt shows its status and no actions.

## 4. Tasks (at most 8)
- [ ] 1. Service calls.
- [ ] 2. Adjust advance dialog.
- [ ] 3. Cancel dialog and the cancelled state.
- [ ] 4. i18n; build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R13`-`R15`, `AC11`, `AC12` | 1-3 | Owner adjusts an advance and cancels a receipt; invoice dues and the school balance follow; a receipt with a cleared cheque shows the refusal text |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Written without a full browser session.
