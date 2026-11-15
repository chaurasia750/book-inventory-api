# Plan school#07: School payment type (Cash or Credit)

- **File:** `docs/plans/school/2026-11-15-07-school-payment-type.md`
- **Kind:** Backend
- **Spec:** [../../specs/school.md](../../specs/school.md) - rules covered: R20; AC16
- **Owner:** abhishek
- **Status:** Approved
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
- [ ] 1. Domain: `PaymentType` (`CASH`, `CREDIT`) and the school behaviour that sets it, default `CREDIT` (R20).
- [ ] 2. Persistence and migration; show the SQL and target before applying.
- [ ] 3. Requests, detail response and `SchoolReference` carry it (R16, R20).
- [ ] 4. Controller binding, catalogue; build at 0 warnings.
- [ ] 5. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R20`, `AC16` | 1-4 | Owner POST with `CASH` -> `GET schools/{id}` shows `CASH`; POST without -> `CREDIT`; an unknown value -> 400; Staff PUT -> 403; the directory reference shows the value |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Additive migration; rollback: drop the column.
