# Plan company#06: Company tax profile for other modules

- **File:** `docs/plans/company/2026-11-15-06-company-tax-profile.md`
- **Kind:** Backend
- **Spec:** [../../specs/company.md](../../specs/company.md) - rules covered: R16; AC17
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `company#01` (Done)
- **UI plan:** none, a read capability for other modules
- **Approved by / on:** abhishek / 2026-11-15 (chat: sales Q1-Q14 answered one by one)
- **Created:** 2026-11-15

## 1. In one minute
After this plan the sales module can ask the company module for the state and GSTIN of the session's company (to choose CGST+SGST or IGST) without touching its tables. Nothing visible changes.

## 2. Touches / leaves out
Touches: `BookInventory.Application/Company` (a small capability and its record), its implementation over the company store.
Leaves out: any endpoint or UI. Schema change: no.

## 3. Approach
Same shape as the other directories: one method returning a small record for the company of the session (null without a company).

## 4. Tasks (at most 8)
- [ ] 1. Capability and implementation (R16).
- [ ] 2. Build at 0 warnings, 0 errors; verify section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R16`, `AC17` | 1 | Throw-away call as the Owner of A: A's state and GSTIN; a Super Admin without a company: null |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
None.
