# Plan company#12: Item-wise discount setting of a company

- **File:** `docs/plans/company/2026-11-15-12-item-discount-setting.md`
- **Kind:** Backend
- **Spec:** [../../specs/company.md](../../specs/company.md) - rules covered: R23; AC24
- **Owner:** abhishek
- **Status:** Done (2026-11-15)
- **Base branch:** docs/company-item-discount
- **Depends on:** `company#08` (the invoice identity, in progress for its screen only)
- **UI plan:** company#13
- **Approved by / on:** abhishek / 2026-11-15 (chat: "yes approve hai"; Q20, Q21 answered with the recommended answers)
- **Created:** 2026-11-15

## 1. In one minute
After this plan a company has an item-wise discount switch, off by default, kept with its invoice settings; the sales module can read it. Try it: `GET /api/v1/companies/current/invoice-identity` shows `usesItemDiscount: false`; `PUT /api/v1/companies/{id}/invoice-identity` with `true` keeps it.

## 2. Touches / leaves out
Touches: the company aggregate's invoice settings (one more flag), the invoice-identity read and write models, the company capability, one additive migration (`uses_item_discount boolean not null default false`).
Leaves out: what sales does with it (sales#12); the screen (company#13).

## 3. Approach
Add the flag next to `print_gstin` in the invoice settings behaviour of the company aggregate, return it in the existing read and accept it in the existing write and in the capability record. The migration is additive with default false, so every existing company is off. SQL is shown before it is applied to the local `book_inventory`.

## 4. Tasks (at most 8)
- [x] 1. The flag in the invoice settings of the company aggregate (R23).
- [x] 2. The read and write models, the capability record and the endpoint (R23).
- [x] 3. Migration `uses_item_discount` default false; show the SQL, apply to the local `book_inventory`.
- [x] 4. Build at 0 warnings; verify AC24 on the local database.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R23`, `AC24` | 1-4 | `GET` for an untouched company -> `usesItemDiscount: false`; `PUT` true -> read true; Staff -> 403; Owner of B on A's id -> 404; the column exists with default false for all rows**Observed (local book_inventory, scratch API):** migration `20261115142429_Company_ItemDiscountSetting` applied at startup (`ALTER TABLE company.company ADD uses_item_discount boolean NOT NULL DEFAULT FALSE`), all 3 companies `false`; untouched company reads `usesItemDiscount: false`; Owner PUT true -> read true with terms kept; Staff -> 403; Owner of ABC on CBC id -> 404 and ABC stays false; switched back to false afterwards. 6 of 6. |

## 6. Not verified
The `PUT` without `usesItemDiscount` sets it to false (the field defaults to false like the request model); the screen (company#13) always sends it. The sales module does not read the flag yet (sales#12).

## 7. Risks and notes
Schema change: the only open schema-changing plan of `company`. Rollback note: dropping the column loses only this flag.
