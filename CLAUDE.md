# Rules for Claude (generic)

These rules are project-independent and are copied unchanged between projects. Project specifics (stack, language, modules, layout, naming, tooling, environment, verification method) live in `CLAUDE.project.md`, imported below. If it is missing, ask.

@CLAUDE.project.md

Design decisions live in `docs/`. `docs/README.md` names this project's source-of-truth document, development guide and any legacy doc locations; read it first, then the relevant docs before building a feature. The source of truth wins over the development guide; specs are derived from both and change only with the user's approval. If `docs/README.md` is missing, ask.

## Docs layout (one spec per module, one backlog per project)

```
docs/specs/<module>.md                        the ONE spec of the module      (template: docs/templates/spec.md)
docs/plans/<module>/YYYY-MM-DD-NN-<slug>.md   the module's plans, one folder per module (template: docs/templates/plan.md)
docs/Backlog.md                               the ONE backlog of the whole project (NV/GAP/DEC/DEBT)
```

- **One spec per module.** Every feature of a module is a group of rules inside `docs/specs/<module>.md` (under a bold feature heading), never a separate spec file. Before adding a rule, search the spec: a rule, term or value exists there exactly once; refer to it by id instead of repeating it.
- **Ids are one serial per module:** rules `R1..`, criteria `AC1..`, decisions `D1..`, open questions `Q1..`. A new feature continues the numbering; ids are never reused or renumbered. A spec that already groups ids by part (for example `N1..` and `A1..`) keeps doing so.
- Module names are lower kebab-case and listed in `CLAUDE.project.md`; the spec file and the plan folder carry the same name.
- Plan file name: `YYYY-MM-DD` = creation date, `NN` = two-digit serial inside `docs/plans/<module>/` (next = highest NN there + 1; list the folder, do not guess). Order is by NN, not by date. Never reuse or renumber. A UI plan's slug starts with `ui-`.
- A plan is referred to as `<module>#NN` everywhere (dependencies, commits, backlog).
- **One backlog per project:** `docs/Backlog.md`, with a module column and one running `B` number. No other backlog files.
- **Approval is per rule range.** The spec's status line says which ranges are approved ("R1-R58 Approved; R59-R82 Draft"); only approved rules may be built.
- Legacy docs named in `docs/README.md` are read-only history.
- Helper skills: `/new-feature`, `/new-plan`, `/start-plan`, `/close-plan`, `/plan-status`. They apply these rules; see "Automation".

## Automation (keep the developer's work small)

- **The developer talks in plain words; Claude picks the skill.** "Build password reset", "implement iam#06", "what is in progress?", "is it done?" are enough. Claude runs the matching skill itself and never asks the developer to type a skill name.
- **Chain the steps without stopping in between:** new feature -> rules in the spec -> plans (backend + UI) -> one approval request; once approved -> start the plan -> when every task is ticked, close it -> one summary. The developer is interrupted only at the gates below.
- **Human gates (the only times Claude waits):** (1) approval of rules and plans; (2) answers to business open questions; (3) actions that are outward or hard to undo: commit, push, PR, deleting files or data, applying a migration, anything the Safety rules list.
- **Everything else Claude decides itself, without asking:** next ids and plan numbers, owner (= the current git user), creating or switching the local branch for the work (only with a clean working tree), status updates, running the build and the checks, filling the backlog.
- **Ask once, in one message.** Collect every question of a step into one numbered list, each with a recommended answer, so the developer can reply "ok" or change only what they disagree with.

## Development workflow: spec -> plan -> code (mandatory)

Every AI-assisted development task follows this order. Do not write production code before the spec and plan exist and the user has approved them.

1. **Spec first** - `docs/specs/<module>.md`. A new feature is added to it as a rule group (`/new-feature`, which also creates the module spec if it does not exist); if behaviour changes, update the existing rules. A spec says WHAT and WHY (rules, data, API, UI, events, acceptance criteria, open questions); never implementation steps. Derive it from the docs named in `docs/README.md` and link them; do not duplicate them. Unknown business rules go into "Open questions" - ask, do not invent. When the user delegates a decision to Claude, it is written in the spec's "Decisions" marked "proposed by the assistant" and the rules it shapes stay `Draft` until the user approves them.
2. **Plan second** - `/new-plan`. Plans may be drafted while their rules are still `Draft`, so both can be reviewed together. Each plan is one deliverable slice and names the rules/criteria it covers.
3. **Approval** - show the user the spec change and the plan(s) together. On approval, mark the rule range `Approved` in the spec and set the plans `Approved` with approver and date, in the same docs PR, before it is merged.
4. **Code** - `/start-plan` (checks approval, dependencies and ownership, sets `In progress`), then implement the tasks in order, tick them as they are done, and follow every other rule in this file.
5. **Close** - `/close-plan`: verify (build clean, then every row of the plan's "Spec coverage" by hand as the project's verification method describes), set the plan `Done` with the completion date, link it in the spec's "Plans" list, and copy what was not verified into `docs/Backlog.md`. The spec must describe the behaviour as it now is.

**One plan per feature**
- No plan exists without rules in the module spec, and no rule is built without a plan.
- Every feature gets its own plan(s). Never bundle two features into one plan, and never hide a second feature inside another plan's tasks.
- A feature that needs more than one deliverable slice gets several plans, each independently verifiable.
- Work that spans modules is one plan per module, linked with `Depends on` (respect the module dependency direction in the project file).

**Plans stay small**
- A plan has at most 8 tasks and fits on about two screens. If it grows past that, split it before asking for approval.
- Every plan opens with "In one minute": what will exist after it, why, and how to see it working (the request to send or the screen to open). No jargon the spec does not already use.
- Every plan lists the files/areas it touches and what it deliberately leaves out.

**UI plan after every plan**
- Each plan that adds or changes behaviour a user can see or trigger is followed by its own UI plan for the same feature (next NN, slug `ui-...`, `Depends on` the backend plan). Backend and UI are never mixed in one plan.
- The UI plan names the screens/components, the endpoints they call, what the user sees in each state (loading, empty, error, success), and the criteria it covers.
- A plan with no user-visible surface (background job, internal refactor) says `UI plan: none, <reason>` instead.
- If the UI code lives in another repository (named in the project file), the UI plan still lives here; it names that repository and its build/lint command.
- The UI plan is written and shown together with the backend plan, and approved before the backend plan is closed.

**Plan traceability**
- Every plan has a "Spec coverage" table: each rule and criterion it claims, the task that implements it, and the verification that proves it. A rule with no row is not covered; a row with no observed result is not done.

**Plan lifecycle**
- Statuses: `Draft` -> `Approved` -> `In progress` -> `Done`, or `Superseded by <module>#NN`.
- A finished plan is history: do not rewrite it. Changed approach or follow-up work = a new plan.
- Specs are living documents (always current behaviour); plans are an ordered log per module.
- Exempt from spec/plan (still build and verify): typo/comment/formatting changes, dependency version bumps, and bug fixes that restore behaviour an existing spec rule already states. Anything that adds or changes behaviour is not exempt, even when it is called a "fix": a bug fix that reveals the spec was wrong or silent updates the spec first. A hotfix that had to change behaviour gets its spec update and plan right after.
- Never mix unrelated work into one plan; if scope grows mid-way, stop, write the extra work as a new plan, and tell the user.

## Working in a team (several developers)

- **One owner per plan.** The plan's `Owner` field names the one developer working on it; do not pick up a plan someone else owns without asking.
- **The module spec is shared.** Change only the rule group of your feature; touching another feature's rules, the glossary or shared sections goes in its own small PR. Rebase before merging a spec change, and re-check that ids you added are still the next free ones.
- **One plan, one branch** (see "Branching" below). Spec and plan are approved before the code PR is opened; a code PR names its plan id.
- **Shared files change separately.** Shared code and project config are changed in their own small PR, never buried in a feature PR.
- **Schema changes are serialised.** At most one open schema-changing plan per module at a time. Before merging, rebase on the main branch and regenerate the migration; never merge two generated schema snapshots by hand.
- **Depend by link, never by copy.** A plan that needs another plan's behaviour names it under `Depends on` and calls it through its public interface.
- **Before starting a session**, run `/plan-status` (or read the active plan) so you do not pick up a plan someone else is working on.

**Branching (trunk-based, short-lived branches)**

| Branch | Use |
|---|---|
| `main` | Always builds. Changed only through a reviewed PR, never by a direct push. Releases are tags on `main` (`vYYYY.MM.DD`); there is no long-lived `develop` branch. |
| `docs/<module>-<slug>` | Spec change and plan(s) of a feature. Merging this PR is the approval. |
| `feature/<module>-NN` | The code of exactly one plan (backend or UI; each has its own NN). |
| `fix/<short-name>` | Exempt bug fix that restores behaviour the spec already states. |
| `chore/<short-name>` | Dependency bumps, config, formatting. |
| `hotfix/<short-name>` | Urgent production fix, branched from the release tag and merged back to `main`. |

- Flow per feature: `docs/...` PR first, then one `feature/...-NN` PR per plan in NN order (a UI plan after its backend plan).
- Branches live a few days at most; rebase on `main` daily. Merge by squash; the squash commit and PR title are `<module>#NN: <summary>` (exempt work: `fix|chore|docs: <summary>`).
- Every PR has at least one human review; PRs touching money, authorization or migrations always do.
- A user-visible feature merged before its UI plan is done stays behind a feature switch.
- Never work on a branch someone else owns; create your own and say so.
- Claude creates and switches local branches itself (see "Automation"); it commits, pushes and opens PRs only when asked, and never changes branch protection or other remote settings.

**Change control**
- Authentication, session, token, cookie, CORS and permission mechanisms are spec-level. Changing how they work needs a spec update and approval even when the change is small.
- Never put a behaviour change under an exempt commit prefix (`fix|chore|docs`).
- Docs in `docs/` that are not specs or plans (requirement drafts, imports, notes) are input only and never override a spec. If one conflicts with a spec, stop and ask the user which wins; do not code from the draft.
- Taking a pattern, design or code from another project or repository needs a plan entry saying what is taken and why, plus the user's confirmation on ownership/licence. Do not copy silently.
- A file named as a source of truth or linked from a spec must exist in the repository. If it is missing, report it and ask; do not assume its content.
- "Today", due dates and month/day boundaries use the business time zone written in the spec; store UTC. If the spec is silent, ask.

## AI guardrails (apply to every task)

**Stay in scope**
- Do exactly what the approved plan says. No drive-by refactors, renames, reformatting of untouched code, or "while I'm here" features. Spotted something else? Report it, or write it up as a new plan.
- Do not add a dependency/package, new project, or new architectural pattern without asking first. Prefer what the framework and shared libraries already provide.
- Ambiguity about a business rule, money, permissions or data ownership: stop and ask. Never guess and never silently pick a default.

**Never fabricate**
- Do not invent APIs, package versions, config keys or CLI flags: check the code, docs, or build output. If something was not verified, say so.
- Do not claim work is done, working, or verified unless you ran the build and the scenario/endpoint and saw it pass. Report failures and skipped steps plainly with the output.

**Money**
- Amounts use an exact decimal type, never float/double, and never mix currencies in one calculation.
- Rounding mode and rounding point are written in the spec; they are a business decision - ask.
- Anything that moves money is idempotent (idempotency key / unique constraint), safe to retry, and runs in one transaction. Ledgers are append-only: mistakes are fixed with a reversing entry, never an edit or delete.
- Every money rule in the spec is verified by a scripted run with exact numbers worked out independently first, including rounding edges and zero/negative/maximum cases; inputs and observed outputs go into the plan's "Spec coverage" table.

**Security**
- Every endpoint is authenticated unless the spec names it anonymous; authorization (role and ownership) is checked server-side, never trusted from the client. Never weaken auth, CORS, validation or the exception handler to make something work.
- Never commit or print secrets, tokens, real customer data (contact details, ID numbers, bank details). No personal data in logs or exception messages. Use fake data in checks and seeds.
- Validate all input at the API boundary (length, range, format); never build SQL, file paths or shell commands from input.

**Safety of the environment**
- Never run destructive commands (drop/truncate, reset --hard, force push, deleting folders, applying migrations to a shared or production database) without explicit approval for that exact action. Use a local/dev database only.
- Migrations: generate them and show the SQL to the user, with the target database name, before applying; that is confirmed before the plan closes. A migration that drops/renames a column or table, or rewrites data, needs explicit approval and a rollback note in the plan. (Team rule on schema changes: "Working in a team".)
- Do not push, open PRs, or change remote/CI settings unless asked. Commit only when asked; one logical change per commit.

**Working memory**
- The plan file is the shared memory between sessions and developers: keep its task checkboxes and notes current as you go, so anyone can resume from the plan alone. Read the module spec and the active plan at the start of each task.
- When a plan is closed, everything it could not verify (and every known gap, follow-up or open decision) goes into `docs/Backlog.md` with the module and the next `B` number. The backlog is reviewed once all planned work is done; it does not block closing a plan.
- If the same mistake or review comment happens twice, propose a one-line rule for this file. Keep this file to rules that change behaviour; do not add background prose, and do not add project-specific names here (they go in `CLAUDE.project.md`).

## Definition of done

A task is done only when all of these are true:
1. The project build runs at 0 warnings, 0 errors, and every row of the plan's "Spec coverage" has its observed result recorded.
2. Verification is real, not asserted: an endpoint exercised with the observed response, a screen action with what was seen, or a scripted scenario with its output, following the project's verification method.
3. Spec, plan and backlog are updated (plan `Done`, plan link in the spec, not-verified items in `docs/Backlog.md`); no TODO/placeholder code, no commented-out code, no leftover debug files.
4. The change is summarised for the human reviewer: what changed, what was verified and how, what was NOT verified, and any decision the user must confirm.

## Coding standards (apply to every line of code)

**Size limits** (if a limit is exceeded, split the code; do not squeeze it)
- A method takes **at most 4 parameters**. Need more: group them into a request/command/options object.
- A class takes **at most 6 constructor dependencies**. More means it has too many responsibilities: split it (for example one service per use case).
- A method is at most **30 lines**, a class at most **300 lines**, a file holds one top-level type. Nesting deeper than 3 levels: use guard clauses or extract a method.
- Parameter/dependency count is checked when reviewing your own diff before saying a task is done.

**Design principles**
- Business rules and state transitions live in the domain model, never in controllers, orchestration services, persistence configuration or handlers. Orchestration code loads, calls one behaviour, saves.
- Name things in the language of the business (spec), not CRUD verbs; no `Manager`, `Helper`, `Utils`, `Data` or `Info` classes.
- A rule exists in exactly one place. Before adding code, search for an existing behaviour, guard or helper and reuse it; a second copy of a rule, mapping, query or message is a defect.
- Values that carry a rule (money, phone, email, percentage) are validated in one place, not checked ad hoc at each call site.
- Follow the pattern already used by neighbouring code in the module.
- No user-facing text hard-coded in logic: use a message code plus a catalogue. Errors carry codes, not prose.
- Return DTOs, never domain entities. No `TODO`, placeholder, or commented-out code.
