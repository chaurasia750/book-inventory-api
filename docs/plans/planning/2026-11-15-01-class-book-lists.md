# Plan planning#01: Class-wise book lists

- **File:** `docs/plans/planning/2026-11-15-01-class-book-lists.md`
- **Kind:** Backend
- **Spec:** [../../specs/planning.md](../../specs/planning.md) - rules covered: R1-R9; AC1-AC5
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `school#05` (Done)
- **UI plan:** planning#02
- **Approved by / on:** abhishek / 2026-11-15 (chat: the wholesale order questions answered one by one, all with the recommended answers)
- **Created:** 2026-11-15

## 1. In one minute
After this plan the Owner keeps each school's class-wise book list per session (books, quantity per student, student estimate), copies last session's lists in one step, and sees what changed. Try it: `PUT /api/v1/schools/{id}/book-lists/2026-27/5th` then `GET /api/v1/schools/{id}/book-lists?session=2026-27`.

## 2. Touches / leaves out
Touches: new `BookInventory.Domain/Planning` (aggregate `ClassBookList` with lines), `Application/Planning` (use cases, query with the difference to the previous session, the capability `IClassBookLists` for sales R22), `Infrastructure` (schema `planning`, migration), `Api/Controllers/Planning`, the module list in `CLAUDE.project.md` (with this docs change), `docs/Code_Map.md`.
Leaves out: the requirement report and vendor order drafts (later planning features), screens (planning#02). Schema change: yes (new schema `planning`, additive).

## 3. Approach
One aggregate per school, session and class, created by the first PUT and replaced as a whole (lines and estimate). Classes come from the school directory (school R16 gains the class names) and the session from school R18 labels. Copy reads the source lists and creates the missing targets. The difference is computed per class against the previous session label (school R18).

## 4. Tasks (at most 8)
- [ ] 1. Domain: `ClassBookList` with lines, limits and the one-line-per-item rule (R2, R3, R5, R9).
- [ ] 2. Persistence and migration; show the SQL and target before applying.
- [ ] 3. Use cases: save one class, delete, copy from a session or school (R4, R6).
- [ ] 4. Query: a school's session lists with the difference to the previous session (R7); capability `IClassBookLists` for sales.
- [ ] 5. Controller, catalogue; build at 0 warnings.
- [ ] 6. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R2`, `R3`, `R6`, `R7`, `AC1` | 1, 3, 4, 5 | Save Class 5 of S 2026-27 (X 2, Y 1, estimate 40); read shows it and the other classes empty; Staff read 200, save 403 |
| `R2`, `R9`, `AC2` | 1, 3 | Unknown class, qty 0 and 51, unknown item, 201 lines -> 400; X twice -> one line |
| `R4`, `AC3` | 3 | Copy 2026-27 -> 2027-28 (no estimates); again without replace -> skipped; from school T -> only S's classes |
| `R7`, `AC4` | 4 | 2027-28 changed -> added, removed, changed marked against 2026-27 |
| `R1`, `AC5` | 5 | Owner of B on A's school -> 404 |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Additive migration; rollback: drop schema `planning`.
