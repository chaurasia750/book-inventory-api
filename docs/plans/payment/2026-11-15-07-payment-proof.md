# Plan payment#07: Optional proof of payment

- **File:** `docs/plans/payment/2026-11-15-07-payment-proof.md`
- **Kind:** Backend
- **Spec:** [../../specs/payment.md](../../specs/payment.md) - rules covered: R20; AC15
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Base branch:** feature/payment-06
- **Depends on:** `payment#01`
- **UI plan:** payment#08
- **Approved by / on:** abhishek / 2026-11-15 (chat: "yes all approve hai"; Q1-Q7 answered with the recommended answers)
- **Created:** 2026-11-15

## 1. In one minute
After this plan the Owner can attach up to five proof files (cheque photo, UPI screenshot, bank slip) to a receipt, see them and remove them; a receipt without proof works as before.

## 2. Touches / leaves out
Touches: the `payment` module (`receipt_proof` child of `Receipt`, three endpoints), one additive migration (`payment.receipt_proof`), the message catalogue; the same image rules style as the company logo (no new package).
Leaves out: screens (payment#08); the proof is never printed.

## 3. Approach
`Receipt.AttachProof` checks the type (PNG, JPEG, PDF by content, not only by name), the size (5 MB), the count (5) and that the receipt is `RECORDED`; the file is stored as `bytea` in the child row; a removal soft-deletes it. The migration SQL is shown before it is applied to the local `book_inventory`.

## 4. Tasks (at most 8)
- [x] 1. Domain: `ReceiptProof` and the attach and remove behaviours with their limits (R20).
- [x] 2. Persistence: configuration and migration (show the SQL, apply to the local `book_inventory`).
- [x] 3. Use cases and the three endpoints (multipart upload, read with the content type, remove); roles; catalogue.
- [x] 4. Build at 0 warnings; verify AC15 on the local database.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R20`, `AC15` | 1-3 | PNG proof -> 201 and the read returns the same bytes as `image/png`; a `.txt`, a 6 MB file and a sixth file -> 400 `payment.proof_invalid`; remove -> read 404; a cancelled receipt takes no proof; Staff 403**Observed (local book_inventory, scratch API):** a receipt with no proof: `proofs: []`; PNG proof -> 201 `{fileName: cheque-photo.png, contentType: image/png, sizeBytes: 148}` and the read returns the same 148 bytes as `image/png`; a PDF (application/pdf) and a JPEG (image/jpeg) are accepted; the three rows are in `payment.receipt_proof` as `bytea` with `size_bytes` equal to `octet_length(content)`; a text file renamed to `.png` (judged by its bytes), a file of 5 MB + 10 bytes with a PNG signature, and an empty file -> 400 (`payment.proof_invalid` for the first two); the fifth proof is accepted and the sixth -> 400 `payment.proof_invalid`; the detail lists the five by file name, type and size, oldest first, with no bytes; removing one -> 204, the read -> 404, the row stays with `is_deleted = true`, and a new proof fits again; removing an unknown proof -> 404; Staff -> 403 on upload, read and remove; the Owner of ABC -> 404; a cancelled receipt takes no proof -> 409 `payment.receipt_not_recorded`; a receipt with proofs still cancels and its proofs stay readable. 19 of 19 checks passed. |

## 6. Not verified
Migration `20261115215913_Payment_ReceiptProof` (`CREATE TABLE payment.receipt_proof` with its foreign key and index) was shown before it ran and applied at the API startup to the local `book_inventory` only. There is no upload time on a proof (a child row has no audit columns); the list is in id order (UUID v7, so oldest first); spec data corrected (no `uploaded_at`). A cancelled receipt answers 409 (the spec said only that it takes no new proof). Not exercised: uploads at the exact limits (5 MB exactly, a file name of 200+ characters), many receipts with proofs at once (memory use of five 5 MB files per receipt read), the default `dotnet build book-inventory.slnx` (blocked by the owner's running API): the API project was built into a scratch folder: 0 warnings, 0 errors. The test receipt of this check (cancelled, with 4 proof files) stays on a deactivated test school.

## 7. Risks and notes
Schema change: one additive table, `payment.receipt_proof`. Rollback note: dropping `payment.receipt_proof` loses only the proof files.
