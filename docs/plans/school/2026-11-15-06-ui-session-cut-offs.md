# Plan school#06: UI session cut-offs setting

- **File:** `docs/plans/school/2026-11-15-06-ui-session-cut-offs.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/school.md](../../specs/school.md) - rules covered: R19 (the screen side); AC15
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `school#05`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-15 (chat: sales Q1-Q14 answered one by one)
- **Created:** 2026-11-15

## 1. In one minute
After this plan the Owner finds "Session cut-offs" under SETTINGS (platform U14): two day-and-month fields with their defaults and an example of what they mean. Open `/<company>/settings/sessions`.

## 2. Touches / leaves out
Touches: `apps/member` (a small settings page, the SETTINGS menu entry for the Owner), `libs/api-services`, i18n.
Leaves out: anything else.

## 3. Approach
One card, two pairs of day and month selects, the example sentence, Save; required `*`; states loading, error, success.

## 4. Tasks (at most 8)
- [ ] 1. Service and page.
- [ ] 2. Menu entry and route (Owner).
- [ ] 3. Build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R19`, `AC15` | 1, 2 | The Owner changes the Apr-Mar cut-off and sees it saved; an invalid day shows the API message; Staff do not see the entry |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Written without a browser.
