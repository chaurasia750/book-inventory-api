# Plan school#05: Session of a date and the company cut-offs

- **File:** `docs/plans/school/2026-11-15-05-session-of-a-date.md`
- **Kind:** Backend
- **Spec:** [../../specs/school.md](../../specs/school.md) - rules covered: R18, R19; AC14, AC15
- **Owner:** abhishek
- **Status:** Done (2026-11-15)
- **Depends on:** `school#01` (Done)
- **UI plan:** school#06
- **Approved by / on:** abhishek / 2026-11-15 (chat: sales Q1-Q14 answered one by one)
- **Created:** 2026-11-15
- **Base branch:** `docs/sales-order-invoice` (one branch `feature/sales` for company#06, price#05, stock#05, school#05, sales#01 and sales#03, the owner's instruction "do it all")

## 1. In one minute
After this plan the school module answers "which session is this date for this school" (and the sessions before and after), using the company's two cut-offs, which the Owner can change. Try it: `GET /api/v1/school-settings/session-cut-offs`.

## 2. Touches / leaves out
Touches: `BookInventory.Domain/School` (the session rule as a value object, the cut-off settings aggregate), `Application/School` (capability `ISchoolSessions`, settings use cases), `Infrastructure` (configuration, migration for the settings table), `Api/Controllers/School` (GET/PUT the cut-offs).
Leaves out: the order screens (sales#02). Schema change: yes (a small `school.session_setting` table, additive).

## 3. Approach
The derivation is one pure domain rule (session type, date, cut-offs -> label, previous, next). The cut-offs are a company-owned settings row created with the BRD defaults when first changed (read returns the defaults until then). The day-and-month check lives in the settings aggregate.

## 4. Tasks (at most 8)
- [x] 1. Domain: the session rule and the settings aggregate with the defaults and the day-month check (R18, R19).
- [x] 2. Persistence and migration; show the SQL and the target `book_inventory` before applying.
- [x] 3. Capability `ISchoolSessions` (session of a school on a date, with before and after).
- [x] 4. Endpoints GET/PUT the cut-offs (Owner writes, Staff read).
- [x] 5. Build; verify section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R18`, `AC14` | 1, 3 | Scratch call with the defaults: Apr-Mar 2027-01-31 `2026-27`, 02-01 `2027-28`, 03-31 `2027-28`, 04-01 `2027-28`; Jan-Dec 2026-11-14 `2026`, 11-15 `2027`, 12-31 `2027`, 2027-01-01 `2027`; 2027-02-01 before/after `2026-27`/`2028-29`. **PASS** |
| `R19`, `AC15` | 1, 4 | GET defaults 2/1 and 11/15; PUT Apr-Mar 1 March -> 2027-02-15 is `2026-27`; 29 Feb and a Jan-Dec cut-off in April -> 400 `school.invalid_cut_off`; Staff PUT 403; B still 2/1; restored. **PASS** |

## 6. Not verified
Run by `check_sales.py` and `check_igst.py` (scratch, not committed; a temporary scratch controller for the capabilities was deleted before the commit) on the real `book_inventory`: 35/35 passed (the first IGST run failed only because an earlier check of the same script had changed the book's price; it was run again on fresh items and passed).

Migration `School_SessionCutOffs` (new table `school.session_setting`; and `core.company_code_series.prefix` widened from 10 to 20 characters for the document number series of sales, platform P23): SQL shown in the session, target the local `book_inventory`, applied by the app's start. Rollback: drop the table; the wider column is harmless.

Note: R19 now names the allowed ranges (an Apr-Mar cut-off from 1 January to 31 March, a Jan-Dec one from 1 October to 31 December), which the derivation needs.

## 7. Risks and notes
Additive migration; rollback: drop the table.
