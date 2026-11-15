# Plan school#05: Session of a date and the company cut-offs

- **File:** `docs/plans/school/2026-11-15-05-session-of-a-date.md`
- **Kind:** Backend
- **Spec:** [../../specs/school.md](../../specs/school.md) - rules covered: R18, R19; AC14, AC15
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `school#01` (Done)
- **UI plan:** school#06
- **Approved by / on:** abhishek / 2026-11-15 (chat: sales Q1-Q14 answered one by one)
- **Created:** 2026-11-15

## 1. In one minute
After this plan the school module answers "which session is this date for this school" (and the sessions before and after), using the company's two cut-offs, which the Owner can change. Try it: `GET /api/v1/school-settings/session-cut-offs`.

## 2. Touches / leaves out
Touches: `BookInventory.Domain/School` (the session rule as a value object, the cut-off settings aggregate), `Application/School` (capability `ISchoolSessions`, settings use cases), `Infrastructure` (configuration, migration for the settings table), `Api/Controllers/School` (GET/PUT the cut-offs).
Leaves out: the order screens (sales#02). Schema change: yes (a small `school.session_setting` table, additive).

## 3. Approach
The derivation is one pure domain rule (session type, date, cut-offs -> label, previous, next). The cut-offs are a company-owned settings row created with the BRD defaults when first changed (read returns the defaults until then). The day-and-month check lives in the settings aggregate.

## 4. Tasks (at most 8)
- [ ] 1. Domain: the session rule and the settings aggregate with the defaults and the day-month check (R18, R19).
- [ ] 2. Persistence and migration; show the SQL and the target `book_inventory` before applying.
- [ ] 3. Capability `ISchoolSessions` (session of a school on a date, with before and after).
- [ ] 4. Endpoints GET/PUT the cut-offs (Owner writes, Staff read).
- [ ] 5. Build; verify section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R18`, `AC14` | 1, 3 | Throw-away call with the defaults: the eight dates of AC14 give their sessions, with before and after |
| `R19`, `AC15` | 1, 4 | PUT Apr-Mar 1 March -> 2027-02-15 gives 2026-27; 29 Feb and 31 Apr -> 400; Staff PUT 403; B unchanged |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Additive migration; rollback: drop the table.
