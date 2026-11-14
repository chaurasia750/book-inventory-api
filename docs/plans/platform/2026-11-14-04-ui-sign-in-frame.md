# Plan platform#04: UI sign-in frame (login, forgot, reset, welcome)

- **File:** `docs/plans/platform/2026-11-14-04-ui-sign-in-frame.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/platform.md](../../specs/platform.md) - rules covered: U11; UAC7
- **Owner:** abhishek
- **Status:** In progress
- **Depends on:** platform#02
- **UI plan:** this is itself a UI plan, no backend part
- **Approved by / on:** abhishek / 2026-11-14 (chat: "login page change kro", answered: like Relator, look only, flow unchanged)
- **Created:** 2026-11-14
- **Base branch:** `feature/school-screens-ui` (UI repo, owner's standing instruction: work on the current branch)

## 1. In one minute
After this plan the login page and the forgot, reset and welcome pages look like the owner's `relator-rj-ui`: a charcoal brand panel on the left, the form on the right. Open `http://localhost:4200/login`.

## 2. Touches / leaves out
**Taken from `relator-rj-ui` (owner confirmed ownership, 2026-11-14):** the structure of the brand shell. Adapted: no photo, Book Inventory logo and texts, inline icons.
Touches: `libs/layout` (new `AuthBrandShellComponent`, `AuthLayoutComponent` now uses it), `apps/public-site` login page markup.
Leaves out: the fields, validation, redirects and API calls of the pages; the text catalogue (existing `login.hero.*` keys are reused); a background photo.

## 3. Approach
One shared shell component projects the page; the login page wraps its existing card in it, and the shared auth layout wraps the router outlet for the other pages.

## 4. Tasks (at most 8)
- [x] 1. `AuthBrandShellComponent` in `libs/layout` (U11).
- [x] 2. `AuthLayoutComponent` uses the shell; forgot, reset and welcome inherit it (U11).
- [x] 3. Login page wrapped in the shell; the old split layout and phone badges removed (U11).
- [x] 4. `npx nx run-many -t build` and `-t lint`: 0 new findings against the base.
- [ ] 5. Look at the four pages in a browser, desktop and phone width, and sign in as Super Admin and as an Owner (UAC7).

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `U11` | 1-3 | Build passes; code reviewed against the reference |
| `UAC7` | 5 | Open the four pages; sign in as Super Admin and Owner: same redirects as before |

## 6. Not verified
To be filled when the plan is closed (no browser tool for the assistant: the owner looks at the screens).

## 7. Risks and notes
The forgot, reset and welcome pages keep their own small stylesheets; if one looks off inside the new frame it is a follow-up fix of that page only.
