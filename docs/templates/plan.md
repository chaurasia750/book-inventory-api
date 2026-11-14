# Plan <module>#NN: <short imperative title>

- **File:** `docs/plans/<module>/YYYY-MM-DD-NN-<slug>.md` (NN = serial inside this folder, never reused or renumbered)
- **Kind:** Backend | UI
- **Spec:** [../../specs/<module>.md](../../specs/<module>.md) - rules covered: R.., AC..
- **Owner:** <developer>
- **Status:** Draft | Approved | In progress | Done | Superseded by <module>#NN
- **Depends on:** `<module>#NN` or none
- **UI plan:** `#NN` | none, <reason>   (Backend plans only)
- **Approved by / on:** <name> / YYYY-MM-DD
- **Created:** YYYY-MM-DD   **Completed:** YYYY-MM-DD

## 1. In one minute
What will exist after this plan, why, and how to see it working (the request to send or the screen to open). Plain words.

## 2. Touches / leaves out
Files or areas this plan changes. What it deliberately does not do, and which plan will.

## 3. Approach
Design decisions and why; anything that deviates from the usual pattern. Schema change: yes/no (one open schema-changing plan per module at a time).

## 4. Tasks (at most 8)
- [ ] 1. ...
- [ ] 2. ...
- [ ] N. Verify every row of section 5 and record the result

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R1` | 2 | ... |
| `AC1` | 3 | ... |

## 6. Not verified
Everything that could not be checked, with the reason. Copied to `docs/Backlog.md` when the plan is closed.

## 7. Risks and notes
Assumptions, rollback note for destructive migrations, follow-ups that became new plans.
