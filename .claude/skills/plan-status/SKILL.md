---
name: plan-status
description: Show the state of all modules and plans (docs/specs, docs/plans/<module>/) - owner, status, depends-on, open backlog items - newest first. Read-only. Use automatically when the developer asks what is in progress, what to do next, who works on what, or what is pending ("kya chal raha hai", "aage kya karna hai", "status batao"), and before picking a plan when none was named.
argument-hint: [module]
---

Report the state of planned work (filter by `$ARGUMENTS` if given). Read-only: change no file.

1. For each `docs/specs/<module>.md` read the status line (which rule ranges are Draft or Approved) and the "Plans" line.
2. For each plan in `docs/plans/<module>/` read from the header: id (`<module>#NN`), kind, status, owner, depends on, created/completed dates.
3. Count rows with status `open` per module in `docs/Backlog.md`.
4. Print, newest plan date first:
   - a table of plans that are `Draft`, `Approved` or `In progress` (these are taken: do not start them without the owner),
   - a table per module: spec status, plans done/total, open backlog items,
   - warnings: a Backend plan without a UI plan or a `UI plan: none` line; two open schema-changing plans in one module; a plan whose `Depends on` is not `Done`; a plan missing from its spec's "Plans" line; a duplicate rule id in a spec.
