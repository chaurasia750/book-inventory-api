---
name: new-feature
description: Start a new feature - add its rules to the module's single spec (docs/specs/<module>.md) with the next free ids, then draft its plans, and ask for one approval. Use automatically whenever the developer asks to build, add, create or start something the spec does not cover yet ("build password reset", "add audit log", "naya feature banao", "ye requirement implement karo"), or when work has no rules in a spec.
argument-hint: <module> <feature name>
---

Add the feature `$ARGUMENTS` (or the feature the developer described) following CLAUDE.md "Docs layout", "Development workflow" and "Automation". Do not stop between steps except at the gates.

1. Work out the module and the feature name from the request. Ask only if the module truly cannot be inferred from `CLAUDE.project.md` and the docs.
2. If the working tree is clean, create or switch to the local branch `docs/<module>-<slug>`. A new branch is always cut from the **current branch** (never switch to `main` first, `main` may be behind); say in one line which branch it was cut from. Mandatory, every time before cutting a new branch: `git fetch origin main`, then merge `origin/main` into the current branch (`git merge origin/main`), and only then cut the new branch. On a merge conflict, stop, show the conflicting files and ask; never resolve silently, never force.
3. If `docs/specs/<module>.md` does not exist, create it from `docs/templates/spec.md` and create `docs/plans/<module>/`.
4. Search the whole spec for the feature and its terms. Existing rules are reused by id, never copied.
5. Take the next free ids (highest `R`, `AC`, `D`, `Q` + 1; a spec that groups ids by part keeps its part prefix) and add, inside the existing sections: a bold feature heading with its rules, its data/API/UI/events, its acceptance criteria and decisions, and one line in "Purpose and scope". Mark the range `Draft` in the status line; set `Last updated`.
6. Fill the rules only from the docs named in `docs/README.md`. What they do not state becomes an open question with a recommended answer; never invent a business rule.
7. Continue straight into `/new-plan` for this feature (backend plan and, when needed, its UI plan).
8. Then stop once, at the approval gate: show the new rules, the plans, and all open questions as one numbered list with recommended answers.
