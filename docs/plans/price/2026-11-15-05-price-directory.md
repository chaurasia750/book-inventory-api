# Plan price#05: Price directory for other modules

- **File:** `docs/plans/price/2026-11-15-05-price-directory.md`
- **Kind:** Backend
- **Spec:** [../../specs/price.md](../../specs/price.md) - rules covered: R18; AC13
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `price#01` (Done)
- **UI plan:** none, a read capability for other modules
- **Approved by / on:** abhishek / 2026-11-15 (chat: sales Q1-Q14 answered one by one)
- **Created:** 2026-11-15

## 1. In one minute
After this plan the sales module can read an item's price versions and its default version on a date, and one version by id, without the purchase price and without touching the price tables.

## 2. Touches / leaves out
Touches: `BookInventory.Application/Price` (capability, records), its implementation over the price store reusing `DefaultPrice` (R4).
Leaves out: endpoints, UI. Schema change: no.

## 3. Approach
The default rule of R4 already lives in `DefaultPrice`; the directory calls it with the given date, so it is not written twice.

## 4. Tasks (at most 8)
- [ ] 1. Capability and records (R18).
- [ ] 2. Implementation reusing `DefaultPrice` (R4).
- [ ] 3. Build; verify section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R18`, `AC13` | 1, 2 | Throw-away call: versions of a book with the default today; a version effective tomorrow is not today's default; a version by id; B's item -> nothing; no purchase price in the records |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
None.
