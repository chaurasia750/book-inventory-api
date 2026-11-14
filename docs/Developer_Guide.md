# Developer Guide: how to start and finish work

For every developer on the team, new or old. All development is done through the AI agent (Claude Code): the agent writes specs, plans and code; the developer steers it, answers business questions, reviews every diff and owns the result.
The agent's rules are in `CLAUDE.md` (generic) and `CLAUDE.project.md` (this project). This page shows the developer the order of work and what to check at each step.

Think of the agent as a fast junior developer that never gets tired and is sometimes confidently wrong. It writes drafts; people own the decisions and the review.

## The flow in one picture

```
you: "build password reset"  ->  Claude: rules + plans  ->  you: approve  ->  you: "implement iam#06"
     ->  Claude: code + checks + close  ->  you: review, "commit"  ->  merge  ->  next plan
```

**You never have to type a skill name.** Say what you want in plain words; Claude picks the right step itself.

You are only asked at three moments:
1. **Approve** the rules and plans (one message, all questions together, each with a recommended answer - reply "ok" or change what you disagree with).
2. **Answer a business question** the documents do not answer.
3. **Allow a risky step:** commit, push, PR, applying a migration, deleting something.

Three things never change:
1. **No code before approved rules and plan.**
2. **One plan = one branch = one PR.** Backend and UI are separate plans.
3. **Nothing is "done" without recorded proof** (the input sent and the output seen).

---

## 0. One-time setup

1. Clone the repository and follow the setup in `README.md` and the "Environment" section of `CLAUDE.project.md` (database, secrets, how to run).
2. Run the build command from `CLAUDE.project.md`. It must finish with 0 warnings, 0 errors before you change anything.
3. Read `docs/README.md`: it names the source-of-truth document and the development guide.

## 1. What to say

| You want to | Say (any language) | Claude does |
|---|---|---|
| Know what is going on | "kya chal raha hai?" / "what is in progress?" | shows open plans, owners, pending items |
| Same, without Claude | run `./scripts/plan-status.ps1` (or `-Module iam`) | table of every plan, done/pending per module, first pending number, what can start now |
| Start a new feature | "password reset banao" / "build password reset" | adds rules to `docs/specs/<module>.md`, drafts backend + UI plans, asks for one approval |
| Approve | "ok" / "approve" (or the changes you want) | marks rules and plans `Approved` |
| Build a plan | "iam#06 implement karo" / "implement iam#06" | checks approval, dependencies, owner; switches to `feature/iam-06`; codes task by task; runs the checks; closes the plan |
| Continue tomorrow | "kal wala kaam continue karo" / "continue" | resumes the plan from the first open task |
| Check if it is finished | "ho gaya?" / "is it done?" | build, every check with real output, what was NOT verified |
| Save the work | "commit karo" / "commit and open PR" | commits with `<module>#NN: <summary>`, opens the PR if asked |

Where things live (Claude keeps them up to date):
- `docs/specs/<module>.md` - the **one** spec of a module; every feature is a rule group inside it.
- `docs/plans/<module>/` - all plans of that module (`iam#06` = the 6th plan of `iam`).
- Plan ids such as `iam#06` in this guide are naming examples only; no plans exist in this project yet.
- `docs/Backlog.md` - the **one** backlog of the whole project.

## 2. Your part, step by step

1. **Every morning:** `git checkout main && git pull`, then ask "what is in progress?". Do not start a plan someone else owns.
2. **New feature:** ask for it. Read what comes back:
   - Are the rules what the business document says? Does any repeat a rule already in the spec?
   - Does "In one minute" in each plan make sense without reading code? At most 8 tasks?
   - Are the open questions answered by the business owner, not guessed?
3. **Approval:** reply "ok" (or your changes). Then "commit and open PR" - the docs PR is reviewed and merged. Merged = approved.
4. **Build:** "implement iam#06". While it works, read every diff: it must only touch what the plan's "Touches" lists. Anything new becomes a new plan, not an extra.
5. **Close:** Claude closes the plan itself when the tasks are done. **Re-run at least one check yourself** for anything touching money, permissions or migrations.
6. **PR:** "commit and open PR". At least one human review; money, authorization and migrations always get one. Merge by **squash**, delete the branch.
7. **Next:** the UI plan (`iam#07`), then the next pair. Open items are in `docs/Backlog.md`.

Rebase a long-running branch daily: `git fetch origin && git rebase origin/main`.

---

## 3. Other situations

| Situation | What to do |
|---|---|
| Continue tomorrow / someone else continues | Start a new Claude session and say "continue <id>"; it resumes from the first open task. Taking over someone else's plan needs their OK. |
| Bug where the spec already says the right behaviour | Branch `fix/<short-name>`, fix, build, verify, PR `fix: <summary>`. No new plan. |
| Bug where the spec is wrong or silent | It is a behaviour change: update the spec and write a plan first (steps 2-4). |
| Urgent production problem | Branch `hotfix/<short-name>` from the release tag, fix, PR to `main`, tag a new release. |
| Dependency bump, config, formatting | Branch `chore/<short-name>`, PR `chore: <summary>`. |
| Your plan changes the database schema | Claude checks that no other open plan in the module changes the schema. Before merging, rebase on `main` and regenerate the migration; never hand-merge two migrations. Show the generated SQL in the PR. |
| You need another plan's behaviour | Put it under `Depends on` and call its public interface. Never copy its code. |
| You must change another feature's rules, the glossary, shared code or config | Separate small PR, not inside your feature PR. Rebase before merging a spec change and check your new ids are still the next free ones. |
| A requirement doc conflicts with the spec | Stop and ask the business owner which one wins. Do not code from the draft. |
| The plan grows past 8 tasks | Stop and split it into a new plan. Tell the reviewer. |

## 4. Checklist before you open a code PR

- [ ] Branch is `feature/<module>-NN`, rebased on `main`.
- [ ] Build: 0 warnings, 0 errors.
- [ ] Every task in the plan is ticked.
- [ ] Every "Spec coverage" row has the input you sent and the output you saw.
- [ ] Not-verified items are in the plan and in `docs/Backlog.md`.
- [ ] Spec describes the behaviour as it is now; plan link added to the spec.
- [ ] Diff touches only what the plan lists. No TODO, commented-out code or debug files.
- [ ] No secrets or real customer data anywhere (code, logs, screenshots, prompts).
- [ ] Migration SQL shown in the PR, if there is one.

## 5. Names at a glance

| Thing | Format | Example |
|---|---|---|
| Module spec | `docs/specs/<module>.md` | `docs/specs/iam.md` |
| Module plans | `docs/plans/<module>/` | `docs/plans/iam/` |
| Backlog | `docs/Backlog.md` (one for the project) | row `B46`, module `iam` |
| Rule / criterion | `R#` / `AC#`, one serial per module | `R83`, `AC41` |
| Plan file | `YYYY-MM-DD-NN-<slug>.md` | `2026-10-28-06-request-reset.md` |
| UI plan file | `YYYY-MM-DD-NN-ui-<slug>.md` | `2026-10-28-07-ui-request-reset.md` |
| Plan id | `<module>#NN` | `iam#06` |
| Docs branch | `docs/<module>-<slug>` | `docs/iam-password-reset` |
| Code branch | `feature/<module>-NN` | `feature/iam-06` |
| Commit / PR title | `<module>#NN: <summary>` | `iam#06: queue reset email` |
| Exempt commit | `fix|chore|docs: <summary>` | `fix: lockout counter resets on success` |

## 6. Reviewing the agent's work (never skip)

| Area | Check |
|---|---|
| Money | Rounding, ledger balance, idempotency. Recompute the plan's verification numbers yourself, independently of the agent. |
| Authorization | Role AND ownership on every endpoint: a user cannot read another user's data. |
| Migrations | Read the generated SQL: no accidental drop/rename/data loss; unique/FK/index as the spec says. |
| Concurrency | Two requests at once on the same record (balances, status changes, assignments). |
| Security | No secrets or real customer data in code, logs, prompts or screenshots; input validated; error responses leak nothing. |
| Scope | The diff touches only what the plan lists; no new package or pattern nobody asked for. |
| Verification | Every "Spec coverage" row has real input and output. Nothing is done on the agent's word. If the project has no automated tests, this review is the safety net. |

## 7. Habits that keep the agent's output good

- **Point at documents, not chat memory.** "Implement `iam#06`" or "per `docs/specs/iam.md` R83-R88". Long sessions forget; the plan file does not.
- **New session per plan.** The spec and plan carry the context.
- **Spec before code.** An unclear business rule goes to "Open questions" and to the business owner, never into code as a guess.
- **Verify, don't trust.** "It builds, I ran the scenario, here is the output" is evidence; "it should work" is not. Ask for the actual output and re-run one check yourself for money or permissions.
- **Small PRs.** One plan, one PR, read by a person before merge. Never merge agent code nobody has read.
- **Keep the rules alive.** Same mistake twice: ask the agent to propose one line for `CLAUDE.md`. Rule never used: delete it.

## 8. Red flags: stop and ask

- A business rule, an amount, a rounding rule, a permission or data ownership is unclear.
- The agent says something is verified but the plan shows no input/output for it, or the output looks edited.
- A change touches auth, CORS, error handling or migrations and the plan did not say so.
- Numbers in the spec do not match the verification notes or the code (rates, slabs, percentages).
- The agent is certain about a package, API or version but there is no build output behind it.
- The diff is much bigger than the plan.
