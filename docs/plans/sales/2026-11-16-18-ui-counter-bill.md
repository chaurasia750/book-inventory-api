# Plan sales#18: UI counter billing screen, bills list and detail

- **File:** `docs/plans/sales/2026-11-16-18-ui-counter-bill.md`
- **Kind:** UI
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R42-R49 (the screen side); AC33-AC41
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** sales#17
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-16 (chat: "all plan approve hai, ek hi branch pe")
- **Created:** 2026-11-16

## 1. In one minute
After this plan a counter opens its billing screen: type the student, scan or search items, tap a category button, add the whole class book list, set discounts, enter Cash and UPI and save; the bill shows with its number and the stock warning, and the bills list, a bill's detail with the Owner's Cancel and a student's bills are there. Open `/<company>/counters/<id>/bill`.

## 2. Touches / leaves out
Touches: `libs/shared-models`, `libs/api-services` (`CounterBillService`, student suggestion), `apps/member` (`pages/counters`: billing page, bills list and detail panel, a student box component; the routes), the order editor's item-entry parts reused where they are shared, `assets/i18n/en.json`.
Leaves out: the receipt buttons (sales#22), the day summary (sales#20).

## 3. Approach
The billing page is a full-width page like the order editor (platform U14): student name with suggestions (name, class, section; a new name asks only for class and the optional values), item box with barcode and search, **category buttons**, "Add the class book list" for the student's class, lines with quantity and MRP, the item discount only when the company allows it, the total discount, the GST toggle starting from the school's default, live totals, Cash and UPI amounts with the difference shown and Save enabled only when they equal the total. Closed counters show a message instead of the form. The bills list opens a bill in the side panel (platform U10) with Cancel for the Owner.

## 4. Tasks (at most 8)
- [ ] 1. Models and `CounterBillService`; the student suggestion call.
- [ ] 2. Student box with suggestions and the new-student fields (R40, R41).
- [ ] 3. Item entry: barcode, search and category buttons; the class book list button (R43).
- [ ] 4. Lines, discounts, GST toggle and live totals (R43, R44).
- [ ] 5. Payments with the difference and Save (R45), the stock warning after saving (R46).
- [ ] 6. Bills list with filters, bill detail with the Owner's Cancel, a student's bills (R47, R49).
- [ ] 7. Routes, guards, i18n; build and lint; verify every row of section 5 and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R40`, `R41`, `AC33`, `AC34` | 2 | typing `rav` suggests students; a new name asks for the class; the same name and class the second time reuses the student |
| `R43`, `AC38` | 3 | scan or search adds an item; a category button lists items to add; "Add the class book list" adds one line per book; a barcode that matches nothing says so |
| `R44`, `R45`, `AC35`, `AC37` | 4, 5 | the totals equal the API's worked bill; Save stays disabled until Cash + UPI equals the total; a refused bill shows the API message and keeps the screen |
| `R47`, `R48`, `AC39`, `AC40` | 6 | the Owner cancels with a reason; Staff see no Cancel; a closed counter shows the message |
| `R49`, `AC41` | 6 | filters and search change the list; a student's bills list together |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
The biggest screen of the feature, built from the order editor's parts. Written without a browser: expect small visual fixes after the owner's first look.
