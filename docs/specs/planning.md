# Spec: planning - Class-wise book lists

- **Module:** `planning` (PostgreSQL schema `planning`)
- **Status:** R1-R9 Approved by abhishek in chat on 2026-11-15 (the wholesale order questions: the class set is the school's class-wise list of a session, qty = students x per-student qty, copy from the last session)
- **Last updated:** 2026-11-15
- **Source docs:** [BRD §4.7 Class-wise Book List and Planning (FR-PLN-01 to FR-PLN-06, FR-PLN-10)](../bussiness/BRD-Book-Inventory-Phase1.md); [specs/school.md](school.md) R6, R16, R18; [specs/item.md](item.md) R16; [specs/sales.md](sales.md) R22
- **Plans:** [planning#01](../plans/planning/2026-11-15-01-class-book-lists.md) (Done), [planning#02](../plans/planning/2026-11-15-02-ui-class-book-lists.md) (In progress)

> One spec per module. It says WHAT the module does and WHY, never implementation steps (those live in plans).

## 1. Purpose and scope

`planning` keeps what each school uses per class: the **class-wise book list** of a school's session (which books, how many per student) and the **approximate number of students** of each class. A wholesaler types a school's list once, copies it next year and changes a book or two, and adds a whole class set to an order in one step (sales R22).

Feature **Class-wise book lists** (R1-R9). Later features of this module: the requirement and shortage report and the vendor order draft from it (FR-PLN-07 to FR-PLN-09, FR-PLN-11, FR-PLN-12).

## 2. Concepts and glossary

- **Class book list**: for one school, one session and one class: its books, each with a quantity per student (FR-PLN-01).
- **Student estimate**: the approximate number of students of a school's class in a session (FR-PLN-06).

## 3. Business rules

**Class-wise book lists**
- **R1** Lists belong to exactly one company, the company of the session (platform P18-P21). Schools, classes, sessions and items are read through their capabilities (school R16, R18; item R16), never by joining their tables.
- **R2** A **class book list** is identified by school, session (school R18 labels, `2026-27` or `2027`) and class (one of the school's classes, school R6, R16). It holds lines: an **item** (active when added) and a **quantity per student** (a whole number from 1 to 50); one line per item (adding it again keeps one line). A list may hold any item, also items that are not stock managed (a school's special book, FR-PLN-03); it is only a help: orders may hold items that are not on it (FR-PLN-04).
- **R3** A list has an optional **student estimate** (a whole number from 0 to 5000, FR-PLN-06).
- **R4** **Copy** (FR-PLN-02): the lists of a school's session can be copied in one step from the same school's earlier session, or from another school's session, class by class (only classes the target school has); the copy replaces nothing that already exists unless asked, and the student estimates are not copied.
- **R5** A list is changed freely (add, remove, change quantities and the estimate); orders made from it earlier keep their lines (sales R22 copies them).
- **R6** **Who does what**: the Owner and an entered Super Admin create, change, copy and delete lists; Staff read them and use them in orders. Anonymous `401`.
- **R7** **Reading**: the lists of a school's session, every class with its line count, estimate and books (code, name, quantity per student), and the difference to the school's previous session per class (books added, removed, quantity changed, FR-PLN-05).
- **R8** Every create and change is recorded who and when (the standard audit columns and the audit trail of `core`).
- **R9** Inputs are checked at the API boundary (lengths, ranges, at most 200 lines per list) and again in the domain; another company's list is `404`.

## 4. Data

Schema `planning`:
- `class_book_list` (aggregate root, company-owned): `school_id`, `session`, `class_name`, `student_estimate` (null or 0-5000), audit and concurrency columns. Unique `(company_id, school_id, session, class_name)` among live rows.
- `class_book_list_line` (child): `item_id`, item code and name (for reading), `quantity_per_student`.

## 5. API contract (`/api/v1`)

| Method + route | Who | Notes |
|---|---|---|
| `GET schools/{schoolId}/book-lists?session=` | `OWNER`, `STAFF`, `SUPER_ADMIN` (entered) | every class of the school with its list for the session (empty when none) and the difference to the previous session (R7) |
| `PUT schools/{schoolId}/book-lists/{session}/{className}` | `OWNER`, `SUPER_ADMIN` (entered) | `{studentEstimate?, lines: [{itemId, quantityPerStudent}]}` creates or replaces one class list (R2, R3, R5) |
| `DELETE schools/{schoolId}/book-lists/{session}/{className}` | `OWNER`, `SUPER_ADMIN` (entered) | removes the list (soft) |
| `POST schools/{schoolId}/book-lists/{session}/copy` | `OWNER`, `SUPER_ADMIN` (entered) | `{fromSchoolId?, fromSession, replaceExisting}` (R4); answers how many classes were copied and skipped |

## 6. User interface

In the UI plan; summary: a school's profile gets a **Book lists** tab (platform U14): the session (current, previous, next), every class as a row with its book count and student estimate; a class opens its list to add books (search, scan or bulk tick, sales R24 picker), set quantities per student and the estimate; "Copy from last session" and "Copy from another school"; the difference to last session is marked.

## 7. Events

None in this feature.

## 8. Dependencies

`planning` depends on `school` (directory R16, session R18), `item` (directory R16) and the platform. `sales` reads lists through this module's capability (R22 of sales).

## 9. Acceptance criteria

- **AC1** The Owner saves Class 5 of school S, session 2026-27, with books X (2 per student) and Y (1) and estimate 40; reading the session lists shows Class 5 with 2 books and 40, the other classes empty; Staff read it; Staff saving is `403` (R2, R3, R6, R7).
- **AC2** A class the school does not have, quantity 0 or 51, an unknown item, 201 lines are `400`; adding X twice keeps one line (R2, R9).
- **AC3** Copying 2026-27 to 2027-28 of S copies every class list without estimates; copying again without replace skips the existing ones; copying from school T copies only the classes S has (R4).
- **AC4** After changing Class 5 in 2027-28 (Z added, Y removed, X 2 -> 3) the read of 2027-28 shows Z added, Y removed and X changed against 2026-27 (R7).
- **AC5** The Owner of B gets `404` for A's school lists (R1, R9).

## 10. Decisions

| # | Decision |
|---|---|
| D1 | New module `planning` with its own schema; a class book list is a company-owned aggregate per school, session and class. Items are kept by id with their code and name for reading. (proposed by the assistant; owner's answer: the class set is the school's class-wise list of a session) |

## 11. Open questions

None. The owner answered the wholesale order questions on 2026-11-15.
