# Plan price#06: Purchase-price directory for the purchase module

- **File:** `docs/plans/price/2026-11-16-06-purchase-price-directory.md`
- **Kind:** Backend
- **Spec:** [../../specs/price.md](../../specs/price.md) - rules covered: R19; AC14
- **Owner:** abhishek
- **Status:** Approved
- **Base branch:** `main` (one branch `feature/purchase-vendor-return` for price#06, sales#27, sales#28 and purchase#01-#04 in both repositories, the owner's instruction "sare plan ek hi branch pe implement krna")
- **Depends on:** `price#01` (Done), `price#05` (Done, the sales directory it sits beside)
- **UI plan:** none, a read capability for other modules
- **Approved by / on:** abhishek / 2026-11-16 (chat: Q1-Q7, Q41, Q42 answered one by one; "sare plan ek hi branch pe implement krna")
- **Created:** 2026-11-16

## 1. In one minute
After this plan other modules can ask the price module for the **purchase price** of a list of items in their default version on a date, through one capability (no endpoint). The purchase module uses it to start the rate of a vendor-return line.

## 2. Touches / leaves out
Touches: `Application/Price` (the capability and its answer record), `Infrastructure` (its data side), `docs/Code_Map.md`. Leaves out: any endpoint, the sale price and the MRP (the capability of price R18 stays the one for those), the purchase-price screens (price#02).

## 3. Approach
A second small interface next to the sales directory, answered from the same default-version rule (R4): for each item the version effective on the date, its id, name and purchase price; items with no version are left out; at most 500 ids; company-limited by the platform filter.

## 4. Tasks (at most 8)
- [ ] 1. The capability, its answer record and its message for more than 500 ids (R19).
- [ ] 2. Its data side, reusing the default-version rule of R4 (never a second copy of it).
- [ ] 3. Build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [ ] 4. Verify AC14 on the local database with a scratch check.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R19`, `AC14` | 1-4 | an item whose default version on 2026-11-16 has purchase price 180 returns 180 and the version name; an item with no version is left out; the answer has no sale price or MRP; 501 ids refused; another company's item is left out |

## 6. Not verified
(filled when the plan is closed)

## 7. Risks and notes
No migration. The check calls the capability from a scratch console app (no endpoint exists).
