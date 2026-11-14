# Plan price#04: UI bulk price change

- **File:** `docs/plans/price/2026-11-14-04-ui-bulk-price-change.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/price.md](../../specs/price.md) - rules covered: R10-R14 (the screen side); AC9, AC10
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `price#03`, `price#02`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-14 (chat: the open questions answered one by one)
- **Created:** 2026-11-14

## 1. In one minute
After this plan the Owner has a "Bulk change" screen under Prices: choose the items (vendor, category, class or all), the version to change and, for MRP, sale price and purchase price, an increase or decrease by percent or rupees; press Preview to see every old and new price, then Apply. Open `/<company>/prices/bulk`.

## 2. Touches / leaves out
Touches: `libs/shared-models`, `libs/api-services` (`PriceService` bulk calls), `apps/member` (the bulk change page and its preview table under `pages/prices`, the `prices/bulk` route for the Owner, a "Bulk change" button on the price list), the three `en.json` catalogues.
Leaves out: Excel upload, the Super Admin's screens in `admin`.

## 3. Approach
One page with three steps in one view: selection, changes, preview. Preview is a button, not automatic; the preview table is paged and shows skipped items with their reason and the totals. Apply asks for a confirmation that names the number of items and is disabled until a preview exists for the current inputs (any change to the inputs clears the preview). The numbers shown are the server's; the page never computes prices itself. The vendor select lists the company's active vendors; category and class use the item screens' lists. Required fields carry the `*` marker.

## 4. Tasks (at most 8)
- [ ] 1. `PriceService` bulk preview and apply calls and models.
- [ ] 2. The bulk change page: selection, version name, three change rows (kind, direction, value) (R10, R11).
- [ ] 3. The preview table with totals and skipped reasons; states loading, empty, error, success (R12).
- [ ] 4. Apply with confirmation and the result counts; the preview clears when inputs change (R13).
- [ ] 5. Route (Owner only), the button on the price list, i18n keys.
- [ ] 6. `npx nx run-many -t build` and `-t lint`: 0 new findings against the base; verify every row of section 5 against the running API and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R10`, `R11`, `R12`, `AC9` | 2, 3 | Select a category, MRP +10 percent, purchase -5 rupees, Preview: the table lists old and new prices and skipped items; reload the price list: nothing changed |
| `R13`, `AC10` | 4 | Apply after a preview: the confirmation shows the item count; afterwards the prices on the price list match the preview; changing an input clears the preview and disables Apply |
| `R5`, `R6` | 5 | Staff do not see the Bulk change button and a typed address answers the unauthorized page |

## 6. Not verified
To be filled when the plan is closed (no browser tool for the assistant: the owner looks at the screens).

## 7. Risks and notes
Written without a browser; expect small visual fixes after the owner's first look.
