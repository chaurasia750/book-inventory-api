# Plan payment#08: UI proof of payment

- **File:** `docs/plans/payment/2026-11-15-08-ui-payment-proof.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/payment.md](../../specs/payment.md) - rules covered: R20 (the screen side); AC15
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `payment#07`, `payment#02`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-15 (chat: "yes all approve hai"; Q1-Q7 answered with the recommended answers)
- **Created:** 2026-11-15

## 1. In one minute
After this plan the receive form has an optional "Proof of payment" file field and the receipt screen lists the proof files to view, add and remove.

## 2. Touches / leaves out
Touches: `apps/member` (`pages/payment`), service, i18n.
Leaves out: the PDF.

## 3. Approach
The form can pick up to five PNG, JPEG or PDF files (type and size checked before the upload) and uploads them after the receipt is recorded; the receipt screen shows the file names, opens a file in a new tab and removes it with a confirm; a wrong file shows the catalogue text.

## 4. Tasks (at most 8)
- [ ] 1. Service calls (upload, read, remove).
- [ ] 2. The optional field in the receive form.
- [ ] 3. The proof list on the receipt screen (view, add, remove) with its states.
- [ ] 4. i18n; build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R20`, `AC15` | 1-3 | Owner records a receipt with a proof photo and without; opens and removes it; a `.txt` file shows the refusal text |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Written without a full browser session.
