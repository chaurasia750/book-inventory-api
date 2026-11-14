# Plan platform#06: UI member app on a phone

- **File:** `docs/plans/platform/2026-11-14-06-ui-member-mobile.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/platform.md](../../specs/platform.md) - rules covered: U13; UAC9
- **Owner:** abhishek
- **Status:** In progress
- **Depends on:** platform#03, platform#04
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-14 (chat: "pura member page mobile responsive hoga")
- **Created:** 2026-11-14
- **Base branch:** `feature/school-screens-ui` (UI repo), owner's standing instruction

## 1. In one minute
After this plan the company's screens (Schools, Staff, Audit log, Settings, Change password, login) work on a 360 px phone: no page-wide sideways scroll, the menu is a drawer, tables scroll inside their box, buttons and inputs are easy to tap. Open the company app in the browser's phone mode.

## 2. Touches / leaves out
Touches: the shared shell (`libs/layout`: header, sidebar drawer, content padding), the shared UI (`libs/ui`: table box, modal, pagination), and the member pages (`apps/member/src/app/pages/*`, `libs/layout` account and audit screens). The admin app benefits where it shares components.
Leaves out: new features, desktop layouts, a native-app feel (no offline, no install).

## 3. Approach
Fix from the shell outwards: the page never scrolls sideways (min-width and overflow rules in the shell), tables sit in `overflow-x-auto` boxes, filter rows wrap, header actions collapse, dialogs use the full width with scroll inside, tap targets are at least 40 px.

## 4. Tasks (at most 8)
- [ ] 1. Shell: header, drawer sidebar, content padding and no page-wide overflow at 360 px (U13).
- [ ] 2. Shared UI: modal, pagination and table box fit a phone (U13).
- [ ] 3. Login and the sign-in frame (U13).
- [ ] 4. Schools and Staff: list, panel, forms, class editor (U13).
- [ ] 5. Settings, Change password, Audit log (U13).
- [ ] 6. Builds and lint: 0 new findings.
- [ ] 7. Look at every member screen at 360 px in a browser and record what was seen (UAC9).

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `U13`, `UAC9` | 1-5 | Code review of every screen's classes; builds pass |
| `UAC9` | 7 | Browser phone mode at 360 px on each member screen (owner's check) |

## 6. Not verified
To be filled when the plan is closed (no browser tool for the assistant).

## 7. Risks and notes
Without a browser the work is done by reading the markup against the rules above; expect small visual fixes after the owner's first look.
