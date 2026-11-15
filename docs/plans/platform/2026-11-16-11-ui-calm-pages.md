# Plan platform#11: UI calm page changes (no shaking, soft fade)

- **File:** `docs/plans/platform/2026-11-16-11-ui-calm-pages.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/platform.md](../../specs/platform.md) - rules covered: U19; PAC22, PAC23
- **Owner:** abhishek
- **Status:** Draft
- **Depends on:** none
- **UI plan:** this is itself a UI plan (the feature has no backend)
- **Approved by / on:** -
- **Created:** 2026-11-16

## 1. In one minute
After this plan clicking around the application no longer shakes the page: the scrollbar room is kept, loading shows placeholders of the final height, and a page change and arriving data fade in softly.

## 2. Touches / leaves out
Touches: the global styles of the three apps, the router setup of the three apps (the view transition), the shared skeleton and list components of `@org/ui`, the list pages that show a placeholder while loading.
Leaves out: the table cards (platform#10); any change of what a page shows.

## 3. Approach
The root stylesheet reserves the scrollbar gutter; the router uses the browser's view transition with a short fade (a CSS fade where the browser has none); a shared "appear" class fades arriving content; the skeleton rows take the height of the rows they stand for; a reload keeps the old rows until the new ones come; all of it is switched off under `prefers-reduced-motion`. The layout shift is measured before and after on the school list and a school's tabs.

## 4. Tasks (at most 8)
- [ ] 1. Reserve the scrollbar gutter in the three apps; measure the sideways shift.
- [ ] 2. The page change fade (view transition and the fallback) for the three apps.
- [ ] 3. The shared "appear" fade for arriving data and its reduced-motion switch-off.
- [ ] 4. Placeholders of the final height on the list pages and the tabs; a reload keeps the old rows.
- [ ] 5. Measure the layout shift (before and after) and the page speed; build and lint.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `U19`, `PAC22` | 1, 4, 5 | on the school list and a school's tabs with a slowed network: the layout-shift score before and after, and no sideways move of the content when the page grows taller than the screen |
| `U19`, `PAC23` | 2, 3, 5 | an opacity animation runs on a page change and on arriving data; with reduced motion emulated none runs; the time to show the data is unchanged |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
The view transition needs a Chromium-based browser; the CSS fade is the fallback. Seen through headless Chrome only.
