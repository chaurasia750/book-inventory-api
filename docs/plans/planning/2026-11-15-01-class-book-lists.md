# Plan planning#01: Class-wise book lists

- **File:** `docs/plans/planning/2026-11-15-01-class-book-lists.md`
- **Kind:** Backend
- **Spec:** [../../specs/planning.md](../../specs/planning.md) - rules covered: R1-R9; AC1-AC5
- **Owner:** abhishek
- **Status:** Done (2026-11-15)
- **Base branch:** `docs/orders-wholesale` (branch `feature/planning-01`)
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
- [x] 1. Domain: `ClassBookList` with lines, limits and the one-line-per-item rule (R2, R3, R5, R9).
- [x] 2. Persistence and migration; show the SQL and target before applying.
- [x] 3. Use cases: save one class, delete, copy from a session or school (R4, R6).
- [x] 4. Query: a school's session lists with the difference to the previous session (R7); capability `IClassBookLists` for sales.
- [x] 5. Controller, catalogue; build at 0 warnings.
- [x] 6. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R2`, `R3`, `R6`, `R7`, `AC1` | 1, 3, 4, 5 | Owner PUT `schools/{S}/book-lists/2026-27/5th` `{studentEstimate:40, lines:[ITM-0001 x2, ITM-0003 x1]}` -> 204; GET `?session=2026-27` -> 5th has 2 books and estimate 40, no other class has a list, `previousSession` 2025-26; Staff (Children Book Center) GET 200, PUT 403. PASS |
| `R2`, `R9`, `AC2` | 1, 3 | PUT with class `13th`, qty 0, qty 51, unknown item, inactive item, 201 lines, session `2027` for an Apr-Mar school -> each 400; ITM-0001 sent twice (1 and 2) -> 204, GET shows one line with qty 3. PASS |
| `R4`, `AC3` | 3 | POST `2027-28/copy` `{fromSession:2026-27}` -> copied [5th]; GET 2027-28 5th has 2 books and no estimate; same call again -> copied [], skipped [5th]; from school T with `replaceExisting` -> copied [4th, 5th] (T's classes that S has). PASS |
| `R7`, `AC4` | 4 | 2027-28 5th saved as ITM-0001 x3 + ITM-0004 x1 against 2026-27 (ITM-0001 x2, ITM-0003 x1) -> ITM-0004 ADDED, ITM-0001 CHANGED (previous 2), removed [ITM-0003]. PASS |
| `R1`, `AC5` | 5 | Owner of Children Book Center on ABC's school: GET -> 404, PUT -> 404; no token -> 401. PASS |

## 6. Not verified
Run: 19 of 19 checks passed on the real local database `book_inventory` (scratch script, not committed). The check found a real bug (DELETE answered 500 because the lines were loaded with the list); fixed with `FindForRemovalAsync` and re-run.

NOT verified: `DELETE` of a class that has no list (404 `planning.list_not_found`) and the removal of a list as seen from the audit trail were not exercised; the child rows of a removed list stay soft-undeleted under the deleted list (same as orders). `IClassBookLists` is checked only by sales#05.

## 7. Risks and notes
Additive migration `Planning_ClassBookLists` applied to the local `book_inventory` after its SQL was shown; rollback: drop schema `planning`. `docs/Code_Map.md` gets its planning line with sales#05 (no pattern used by sales yet).
