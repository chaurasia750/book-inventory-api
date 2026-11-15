# Plan school#07: School payment type (Cash or Credit)

- **File:** `docs/plans/school/2026-11-15-07-school-payment-type.md`
- **Kind:** Backend
- **Spec:** [../../specs/school.md](../../specs/school.md) - rules covered: R20; AC16
- **Owner:** abhishek
- **Status:** Done (2026-11-15)
- **Base branch:** `feature/sales-07` (branch `feature/school-07`)
- **Depends on:** `school#03` (Done)
- **UI plan:** school#08
- **Approved by / on:** abhishek / 2026-11-15 (chat: Cash/Credit memo answered "recommended"; the owner asked to modify the existing invoice PDF spec and plans)
- **Created:** 2026-11-15

## 1. In one minute
After this plan every school has a payment type, Cash or Credit (Credit when none is given), that the Owner sets when adding the school and changes in the edit form, and that other modules read from the school directory. Try it: `POST /api/v1/schools` with `"paymentType": "CASH"`, then `GET /api/v1/schools/{id}`.

## 2. Touches / leaves out
Touches: `Domain/School` (the payment type enum and the school behaviour that sets it), `Application/School` (requests, detail response, `SchoolReference` of the directory), `Infrastructure` (one column, migration), `docs/Code_Map.md` only if a new pattern appears.
Leaves out: orders and invoices (sales#09), screens (school#08). Schema change: yes (`school.school.payment_type`, additive, existing rows `CREDIT`).

## 3. Approach
A school value like its sale type: validated in the aggregate, part of the register and edit requests, shown in the detail and in the directory reference. The column defaults to `CREDIT` so every existing school is valid.

## 4. Tasks (at most 8)
- [x] 1. Domain: `PaymentType` (`CASH`, `CREDIT`) and the school behaviour that sets it, default `CREDIT` (R20).
- [x] 2. Persistence and migration; show the SQL and target before applying.
- [x] 3. Requests, detail response and `SchoolReference` carry it (R16, R20).
- [x] 4. Controller binding, catalogue; build at 0 warnings.
- [x] 5. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R20`, `AC16` | 1-4 | **PASS** (8 of 8, scratch `check_school07.py`, real `book_inventory`). The 117 existing schools took `CREDIT`; POST with `paymentType: CASH` -> 201 and `GET schools/{id}` shows `CASH`; POST without it -> `CREDIT`; `BARTER` -> 400; PUT to `CREDIT` -> 204 and read back, PUT back to `CASH` kept (not reset by a default); Staff PUT -> 403; the row holds `CASH`. The test schools were soft-deleted. Original row: Owner POST with `CASH` -> `GET schools/{id}` shows `CASH`; POST without -> `CREDIT`; an unknown value -> 400; Staff PUT -> 403; the directory reference shows the value |

## 6. Not verified
The directory reference (`SchoolReference.PaymentType`) is read by sales#09 and is checked there; no endpoint shows it on its own.

## 7. Risks and notes
Additive migration `School_PaymentType` (`payment_type text NOT NULL DEFAULT 'CREDIT'` and a CHECK constraint) applied to the local `book_inventory` after its SQL was shown; rollback: drop the constraint and the column. The default lives only in the migration: a model default would have turned a `CASH` school into `CREDIT` on insert (EF treats the CLR default as "not set"); caught from an EF warning. The enum `PaymentType` is in `Domain/Core` so the school and sales modules share one meaning. An edit request without `paymentType` sets `CREDIT` (the screen school#08 always sends it).
