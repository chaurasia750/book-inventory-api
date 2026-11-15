# Plan sales#15: School counters and student records

- **File:** `docs/plans/sales/2026-11-16-15-counters-and-students.md`
- **Kind:** Backend
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R38-R41, R48; AC31-AC34
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Depends on:** none (school directory R16, session R18 and the company code series exist)
- **UI plan:** sales#16
- **Approved by / on:** abhishek / 2026-11-16 (chat: "all plan approve hai, ek hi branch pe")
- **Created:** 2026-11-16

## 1. In one minute
After this plan the Owner can open a counter for a school that sells at the counter, close and open it again, and the counter bill (a later plan) can find or create a student by name and class; typing two letters of a name suggests the school's students. Try: as an Owner `POST /api/v1/counters` with `{name, schoolId, startDate}`, then `GET /api/v1/students/suggest?schoolId=...&text=rav`.

## 2. Touches / leaves out
Touches: `Domain/Sales` (`Counter` and `Student` aggregates, the counter status, message codes), `Application/Sales` (counter use cases, the student find-or-create capability for the bill, the suggestion, contracts, catalogue), `Infrastructure/Sales` and `Persistence` (two tables, configurations, migration), `Api/Controllers/Sales` (counters and students endpoints), `ISchoolDirectory` (the reference gets the sale type; additive), `docs/Code_Map.md`.
Leaves out: the bill (sales#17), every screen (sales#16), the day summary and the receipt.

## 3. Approach
Counter and Student derive from `CompanyOwnedRoot` like the other masters. The counter checks the school through the school directory (active, sale type allows counters, R38) and takes the session from school R18; closing and opening are root behaviours (R39). The student keeps its identity rule (school + normalised name + class, R40) in one domain place and exposes one find-or-create use case that the bill calls inside its own transaction (R41); the suggestion is a no-tracking query capped at 20. The class is checked and spelled through the school directory (school R16), the mobile with the shared contact rule.
Schema change: yes (two new tables in the existing schema `sales`, additive); no other schema-changing sales plan is open.

## 4. Tasks (at most 8)
- [x] 1. Domain: `Counter` (create, edit, close, open; school change locked once billed through a flag the bill sets later) and `Student` (create, update optional values, identity key) with message codes (R38-R40).
- [x] 2. Persistence: configurations (indexes, the unique student identity on live rows), migration `Sales_CounterAndStudent`; show the SQL and the target `book_inventory` before applying.
- [x] 3. Counter use cases: add, edit, close, open, with the school checks and the session (R38, R39).
- [x] 4. Student find-or-create and the suggestion query (R40, R41).
- [x] 5. `CountersController` and `StudentsController` with the roles of R48 and the API table; catalogue and registration.
- [x] 6. Build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [x] 7. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R38`, `AC31` | 1, 3, 5 | Owner of A adds a counter for a `BOTH` school: 201, session of the start date; a second counter for the same school and session 201; a `SCHOOL_ORDER` school 400; end before start 400; Staff 403 |
| `R39`, `AC32` | 1, 3 | close then open: 204; closing twice 409; the school of a counter with a bill cannot change (checked with the bill in sales#17); dates never refuse |
| `R40`, `AC33` | 1, 4 | find-or-create of `Ravi Kumar` class `5th`, then `  ravi   KUMAR ` same class reuses it, class `6th` makes another; SQL shows one row per identity and the section stored as the latest |
| `R41`, `R48`, `AC34` | 4, 5 | suggest `rav` for school S: only S's students, at most 20, class filter works, 1 character empty, company B sees none |

## 6. Not verified
Observed (2026-11-16, scratch API on the real `book_inventory`; migration `20261115234945_Sales_CounterAndStudent` applied to it, additive: tables `sales.counter` and `sales.student`, no foreign keys, the unique student identity `(company_id, school_id, name_normalized, class_name)` on live rows):
- AC31: Owner opens a counter for an active `BOTH` school with start 2027-04-01 -> 201, session `2027-28`, ACTIVE; a second counter for the same school and session -> 201; an active `SCHOOL_ORDER` school -> 400 `sales.counter.school_not_counter`; end before start -> 400 `invalid_dates`; session `2040-41` -> 400 `invalid_session`; a 1-character name -> 400 `invalid_name`; Staff -> 403.
- AC32: close 204, again 409 `already_closed`; open 204, again 409 `already_open`; edit of name and end date 204 and read back; Staff close 403, Staff list 200; company B opening A's counter 404 `counter.not_found`; anonymous 401; list: page size 500 -> 200, search by name and by school code, status filter, unknown sort ignored.
- R41 (empty data): `students/suggest` with 1 character -> 200 empty, with 2 characters -> 200 `[]`, unknown or another company's school -> 404, no school -> 400.
Student rows closed together with sales#17 (2026-11-16, same scratch API): the bill creates and reuses students. `Ravi Kumar` class `Nursery` is created by the first bill; `  ravi   KUMAR ` in upper case with the same class reuses it (same id) and a section `B` given later is stored on it while the first bill still shows no section; the same name in class `LKG` is a second student; `students/suggest?text=ravi kum` lists both with their class, with `className` only one; a bill refused for a payment mismatch created no student; a locked school: a counter with a bill answers 409 `sales.counter.school_fixed` when its school changes, a rename keeping the school is 204; a closed counter answers 409 `sales.counter.closed` for a bill.
NOT verified: a school of company B suggested to company A with data (only the 404 for an unknown school was seen).

## 7. Risks and notes
The migration is additive (two tables in the existing schema `sales`); rollback: drop the two tables. The student find-or-create is called by the bill (sales#17); here it is checked through a scratch call only.
