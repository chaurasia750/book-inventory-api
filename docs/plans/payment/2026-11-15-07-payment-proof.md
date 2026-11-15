# Plan payment#07: Optional proof of payment

- **File:** `docs/plans/payment/2026-11-15-07-payment-proof.md`
- **Kind:** Backend
- **Spec:** [../../specs/payment.md](../../specs/payment.md) - rules covered: R20; AC15
- **Owner:** abhishek
- **Status:** Draft
- **Depends on:** `payment#01`
- **UI plan:** payment#08
- **Approved by / on:** -
- **Created:** 2026-11-15

## 1. In one minute
After this plan the Owner can attach up to five proof files (cheque photo, UPI screenshot, bank slip) to a receipt, see them and remove them; a receipt without proof works as before.

## 2. Touches / leaves out
Touches: the `payment` module (`receipt_proof` child of `Receipt`, three endpoints), one additive migration (`payment.receipt_proof`), the message catalogue; the same image rules style as the company logo (no new package).
Leaves out: screens (payment#08); the proof is never printed.

## 3. Approach
`Receipt.AttachProof` checks the type (PNG, JPEG, PDF by content, not only by name), the size (5 MB), the count (5) and that the receipt is `RECORDED`; the file is stored as `bytea` in the child row; a removal soft-deletes it. The migration SQL is shown before it is applied to the local `book_inventory`.

## 4. Tasks (at most 8)
- [ ] 1. Domain: `ReceiptProof` and the attach and remove behaviours with their limits (R20).
- [ ] 2. Persistence: configuration and migration (show the SQL, apply to the local `book_inventory`).
- [ ] 3. Use cases and the three endpoints (multipart upload, read with the content type, remove); roles; catalogue.
- [ ] 4. Build at 0 warnings; verify AC15 on the local database.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R20`, `AC15` | 1-3 | PNG proof -> 201 and the read returns the same bytes as `image/png`; a `.txt`, a 6 MB file and a sixth file -> 400 `payment.proof_invalid`; remove -> read 404; a cancelled receipt takes no proof; Staff 403 |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Schema change: one additive table. Rollback note: dropping `payment.receipt_proof` loses only the proof files.
