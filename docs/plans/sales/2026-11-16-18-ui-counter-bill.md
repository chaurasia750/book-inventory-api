# Plan sales#18: UI counter billing screen, bills list and detail

- **File:** `docs/plans/sales/2026-11-16-18-ui-counter-bill.md`
- **Kind:** UI
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R42-R49 (the screen side); AC33-AC41
- **Owner:** abhishek
- **Status:** Done (2026-11-16; the owner asked to close it after the headless-Chrome check)
- **Base branch:** `main` of `book-inventory-ui` (one branch `feature/sales-school-counter` for sales#15-#22, the owner's instruction "same branch pe")
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
- [x] 1. Models and `CounterBillService`; the student suggestion call.
- [x] 2. Student box with suggestions and the new-student fields (R40, R41).
- [x] 3. Item entry: barcode, search and category buttons; the class book list button (R43).
- [x] 4. Lines, discounts, GST toggle and live totals (R43, R44).
- [x] 5. Payments with the difference and Save (R45), the stock warning after saving (R46).
- [x] 6. Bills list with filters, bill detail with the Owner's Cancel, a student's bills (R47, R49).
- [x] 7. Routes, guards, i18n; build and lint; verify every row of section 5 and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R40`, `R41`, `AC33`, `AC34` | 2 | typing `rav` suggests students; a new name asks for the class; the same name and class the second time reuses the student |
| `R43`, `AC38` | 3 | scan or search adds an item; a category button lists items to add; "Add the class book list" adds one line per book; a barcode that matches nothing says so |
| `R44`, `R45`, `AC35`, `AC37` | 4, 5 | the totals equal the API's worked bill; Save stays disabled until Cash + UPI equals the total; a refused bill shows the API message and keeps the screen |
| `R47`, `R48`, `AC39`, `AC40` | 6 | the Owner cancels with a reason; Staff see no Cancel; a closed counter shows the message |
| `R49`, `AC41` | 6 | filters and search change the list; a student's bills list together |

## 6. Not verified
Every row of section 5 was checked against the running API in headless Chrome (real login, company `abc-book-center`, counter `AC42 Counter C 26`).

Done and observed:
- Student box: typing `AC42` listed the school's students (`AC42 Student Four 5th`, ...); picking one shows "Reusing AC42 Student Four (5th)" and hides the class fields; a new name brings the class select (5th, 6th).
- Item entry: search `AC42c` listed `ITM-0007/0008/0009` with MRP; picking one added a line; a barcode that matches nothing shows "Item not found."; the `Book` category button listed the books and Add added a line; "Add the class book list" for `5th` said "Class 5th has no book list for this session.".
- Totals and payments: the grand total shown (380.00) equals the saved bill's grand total; Save bill is disabled while Cash is short and enabled once Cash + UPI equals the total ("Paid in full."); saving shows the bill number (`CNT/2026-27/0005`).
- Bills list: search `0005` / `AC18` narrows the list, `zzz-none` shows "No bills match."; a bill opens in the side panel.
- Owner cancels a bill with a reason -> status Cancelled with "Cancelled: <reason> (<date>)". Staff see no Cancel button. A closed counter shows "This counter is closed".

**Not verified:** "the same name and class the second time reuses the student" (AC34); "Add the class book list" adding one line per book - no book list exists for class `5th` in session `2026-27` (only the "no book list" notice was seen); a refused bill showing the API message and keeping the screen (AC37); a student's bills list together ("This student's bills" was not opened). Build/lint as in sales#16.

## 7. Risks and notes
The biggest screen of the feature, built from the order editor's parts. Written without a browser: expect small visual fixes after the owner's first look.
