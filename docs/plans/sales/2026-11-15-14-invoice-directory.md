# Plan sales#14: Invoice directory for the payment module

- **File:** `docs/plans/sales/2026-11-15-14-invoice-directory.md`
- **Kind:** Backend
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R37; AC30
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `sales#03` (Done)
- **UI plan:** none, a read capability with no screen of its own
- **Approved by / on:** abhishek / 2026-11-15 (chat: "yes all approve hai"; Q1-Q7 answered with the recommended answers)
- **Created:** 2026-11-15

## 1. In one minute
After this plan the payment module can ask sales for the open invoices of a school (number, date, total) and for the facts of given invoice ids. Nothing changes on any screen.

## 2. Touches / leaves out
Touches: `Application/Sales` (the capability interface and its implementation over the invoice reads), the module registration. No schema change.
Leaves out: any payment behaviour (payment#01).

## 3. Approach
A small read-only capability in the same shape as the other directories (school R16, item R16): invoices of one school with status `ISSUED` oldest first, and facts by ids with their status; both limited to the session company by the platform filter.

## 4. Tasks (at most 8)
- [ ] 1. The capability and its records (invoice id, number, date, session, school, grand total, status).
- [ ] 2. The implementation over the invoice store, not tracked; build at 0 warnings.
- [ ] 3. Verify AC30 with a scratch console app against the local `book_inventory`.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R37`, `AC30` | 1-3 | A scratch run resolves the capability for a test school with two issued and one cancelled invoice: lists the two oldest first; by ids returns three with the cancelled one marked; another company's id is absent |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
No migration.
