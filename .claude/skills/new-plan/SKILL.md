---
name: new-plan
description: Draft the next plan for a module (docs/plans/<module>/YYYY-MM-DD-NN-<slug>.md) plus its paired UI plan, enforcing the size, UI and traceability rules in CLAUDE.md. Use automatically right after new rules are added, or whenever the developer asks for a plan, a next step, or to split work ("plan banao", "is feature ka plan", "split this").
argument-hint: <module> <slug>
---

Draft the next plan for `$ARGUMENTS` (or for the rules just added) following CLAUDE.md ("Docs layout", "Plans stay small", "UI plan after every plan", "Plan traceability", "Automation"). Decide everything below yourself; do not ask.

1. The module spec `docs/specs/<module>.md` must contain the rules the plan covers (`Draft` or `Approved`); if not, run `/new-feature` first.
2. List `docs/plans/<module>/` and take next NN = highest NN + 1 (two digits, `01` when empty).
3. File `<today YYYY-MM-DD>-<NN>-<slug>.md` from `docs/templates/plan.md`: status `Draft`, kind `Backend` (or `UI` when the slug starts with `ui-`), id `<module>#NN`, spec link `../../specs/<module>.md`, owner = the current git user.
4. Fill: "In one minute" in plain words with how to see it working; "Touches / leaves out"; at most 8 tasks (more: split into several plans on your own); "Spec coverage" with one row per rule/criterion and its planned check, so that every rule of the feature is covered by some plan; schema change yes/no (if another open plan of the module changes the schema, add a `Depends on` to it).
5. User-visible behaviour: create the UI plan with NN + 1, slug `ui-<slug>`, kind `UI`, `Depends on` the backend plan, naming the UI repository and its build/lint command when the UI lives elsewhere, the screens, endpoints, each state (loading/empty/error/success) and the criteria. Otherwise write `UI plan: none, <reason>`.
6. Add the plans to the spec's "Plans" line as `Draft`.
7. If called on its own (not from `/new-feature`), stop at the approval gate: show the plans and any `Draft` rules they cover. On approval, set the rule range and the plans `Approved` with approver and date.
