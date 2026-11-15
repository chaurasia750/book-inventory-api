# Plan sales#12: Item discount is a company choice (orders, invoices, PDF)

- **File:** `docs/plans/sales/2026-11-15-12-item-discount-choice.md`
- **Kind:** Backend
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R36; AC28, AC29
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `company#12`, `sales#01`, `sales#03`, `sales#07` (Done)
- **UI plan:** sales#13
- **Approved by / on:** abhishek / 2026-11-15 (chat: "yes approve hai"; Q20, Q21 answered with the recommended answers)
- **Created:** 2026-11-15

## 1. In one minute
After this plan a company with the switch off cannot put a discount on a line (it gets a clear refusal), and its invoice PDF has no discount column, only the total discount. A company with the switch on, and documents that already have line discounts, behave as before.

## 2. Touches / leaves out
Touches: the order draft and line behaviours (the refusal), the order and invoice use cases that read the company setting through the company capability, the invoice PDF model and the invoice body template (the discount column shown only when R36 says), the message catalogue. No schema change.
Leaves out: the screens (sales#13); the amounts computation (R7 is unchanged).

## 3. Approach
One rule in the domain: a line with a new item discount is refused when the company does not use it; an unchanged value is accepted. The invoice PDF shows the discount column exactly when the company setting is on (the setting alone drives it, owner 2026-11-15). The screens use "setting on, or a line has a discount" (R36), read from the order and invoice responses so that fact exists once.

## 4. Tasks (at most 8)
- [ ] 1. The refusal `sales.item_discount_disabled` for a new item discount on a draft and on a confirmed order's line (R36).
- [ ] 2. The "shows the discount column" fact on the order and invoice responses (setting on or a line has a discount).
- [ ] 3. The invoice PDF model and template show the column only when the company setting is on (no column otherwise, totals keep the total discount).
- [ ] 4. Catalogue text; build at 0 warnings.
- [ ] 5. Verify AC28 and AC29 on the local database and PDF text.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R36`, `AC28` | 1-3 | Setting off: an order with a 5% line discount -> 400 `sales.item_discount_disabled`, no order; the same with a 10% total discount -> 201 and the R7 totals; its invoice PDF text has no discount column header |
| `R36`, `AC29` | 1-3 | Setting on: a line discount is accepted, the invoice shows the column; switch off: the same invoice and order still show it on the screen with the same amounts, the PDF has no discount column, a new line discount -> 400 |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
No migration. Documents already made are read as they are; nothing is rewritten.
