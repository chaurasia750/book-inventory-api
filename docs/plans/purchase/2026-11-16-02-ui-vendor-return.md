# Plan purchase#02: UI vendor returns: list, form and panel

- **File:** `docs/plans/purchase/2026-11-16-02-ui-vendor-return.md`
- **Kind:** UI
- **Spec:** [../../specs/purchase.md](../../specs/purchase.md) - rules covered: R2-R9, R12 (the screen side); AC1-AC8, AC10
- **Owner:** abhishek
- **Status:** Approved
- **Base branch:** `main` of `book-inventory-ui` (one branch `feature/purchase-vendor-return` for price#06, sales#27, sales#28 and purchase#01-#04 in both repositories, the owner's instruction "sare plan ek hi branch pe implement krna")
- **Depends on:** purchase#01
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-16 (chat: Q1-Q7, Q41, Q42 answered one by one; "sare plan ek hi branch pe implement krna")
- **Created:** 2026-11-16

## 1. In one minute
After this plan the Owner and Staff open **Purchases > Vendor returns**, see the list, record a return (vendor, date, items with stock source, quantity and rate, remark) and read it beside the list; the Owner cancels one. The vendor's page lists its vendor returns.

## 2. Touches / leaves out
Repository `book-inventory-ui` (build: `npx nx build member`, lint: `npx eslint` on the touched folders): a `purchase` page folder under `apps/member/src/app/pages/`, `libs/api-services` (a vendor return service), `libs/shared-models`, the Purchases menu, the vendor page block, `assets/i18n/en.json`. Leaves out: the Debit Note buttons (purchase#04).

## 3. Approach
The list-and-detail panel as the other records (X to close, id in the URL). The vendor is a dropdown of active vendors; items are found with the order editor's item entry; the rate starts empty with the purchase price shown by the server on saving (the API fills it), or typed; the stock source is a dropdown; totals are the server's, shown after saving (no amount is worked out on the screen).

## 4. Tasks (at most 8)
- [ ] 1. Shared models and the vendor return API service.
- [ ] 2. The list with search and filters (vendor, status, dates); the Purchases menu.
- [ ] 3. The panel (read): lines, stock source, total, remark.
- [ ] 4. The form: vendor, date, item entry, stock source, quantity, rate, remark; the low-stock notice after saving.
- [ ] 5. The Cancel action for the Owner with the reason; Staff do not see it.
- [ ] 6. The vendor page block of its vendor returns.
- [ ] 7. Loading, empty and error states; messages through the catalogue; English strings.
- [ ] 8. Build and lint clean; every row of section 5 checked in headless Chrome and a screenshot looked at.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R2`-`R7`, `AC1`, `AC2`, `AC5` | 4 | a return of 5 from Main and 2 from Damaged saves and opens in the panel with the server total; more than the stock shows the notice |
| `R8`, `R9`, `AC7` | 5 | the Owner cancels and sees `CANCELLED`; Staff see no Cancel |
| `R12`, `AC10` | 2, 3, 6 | the filters narrow the list; the vendor page lists its returns |

## 6. Not verified
(filled when the plan is closed)

## 7. Risks and notes
Screenshots are looked at before the plan closes (UI consistency).
