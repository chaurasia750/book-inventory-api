# Plan school#03: School directory capability (other modules read schools)

- **File:** `docs/plans/school/2026-11-14-03-school-directory.md`
- **Kind:** Backend
- **Spec:** [../../specs/school.md](../../specs/school.md) - rules covered: R16; AC12
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `school#01` (Done)
- **UI plan:** none, it adds no screen or endpoint: it is a read capability for other modules
- **Approved by / on:** abhishek / 2026-11-14 (chat: Q1-Q11 of the customer spec answered one by one, all with the recommended answers)
- **Created:** 2026-11-14

## 1. In one minute
After this plan another module (the customer master first) can ask the school module, without touching its tables: does this school exist in my company and is it active, what are the names of these schools, and is this class in the school's class list (and how does the school write it). Nothing visible changes; customer#01 is the first user. It is seen working through a throw-away check call for the Owner of a company.

## 2. Touches / leaves out
Touches: `BookInventory.Application/School` (a capability `ISchoolDirectory` with its small result records), its implementation over the existing school store (Application or Infrastructure, following `ItemDirectory` / `VendorDirectory`), `docs/Code_Map.md`.
Leaves out: any endpoint, any change to a school's data, any UI.
Schema change: no.

## 3. Approach
`ISchoolDirectory` follows the vendor and item directories: `GetAsync(id)` gives id, code, name and the active flag or null; `NamesAsync(ids)` gives id and name pairs; `ClassOfAsync(schoolId, text)` gives the school's own class name that equals the text ignoring case and extra spaces (the same normalisation as R6), or null. It answers only for the session's company through the platform filter (P19), returns small records, never the aggregate, and reads with no tracking.

## 4. Tasks (at most 8)
- [ ] 1. `ISchoolDirectory` and its records in `BookInventory.Application/School` (R16).
- [ ] 2. The implementation over the company-filtered school store, reusing the class-name normalisation of R6 (R16).
- [ ] 3. Build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [ ] 4. Verify every row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R16`, `AC12` | 1, 2 | A throw-away call (scratch code, not committed) for the Owner of A: A's school -> id, name, active; B's school id -> null; names of two ids; ` 5TH ` -> `5th`; `13th` -> null; an inactive school -> active false |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
A small read capability like vendor#03 and item#03; customer#01 is its first caller and repeats the check through its own requests.

**Parked (owner, 2026-11-14) with the customer master, then needed earlier:** the ledger statement and outstanding list (finance#03) read school names through it, so it is built before finance#03 (its rules are unchanged).
