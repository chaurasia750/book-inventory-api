# Plan platform#02: UI app shell and theme (sidebar, header, palette)

- **File:** `docs/plans/platform/2026-11-14-02-ui-app-shell-theme.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/platform.md](../../specs/platform.md) - rules covered: U9; UAC5
- **Owner:** abhishek
- **Status:** In progress
- **Depends on:** none
- **UI plan:** this is itself a UI plan, no backend part
- **Approved by / on:** abhishek / 2026-11-14 (chat: asked for the same design and menu as `relator-rj-ui`, confirmed it is the owner's own project, same colours)
- **Created:** 2026-11-14
- **Base branch:** `feature/school-screens-ui` (UI repo, owner's standing instruction: work on the current branch)

## 1. In one minute
After this plan the `admin` and `member` apps look like the owner's `relator-rj-ui`: a dark charcoal sidebar with search, sections, sub-menus, a collapsed icon mode and a user card, a slim header bar, and the charcoal and gold colours. Open `http://localhost:4200` as Super Admin or as a company Owner and look at the menu.

## 2. Touches / leaves out
**Taken from `relator-rj-ui` (owner confirmed ownership, 2026-11-14):** `libs/layout` app-sidebar (ts, html, scss), app-header (ts, html), app-layout, `services/user-label.ts`, the colour palette in `tailwind.config.js`, the theme tokens of each app's `styles.scss`. Adapted: logo path, app name, no notifications, no page trail.
Touches: `libs/layout` (shell components), `tailwind.config.js`, `apps/{admin,member,public-site}/src/styles.scss`, the menu definitions of the two layouts (sections and icons only; no new routes).
Leaves out: page content components (tables, forms, dialogs), the login page layout, notifications, the page trail (breadcrumbs), new menu entries, any API change.

## 3. Approach
Copy the shell files, replace their relator-specific parts (logo, brand, features), keep our `SidebarService`, `AuthService` and roles. Menu items keep their role filter. The palette comes from the reference tailwind config as a whole, so every existing `brand`, `gold`, `saffron` class resolves to the same gold as in the reference.

## 4. Tasks (at most 8)
- [x] 1. Palette: `tailwind.config.js` and the theme tokens of the three `styles.scss` as in the reference (U9).
- [x] 2. `user-label.ts` and the sidebar (ts, html, scss) with search, sections, sub-menus, user card (U9).
- [x] 3. Header bar and layout as in the reference, without notifications and page trail (U9).
- [x] 4. `admin` and `member` layouts: menu entries with sections and icons per role; logo and app name (U9, U6).
- [x] 5. `npx nx run-many -t build` and `-t lint`: 0 new findings against the base.
- [ ] 6. Look at both apps in a browser and record what was seen (UAC5).

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `U9` | 1-4 | Build passes; shell code reviewed against the reference |
| `UAC5` | 6 | Open admin as Super Admin and member as Owner and Staff: search, Ctrl K, sub-menu, collapse, user card, Sign out |

## 6. Not verified
To be filled when the plan is closed (no browser tool is available to the assistant: the owner looks at the screens).

## 7. Risks and notes
Swapping the palette recolours the whole UI, including the login page, the same way the reference looks. A class that was green by name (`bg-brand-500`) becomes gold; this is wanted ("same design").
