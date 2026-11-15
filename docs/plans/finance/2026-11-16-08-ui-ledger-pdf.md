# Plan finance#08: UI download the ledger as PDF

- **File:** `docs/plans/finance/2026-11-16-08-ui-ledger-pdf.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/finance.md](../../specs/finance.md) - rules covered: R18 (the screen side); AC10
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `finance#07`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-16 (chat: "next pending plan kro")
- **Created:** 2026-11-16

## 1. In one minute
After this plan the Ledger tab of a school and of a vendor has a Download PDF button that downloads the statement of the filter on screen.

## 2. Touches / leaves out
Touches: `apps/member` (`pages/ledger`: the statement component), the ledger service, i18n.
Leaves out: the links (finance#09).

## 3. Approach
The button sits beside the filter and uses the filter on screen (all, a financial year, a session, a date range); it downloads the file named after the party; while it works the button shows a spinner; an error (for example more than 5000 lines) shows its catalogue text.

## 4. Tasks (at most 8)
- [ ] 1. Service call for the PDF with the filter.
- [ ] 2. The Download PDF button with its loading and error states.
- [ ] 3. i18n; build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R18`, `AC10` | 1-2 | Owner opens a school Ledger tab, picks a financial year and downloads: the file has the same rows as the screen; all entries and a date range too |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Seen through headless Chrome only.
