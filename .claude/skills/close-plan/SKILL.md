---
name: close-plan
description: Close a finished plan against CLAUDE.md's Definition of done - build, verify every Spec coverage row, set Done, update the module spec, add not-verified items to docs/Backlog.md, and summarise for the reviewer. Use automatically when all tasks of a plan are ticked, or when the developer asks whether something is done, finished, verified or ready for review ("ho gaya?", "is it done", "close iam#06").
argument-hint: <module>#NN
---

Close plan `$ARGUMENTS` following CLAUDE.md "Definition of done". Do not mark anything done that was not actually run.

1. Open the plan file (`docs/plans/<module>/*-NN-*.md`). Every task must be ticked; if not, list the open tasks and stop.
2. Run the project build command from `CLAUDE.project.md` (for a UI plan, the build and lint command the plan names for the UI repository). It must end with 0 warnings and 0 errors; otherwise show the output and stop.
3. For every row of "Spec coverage": run the check (request, screen action, or script as the project's verification method says) and write the exact input and observed output in the row. A row that cannot be checked moves to "Not verified" with the reason.
4. Review your own diff for the coding standards (parameter and dependency counts, method/class size, no TODO/commented-out code, no leftover debug files).
5. If a migration was added: confirm the SQL was shown to the user and the target database named.
6. A Backend plan with a UI plan: the UI plan must be at least `Approved`; otherwise stop and tell the user.
7. Branch check: the current branch is `feature/<module>-NN` for this plan, not `main` and not another plan's branch. Branches are cut from the branch the work was based on (the plan's `Base branch` line), not necessarily `main`; do not require a rebase on `main`. Report a mismatch in the summary and stop before step 10.
8. Update:
   - plan: status `Done`, completion date;
   - `docs/specs/<module>.md`: plan status `Done` in "Plans", rules and text describing the behaviour as it now is (if the code had to differ from a rule, the rule is corrected with the user's approval, never silently), `Last updated`;
   - `docs/Backlog.md`: one row per "Not verified" item and every known gap, follow-up or open decision, with the module and the next `B` number.
9. Commit, push and PR (mandatory and automatic once steps 1-8 pass; the developer's standing authorization, do not ask): 
   - commit all the plan's changes (code, plan, spec, backlog) on the plan's branch, message `<module>#NN: <summary>` plus the attribution line from the session; one logical change per commit, no secrets or scratch files;
   - push the branch to `origin` (`git push -u origin <branch>`), never force;
   - open the PR with `gh pr create` against the plan's `Base branch` (if the line is missing, the branch it was cut from; if that cannot be told, ask once), title `<module>#NN: <summary>`, body = the reviewer summary from step 10 plus the attribution line.
   If any step fails (hook, auth, conflict), show the output and stop; never skip hooks or force.
10. Give the reviewer summary: what changed, what was verified and how, what was NOT verified, decisions the user must confirm, and the PR link. If the plan has a UI plan, offer to start it next.
