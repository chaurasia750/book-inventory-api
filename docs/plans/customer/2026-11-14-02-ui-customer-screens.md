# Plan customer#02: UI customer screens

- **File:** `docs/plans/customer/2026-11-14-02-ui-customer-screens.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/customer.md](../../specs/customer.md) - rules covered: R2-R9, R11 (the screen side); AC1-AC3, AC5, AC6, AC8, AC9
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `customer#01`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-14 (chat: Q1-Q11 of the customer spec answered one by one, all with the recommended answers)
- **Created:** 2026-11-14

## 1. In one minute
After this plan the Owner and Staff have a "Customers" menu in the company app, in its own group (its ledger joins it later): the customer list with search and filters; clicking a customer opens it beside the list with an X to close; adding and editing open in the same panel; the Owner activates and deactivates. A small reusable customer picker (type a name or mobile, choose or "add new") is ready for the later bill screen. Open `/<company>/customers`.

## 2. Touches / leaves out
Touches: `libs/shared-models` (customer model), `libs/api-services` (`CustomerService`), `apps/member` (`pages/customers`: list, detail panel, form; the `customers` route with children `new`, `:id`, `:id/edit`; the "Customers" menu item for Owner and Staff), a customer picker component used by the later bill, the three `en.json` catalogues (`customer.*`, `errors.customer.*`).
Leaves out: the bill screen, the customer ledger and payments, the Super Admin's screens.

## 3. Approach
It follows the vendor and school screens (platform U10 list and detail, the form in the panel, required fields with a red `*`). The school field is a select of the company's active schools; the class select appears after a school is chosen and lists that school's classes. The opening balance and credit limit are amount inputs with two decimals. Staff see "Add customer" but no Edit, Activate or Deactivate. The picker calls `customers/suggest` after 300 ms of typing (at least 2 characters). Each state shows loading, empty, error with retry and success; all texts come from the catalogue.

## 4. Tasks (at most 8)
- [ ] 1. Models and `CustomerService` for the endpoints.
- [ ] 2. Customer list: search, status and school filters, sort, paging, compact rows when the panel is open, states (R11).
- [ ] 3. Detail panel: values, status, Owner's activate and deactivate (R5, R9).
- [ ] 4. Form in the panel (add and edit): name, mobile, address, school and class, opening balance, credit limit; red `*` on required fields; API messages (R2-R7).
- [ ] 5. Customer picker (suggest, choose, "add new" opening the form) for the later bill (R8).
- [ ] 6. Routes, role guards (Owner edits, Staff create and read), the "Customers" menu item, i18n keys.
- [ ] 7. `npx nx run-many -t build` and `-t lint`: 0 new findings against the base; verify every row of section 5 against the running API and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R2`, `R10`, `AC1` | 4 | Add a customer with name and mobile: the panel shows `CUS-....` and Active |
| `R2`, `R3`, `R6`, `R7`, `AC2` | 4 | A short name, a missing mobile, a negative opening, a limit of 0 show the field or API message; the same mobile again shows the API message |
| `R4`, `AC3` | 4 | Choosing a school lists its classes; the saved customer shows school and class |
| `R5`, `R8`, `AC5`, `AC8` | 3, 5 | The picker finds by name and by mobile, never an inactive customer; deactivate and activate from the panel |
| `R9`, `AC6` | 3, 6 | Staff see the list, the panel and Add, but no Edit, Activate or Deactivate |
| `R11`, `AC9` | 2 | Search, filters, sort and paging change the list |

## 6. Not verified
To be filled when the plan is closed (no browser tool for the assistant: the owner looks at the screens).

## 7. Risks and notes
Written without a browser; expect small visual fixes after the owner's first look. The picker has no user until the Counter Sale bill screen; it is shown on its own in the list's "Add" flow only if the owner wants it there.
