# Plan payment#04: UI cheque clear and bounce, pending cheques

- **File:** `docs/plans/payment/2026-11-15-04-ui-cheque-status.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/payment.md](../../specs/payment.md) - rules covered: R10-R12, R16 (the screen side); AC8-AC10
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Base branch:** feature/payment-03 (UI repository branch feature/payment-04)
- **Depends on:** `payment#03`, `payment#02`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-15 (chat: "yes all approve hai"; Q1-Q7 answered with the recommended answers)
- **Created:** 2026-11-15

## 1. In one minute
After this plan the receipt screen has Clear and Bounce buttons on a pending cheque (the bounce asks for a reason), and the receipts list can show the pending cheques.

## 2. Touches / leaves out
Touches: `apps/member` (`pages/payment`), service, i18n.
Leaves out: adjust, cancel, PDF.

## 3. Approach
Buttons only on a `PENDING` cheque; a small dialog with the date (and the reason for a bounce); the result updates the receipt and the school balance; errors show the catalogue text.

## 4. Tasks (at most 8)
- [x] 1. Service calls for clear and bounce.
- [x] 2. The cheque actions and their dialogs on the receipt detail.
- [x] 3. The pending-cheque filter on the list.
- [x] 4. i18n; build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R10`-`R12`, `AC8`-`AC10` | 1-3 | Owner clears and bounces cheques; the status and the school balance change; the pending filter lists the rest**Observed (headless Chrome, Owner of Children Book Center, scratch API on the dev proxy port, a test school with two 1,000 invoices each paid by a pending cheque):** the Payments tab has the checkbox "Only receipts with a pending cheque"; ticked, both receipts (RCP/2026-27/0013, 0014) stay; opening 0013 shows the cheque `900001 · 10 Nov 26 · State Bank` with status Pending and the buttons Mark cleared and Mark bounced; Mark cleared opens a form with the date (today) and Save: the cheque then reads Cleared and the buttons are gone; the list with the filter then has 1 row; opening 0014, Mark bounced, Save with no reason shows the server's refusal "The Reason field is required."; with the reason "Insufficient funds" Save: the cheque reads Bounced and the receipt shows "Not applied to any invoice." with advance 0.00. The money side (ledger, dues, invoice reopened) is proven in payment#03. UI build of 3 apps passes, lint shows only the 3 known findings. |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Seen through headless Chrome driving the real screens (not a human click-through).

## Not verified
The date rules from the screen (before the cheque date, in the future: the server refuses them, the texts were not seen), Staff not seeing the actions (the Payments tab is the Owner's), the narrow screen and the school balance on the profile after a bounce were not looked at. The test data of this check (two cheque receipts on a deactivated "ChqUI Check School") stays like the other check receipts (backlog B103).
