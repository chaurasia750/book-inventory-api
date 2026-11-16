# Plan sales#24: UI school return (credit note) screens

- **File:** `docs/plans/sales/2026-11-16-24-ui-school-return.md`
- **Kind:** UI
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R53-R61 (the screen side); AC44-AC51
- **Owner:** abhishek
- **Status:** In progress
- **Base branch:** `main` of `book-inventory-ui` (one branch `feature/sales-school-return` for sales#23-#26 in both repositories, the owner's instruction)
- **Depends on:** sales#23
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-16 (chat: "all approved hai, ek branch pe sab kro UI and api")
- **Created:** 2026-11-16

## 1. In one minute
After this plan the Owner and Staff open **Orders > Returns**, see the list of credit notes, add one (school, optional invoice, the books returned as good or damaged, the reason) and read it beside the list; the Owner cancels one. An invoice's detail lists the credit notes made against it.

## 2. Touches / leaves out
Repository `book-inventory-ui` (build: `npx nx build member`, lint: `npx eslint` on the touched folders): a `returns` page folder under `apps/member/src/app/pages/`, `libs/api-services` (a credit note service), `libs/shared-models` (the types), the menu, the invoice detail (a "Returns" block), `assets/i18n/en.json`.
Leaves out: the Credit Note PDF buttons, the refund screen, the counter return.

## 3. Approach
The list-and-detail panel as the other records (X to close, id in the URL). The school is chosen with the shared searchable school select; choosing an invoice (optional) fills the lines from `invoices/{id}/returnable`, each with a quantity and a Good / Damaged choice; without an invoice the items are searched like the order editor, the rate is shown and changeable. The shared date picker for the date; dropdowns, not free text, for the reason. The over-return warning is yellow and never blocks.

## 4. Tasks (at most 8)
- [ ] 1. Shared models and the credit note API service.
- [ ] 2. The Returns list with search and filters (school, status, reason, session), the Orders menu entry.
- [ ] 3. The credit note panel (read): lines, condition, totals, reason, the invoice link.
- [ ] 4. The add form: school, optional invoice, lines from the invoice or item search, condition, reason and remark, with the totals preview and the warning.
- [ ] 5. The cancel action for the Owner with the reason; Staff do not see it.
- [ ] 6. The invoice detail's "Returns" block (credit notes and the returned quantity per line).
- [ ] 7. Loading, empty and error states; messages through the catalogue; English strings.
- [ ] 8. Build and lint clean; every row of section 5 checked in headless Chrome and a screenshot looked at.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R53`, `R54`, `R56`, `AC44`, `AC45`, `AC46` | 4 | add a return of 4 books from an invoice as Good, then as Damaged: the panel shows both; returning more than sold shows the yellow warning and still saves |
| `R55`, `AC48` | 4 | the preview totals match the figures of the API |
| `R58`, `R60`, `AC49` | 5 | the Owner cancels and sees `CANCELLED`; Staff see no Cancel |
| `R61`, `AC51` | 2, 3, 6 | the list filters narrow the list; the invoice detail lists its credit note and the returned quantity |

## 6. Not verified
(filled when the plan is closed)

## 7. Risks and notes
The screen follows the order editor's parts; screenshots are looked at before the plan closes (UI consistency).
