# Backlog: things not verified or left open

Every item a plan could **not verify**, and every known gap or follow-up a review leaves open, is added here when that plan is
closed. The whole list is reviewed **after all planned work is done** (each item then becomes a check, a fix in a new plan, an
accepted limit, or is dropped). Status: `open` until reviewed.

Kinds: **NV** not verified · **GAP** known gap · **DEC** decision the owner has to take · **DEBT** cleanup.

| # | Module | Kind | Item | Source | Status |
| --- | --- | --- | --- | --- | --- |
| B1 | iam | DEC | Migration `Iam_CompanyAccounts` (two nullable columns, two indexes, wider role, login-result and logout-reason check constraints; additive) was applied only to a throw-away database. The SQL was shown; the owner has not confirmed it yet. The app applies it to `book_inventory` at its next start | iam#01 | open |
| B2 | iam | NV | The capabilities (Owner creation, session revocation, entered company) were driven by a temporary probe controller and a fake `ICompanyStatusReader`; the real callers and the real status reader arrive with company#01 and company#03 | iam#01 | open |
| B3 | iam | NV | Two parallel creations of the same Owner user id (unique index as the guard) were not exercised | iam#01 | open |
| B4 | iam | GAP | The user id of a company account is not yet visible in any UI; role routing, the inactive-company message and the company name in the header are iam#02 | iam#01 | open |
| B5 | iam | DEC | iam#01 was built on `docs/company-accounts` (the owner's instruction), not on `feature/iam-01`; the close step stopped before push and PR because the branch does not match the plan. Decide how the branch is pushed and reviewed | iam#01 | open |
| B6 | iam | DEBT | `AppUser.cs` is over 300 lines (about 347, partly before this plan); company behaviour went into the new partial `AppUser.Company.cs`. Split the aggregate further when it is next touched | iam#01 | open |
