---
name: start-plan
description: Implement an approved plan (<module>#NN) end to end - check approval, dependencies, ownership and schema conflicts, switch to its branch, set In progress, do the tasks, and close it automatically when done. Use automatically whenever the developer asks to implement, build, code, continue or resume a plan or an approved feature ("implement iam#06", "iam#06 shuru karo", "continue", "kal wala kaam aage badhao").
argument-hint: <module>#NN
---

Implement plan `$ARGUMENTS` (or the plan the developer means; when unclear, the oldest `Approved` or `In progress` plan they own) following CLAUDE.md "Development workflow", "Working in a team" and "Automation".

1. Open `docs/plans/<module>/*-NN-*.md`.
2. Gates - if one fails, stop and say in one line what is missing and what would fix it:
   - the plan is `Approved` or `In progress`;
   - its rules are in an `Approved` range of the spec;
   - every plan under `Depends on` is `Done`;
   - the owner is the current git user (someone else: ask once before taking over);
   - a schema-changing plan: no other schema-changing plan of the module is `In progress`.
3. With a clean working tree, create or switch to the local branch `feature/<module>-NN` yourself. If it does not exist yet, first (mandatory, every time a new branch is cut) run `git fetch origin main` and merge `origin/main` into the current branch (`git merge origin/main`); on a merge conflict stop, show the conflicting files and ask, never resolve silently or force. Then cut it from the **current branch** (never switch to `main` first), and write the base branch name in the plan file as a line `Base branch: <name>` so `/close-plan` can open the PR against it. Resuming on the existing branch: keep it and its recorded base.
4. Set `In progress`. Read the covered rules and the areas in "Touches". Resuming: continue from the first open task.
5. Implement task by task and tick each in the plan file. Something outside "Touches" or a new behaviour: do not build it; note it as a new plan proposal for the final summary.
6. Gates on the way (stop only here): showing migration SQL before applying it, a business question the spec does not answer.
7. When every task is ticked, run `/close-plan <module>#NN` yourself.
