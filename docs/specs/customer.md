# Spec: customer - Customer master

- **Module:** `customer` (PostgreSQL schema `customer`)
- **Status:** R1-R14 Approved by abhishek in chat on 2026-11-14 (Q1-Q11 answered one by one, all with the recommended answers; D1-D4 with them); parked by the owner on 2026-11-14: built last, after the important transaction modules
- **Last updated:** 2026-11-14
- **Source docs:** [BRD §1.3 (module 19), §3, §4.3A Customer Master (FR-CUS-01 to FR-CUS-05), §4.11 Direct Sale (FR-CNT-14 to FR-CNT-21), §4.14 PAY-C, §4.15 FR-LED-10/11, §5.6 BR-LED-03, §7.1, §8 Data Model (Customer), §9 Q-13 to Q-16](../bussiness/BRD-Book-Inventory-Phase1.md); [specs/platform.md](platform.md) P18-P23; [specs/company.md](company.md); [specs/school.md](school.md) R16; [specs/vendor.md](vendor.md) (the neighbouring master it follows)
- **Plans:** [customer#01](../plans/customer/2026-11-14-01-customer-master.md) (Approved), [customer#02](../plans/customer/2026-11-14-02-ui-customer-screens.md) (Approved); the capability it needs from school: [school#03](../plans/school/2026-11-14-03-school-directory.md) (Approved)

> One spec per module. It says WHAT the module does and WHY, never implementation steps (those live in plans).

## 1. Purpose and scope

`customer` holds the shop's direct customers who buy on credit (udhaar): who they are, how to reach them, their starting balance and an optional credit limit. The shop's direct sale (Counter Sale, "Dukaan (Direct)"), customer payments (PAY-C) and the customer ledger of later modules point at a customer by id and never copy these values except what a document must keep.

Feature **Customer master** (R1-R14): the Owner (and, as far as Q4 allows, Staff) creates customers, the Owner edits, activates and deactivates them; while billing, a customer is suggested by name or mobile (FR-CUS-03). A customer belongs to exactly one company and no other company ever sees it ([specs/platform.md](platform.md) P18-P21).

Out of scope for this feature: the customer ledger, the outstanding report and posting the opening balance (FR-CUS-05, FR-LED-10/11: the Ledger module); customer payments (PAY-C: the Payment module); the direct sale bill itself and the credit-limit warning shown on it (FR-CUS-04 is checked by the Counter Sale module against the ledger balance, R9); Excel import of customers (Q10); the Super Admin's screens in the `admin` app (Q9).

## 2. Concepts and glossary

- **Customer**: one person (usually a parent) who buys from the shop on credit (FR-CUS-02). A walk-in customer who pays at once has no customer record; the bill keeps an optional name and mobile (FR-CNT-15, FR-CNT-18).
- **Code**: the customer's generated number `CUS-0001`, ... (R10, Q1).
- **Opening balance**: what the customer owed the shop when the system was started (R6).
- **Credit limit**: an optional amount the customer's balance should not cross; crossing it only warns (R7, FR-CUS-04, Q-15).

## 3. Business rules

**Customer master**
- **R1** A customer belongs to exactly one company, the company of the session that creates it. Another company's customers are never read or changed (platform P18-P21); asking for one answers `404`, identical to an unknown id.
- **R2** A customer has: **code** (generated, never typed or changed, R10), **name** (required, 2-150 characters), **mobile** (required, Q2), address, and optionally a **school** and **class** (FR-CUS-01). The optional values are trimmed and empty becomes none. Only credit customers are entered; nothing in this module refuses a customer for not having bought on credit yet (FR-CUS-02).
- **R3** The mobile number is stored as digits only (10-15 digits); an Indian number with a leading `91` on 12 digits or `0` on 11 digits is stored without it, the same way `iam`, `school` and `vendor` store mobile numbers. Two customers of a company may not have the same mobile number (`409`), so the suggestion by mobile finds one customer (Q3).
- **R4** The **school**, when given, is an active school of the same company, read through the school directory ([specs/school.md](school.md) R16); an unknown, another company's or an inactive school is `400`. The **class** may be given only with a school and must be one of that school's classes (ignoring case and extra spaces), stored as the school writes it; otherwise `400` (Q5). A school that is deactivated later stays on the customer.
- **R5** A customer is `ACTIVE` or `INACTIVE`. A new customer is active. An inactive customer keeps all its data and history, is not suggested (R8) and cannot be chosen for a new bill (later modules); it can be activated again. A customer is never deleted (Q8).
- **R6** The **opening balance** is an amount of at least zero with two decimals: what the customer owed the shop when the system was started (FR-CUS-01). Positive means the customer owes the shop (receivable, like a school's, Q6). It is stored here; posting it into the customer ledger belongs to the Ledger module.
- **R7** The **credit limit** is optional; when given it is an amount greater than zero with two decimals (FR-CUS-04, Q-15). This module only stores it; the warning when a bill would take the customer's balance above it is shown by the Counter Sale module from the customer ledger, and it never blocks the bill (Q7).
- **R8** **Suggest while billing** (FR-CUS-03): a short lookup by a text of at least 2 characters returns at most 20 **active** customers whose name or mobile contains the text (ignoring case and spaces), with code, name, mobile and school name; a customer not found can be created from the same screen with the rules of R2-R7 (Q11).
- **R9** **Who does what** (Q4): the Owner and an entered Super Admin create, edit, activate and deactivate customers. Staff read the list and a customer, use the suggestion and create a customer (they sell at the counter and may need to add one there), but cannot edit, activate or deactivate (`403`). Anonymous callers get `401`. A Super Admin with no company entered reads nothing and a write is refused (`403 core.company_required`, platform P20).
- **R10** The **code** is generated when the customer is created: `CUS-` and a number of at least four digits from the company's code series ([specs/platform.md](platform.md) P23). A number is never given twice, also not after the customer is deactivated; two customers created at the same moment get different numbers. A `code` sent in a request is ignored (Q1).
- **R11** The list is paged (page size capped at 200), searchable by name, code and mobile, filterable by status and school, and sortable by name, code and created date (BRD §7.1).
- **R12** Every create and change is recorded who and when through the standard audit columns and the audit trail of `core` ([specs/core.md](core.md) A1-A9). This module adds no audit mechanism of its own.
- **R13** Changing a customer's name, mobile or school does not touch bills already issued: later modules copy what they must keep (BRD §8 design decision 3).
- **R14** The customer's money rules: amounts use `numeric(18,2)`, never float; no rounding happens in this module (it only stores the opening balance and the credit limit).

## 4. Data

Schema `customer`:
- `customer` (aggregate root, company-owned, platform P18): `company_id`, `code` (R10), `name`, `name_normalized`, `mobile`, `address`, `school_id` (plain id, no foreign key across schemas), `class_name`, `opening_balance_amount` (`numeric(18,2)`, not negative), `credit_limit_amount` (`numeric(18,2)`, null or greater than zero), `status`, plus the standard audit and concurrency columns. Unique index on `(company_id, code)` and on `(company_id, mobile)`; index on `(company_id, name_normalized)` and `(company_id, school_id)`.

## 5. API contract (`/api/v1`)

All routes are company-scoped by the session (platform P19, P20); the company is never a request field.

| Method + route | Who | Notes |
|---|---|---|
| `POST customers` | `OWNER`, `STAFF`, `SUPER_ADMIN` (entered) | `{name, mobile, address?, schoolId?, className?, openingBalanceAmount?, creditLimitAmount?}` -> `201` with the generated `code`; `400` invalid values (R2-R7); `409` mobile taken (R3) |
| `GET customers` | `OWNER`, `STAFF`, `SUPER_ADMIN` (entered) | paged list (R11) with the school's name |
| `GET customers/suggest?text=` | `OWNER`, `STAFF`, `SUPER_ADMIN` (entered) | at most 20 active customers (R8); a text shorter than 2 characters answers an empty list |
| `GET customers/{id}` | `OWNER`, `STAFF`, `SUPER_ADMIN` (entered) | the customer; `404` otherwise |
| `PUT customers/{id}` | `OWNER`, `SUPER_ADMIN` (entered) | edits R2-R7 (not the code, not the status) |
| `POST customers/{id}/deactivate`, `/activate` | `OWNER`, `SUPER_ADMIN` (entered) | R5; `409` when already in that status |

Errors are `ProblemDetails` with a message code from the module's catalogue. Validation failures are `400`.

## 6. User interface

Written in the UI plan; summary: in the company workspace (`member` app) the Owner and Staff get a "Customers" menu in its own group (its ledger joins it later): a list with search and filters; a customer opens beside the list with an X to close, and adding and editing open in the same panel (platform U10); activate and deactivate for the Owner; required fields marked with a red `*`. The suggestion of R8 is used by the later bill screen. Each state shows loading, empty, error with retry and success.

## 7. Events

Raised by the `Customer` aggregate (past tense, ids and values only): `CustomerCreated`, `CustomerDeactivated`, `CustomerReactivated`. Reactions: none in this feature.

## 8. Dependencies

`customer` depends on `iam` for the current company, on `core` for the audit trail, on the platform (P18-P23) for company-owned data and the code series, and on `school` through its directory capability ([specs/school.md](school.md) R16, plan school#03). Counter Sale, Payment and Ledger will depend on `customer` by its id.

## 9. Acceptance criteria

- **AC1** The Owner of company A creates a customer with name and mobile: `201`, code `CUS-0001`, `ACTIVE`, opening balance 0, no credit limit; the row's `company_id` is A; a second one gets `CUS-0002`, the first in B `CUS-0001`; a `code` in the body is ignored (R1, R2, R5, R10).
- **AC2** A 1-character name, a missing mobile, a mobile of 8 digits, a negative opening balance, a credit limit of 0 or -5 and an amount with three decimals are `400`; a second customer with mobile `+91 98765 00001` after `9876500001` is `409` (R2, R3, R6, R7).
- **AC3** A school of A with its class `5th` (typed ` 5TH `) is stored as that school and `5th`; a class without a school, a class not in the list, B's school and an inactive school are `400` (R4).
- **AC4** The Owner of B listing or opening A's customer gets nothing or `404`, identical to an unknown id; a `companyId` in the query or body changes nothing (R1).
- **AC5** The suggestion with `ram` finds the active customers named Ram..., with `98765` the ones whose mobile contains it, never an inactive one, at most 20; a text of 1 character returns an empty list (R5, R8).
- **AC6** Staff list, open, suggest and create a customer (`201`), but editing, activating or deactivating answers `403`; anonymous `401` (R9).
- **AC7** A Super Admin with no company entered lists nothing and `POST customers` answers `403 core.company_required`; after entering A they list and create (R9).
- **AC8** Deactivating keeps the customer under the inactive filter and out of the suggestion; activating restores it; doing either twice is `409` (R5).
- **AC9** The list is paged and capped at 200; search by name, code and mobile, the status and school filters and the sorts work; an unknown sort column is ignored (R11).
- **AC10** Every create and change of a customer appears in the audit trail with the actor and company A (R12).

## 10. Decisions

| # | Decision |
|---|---|
| D1 | New module `customer` with its own schema. A customer is a company-owned aggregate root, like a school or a vendor. (proposed by the assistant; approved with the rules on 2026-11-14) |
| D2 | Customers are deactivated, never deleted (R5). (proposed by the assistant, Q8) |
| D3 | The credit limit is stored here; the check needs the customer's balance, which only the Ledger has, so the warning lives in the Counter Sale module (R7). (proposed by the assistant, Q7) |
| D4 | The school and class are checked through a new directory capability of the school module (school R16), the same way the item module reads vendors (vendor R15), never by joining its tables. (proposed by the assistant) |

## 11. Open questions

None. Q1-Q11 were answered by the owner one by one on 2026-11-14, all with the recommended answers; the rules above follow them. The list keeps the questions with the answer taken.

- **Q1** Customer code. Recommended: generated `CUS-0001` per company, like `SCH-` and `VEN-` (R10). Alternative: no code, only name and mobile.
- **Q2** Mobile. Recommended: required, because a credit customer must be reachable and the bill suggests by mobile (R2). Alternative: optional.
- **Q3** The same mobile twice. Recommended: refused inside a company, one customer per mobile (a parent with two children is one customer) (R3). Alternative: allowed, the suggestion shows both.
- **Q4** Staff and customers. Recommended: Staff read, suggest and create (they add a customer at the counter), only the Owner edits and deactivates (R9). Alternative: Staff read and suggest only.
- **Q5** School and class. Recommended: both optional; a class only with a school and from that school's class list (R4). Alternative: class as free text.
- **Q6** Opening balance sign. Recommended: at least zero, positive means the customer owes the shop (R6).
- **Q7** Credit limit. Recommended: optional, greater than zero when given, stored here; the warning is shown on the bill by the Counter Sale module and never blocks (R7, D3).
- **Q8** Delete. Recommended: never, only deactivate (R5).
- **Q9** The Super Admin's customer screens in the `admin` app. Recommended: later, like the other masters.
- **Q10** Excel import of customers. Recommended: later, with the other master imports (ClosedXML is already approved, stock D5).
- **Q11** The suggestion. Recommended: at least 2 characters, at most 20 active customers, matching name or mobile (R8).
