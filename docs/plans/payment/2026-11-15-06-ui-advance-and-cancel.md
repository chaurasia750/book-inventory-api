# Plan payment#06: UI adjust advance and cancel receipt

- **File:** `docs/plans/payment/2026-11-15-06-ui-advance-and-cancel.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/payment.md](../../specs/payment.md) - rules covered: R13-R15 (the screen side); AC11, AC12
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Base branch:** feature/payment-05 (UI repository branch feature/payment-06)
- **Depends on:** `payment#05`, `payment#02`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-15 (chat: "yes all approve hai"; Q1-Q7 answered with the recommended answers)
- **Created:** 2026-11-15

## 1. In one minute
After this plan the receipt screen shows the advance with an Adjust button and a Cancel receipt button that asks for a reason.

## 2. Touches / leaves out
Touches: `apps/member` (`pages/payment`), service, i18n.
Leaves out: the PDF.

## 3. Approach
The adjust dialog repeats the oldest-first preview of the receive form; cancel is a confirm dialog with the reason; a cancelled receipt shows its status and no actions.

## 4. Tasks (at most 8)
- [x] 1. Service calls.
- [x] 2. Adjust advance dialog.
- [x] 3. Cancel dialog and the cancelled state.
- [x] 4. i18n; build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R13`-`R15`, `AC11`, `AC12` | 1-3 | Owner adjusts an advance and cancels a receipt; invoice dues and the school balance follow; a receipt with a cleared cheque shows the refusal text**Observed (headless Chrome, Owner of Children Book Center, scratch API on the dev proxy port, a test school with invoices 1,000, 500 and 1,000; a cash receipt of 1,500 with 1,000 applied; a cheque receipt of 1,000 with a cleared cheque):** the cash receipt shows Advance 500.00 and the buttons Adjust advance and Cancel receipt; Adjust advance opens a panel "Apply the advance to invoices: 500.00" with the open invoice INV/2026-27/0039 (due 500.00, applied 500.00) and the by-hand checkbox; Apply: the receipt then lists INV/2026-27/0038 1,000.00 and INV/2026-27/0039 500.00, Advance 0.00 and no Adjust button; Cancel receipt with no reason shows the server refusal "The Reason field is required."; with "Entered by mistake" it shows status Cancelled, "Not applied to any invoice." and "Cancelled on 16 Nov 2026: Entered by mistake"; the list shows RCP/2026-27/0018 Cancelled; the cheque receipt's cancel shows "A receipt with a cleared or bounced cheque cannot be cancelled.". UI build of 3 apps passes, lint shows only the 3 known findings. |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Seen through headless Chrome driving the real screens (not a human click-through).

## Not verified
Adjusting by hand (the by-hand checkbox), the narrow screen, the school balance on the profile after an adjust or a cancel, and Staff not seeing the actions (the Payments tab is the Owner's) were not looked at. The test data of this check stays: one cancelled receipt (RCP/2026-27/0018) and one with a cleared cheque (0019, which R14 does not let be cancelled; backlog B103).
Found while checking: a cancelled receipt showed its whole total as "Advance"; the API now reports 0 for a cancelled receipt (a one-line rule in the receipt; payment R14 says so).
