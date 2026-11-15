# Plan payment#08: UI proof of payment

- **File:** `docs/plans/payment/2026-11-15-08-ui-payment-proof.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/payment.md](../../specs/payment.md) - rules covered: R20 (the screen side); AC15
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Base branch:** feature/payment-07 (UI repository branch feature/payment-08)
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
- [x] 1. Service calls (upload, read, remove).
- [x] 2. The optional field in the receive form.
- [x] 3. The proof list on the receipt screen (view, add, remove) with its states.
- [x] 4. i18n; build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R20`, `AC15` | 1-3 | Owner records a receipt with a proof photo and without; opens and removes it; a `.txt` file shows the refusal text**Observed (headless Chrome, Owner of Children Book Center, scratch API on the dev proxy port, a test school):** the Receive payment dialog has the field "Proof of payment (optional: cheque photo, UPI screenshot, bank slip)"; a `notes.txt` shows "A proof must be a PNG, JPEG or PDF file." and nothing is kept; six files show "At most 5 proof files can be attached."; a PNG and a PDF are accepted (their names are listed); Record payment answers "Payment recorded: RCP/2026-27/0023." and the receipt then lists cheque.png and slip.pdf with Open and Remove and the add field "Add a proof (PNG, JPEG or PDF, at most 5 MB, at most 5 files)"; a `.txt` added from the receipt shows the same refusal; Open (with the new tab stubbed in the page, because headless Chrome does not report tabs) gave the tab a blob URL whose bytes are the PNG (12 bytes, starting 137,80,78,71) with type `image/png` and no error; Remove on slip.pdf asks "Yes, remove" and then the list shows only cheque.png. UI build of 3 apps passes, lint shows only the 3 known findings. |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Seen through headless Chrome driving the real screens (not a human click-through).

## Not verified
The real new browser tab (a person should click Open and look at the file; the first version called `window.open` after the download, which popup blockers stop, so the tab is now opened at the click and filled when the file arrives, with a download as the fallback), a proof that the server refuses (a text file renamed to `.png`, which only the server can tell), a failed upload after the payment was recorded (the "recorded, but these proofs were not attached" message was not provoked), Staff not seeing the tab, and the narrow screen. The test receipt of this check was cancelled afterwards; its two proof files stay on a deactivated test school.
