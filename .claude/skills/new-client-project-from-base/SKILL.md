---
name: new-client-project-from-base
description: "Set up a new client's project (API + UI) from an existing base repo: copy, rename everything from the project name, clean the old project's plans and docs, push. Setup only; features follow spec and plan."
---

# New client project from a base repo (API + UI)

A new client's project starts as a copy of one of the company's existing projects (an API repo and a UI repo). The user gives the **new project name** and the **base repos**; this skill finds everything that belongs to the base project by scanning, and changes or removes it.

## Principles

- **Discover, never assume.** No file name, key, value or business term is hard-coded in this skill. Every list below is produced by a scan of the base repo, shown to the user once, and then applied. If a scan finds nothing, that is a result, not an error.
- **Scope: initial setup only.** Copy, rename, configuration, old docs. No business plan reading, no specs, no plans, no feature code: features follow the base repo's own flow (`CLAUDE.md`: spec -> plan -> approval -> code).
- **Only reference what is in the repository.** Never write into the repo a pointer to a file that lives outside it (a chat attachment, a claude.ai Project file, a local path): the repo rules require every source-of-truth file to exist in the repository. If the client's business document should be the source, ask the user to add it under `docs/business/` and link it then; until then write "none in the repository yet".
- **Ask once.** All questions of the skill go into one message (Step 2), each with a recommended answer.
- **Every step ends with a Check.** Do not continue while a check fails.
- Reply in the language and style the user prefers; tables for lists.
- Never print huge files: pipe scans through `cut -c1-200 | head -100`.

## Placeholders

`OLD` = the base project, `NEW` = the client project. Each has these **name forms**:

| Form | Rule |
|---|---|
| Display | as written by people, with spaces |
| Pascal | words capitalised and joined |
| kebab | lower case, joined with `-` |
| snake | lower case, joined with `_` |
| lower | lower case, joined |
| UPPER | upper case, joined with `_` |
| Initials | first letter of each display word, upper case |

---

## Step 1. Inputs and OLD name detection

Ask the user for: (1) the NEW **display name**; (2) the base **API repo** and **UI repo**; (3) which module specs of the base will be **rewritten** for NEW (their spec files are removed in Step 7; default none).

Get access (Step 3), then detect the OLD name forms from the base repos instead of asking:

```bash
cd <OLD-api>
ls *.sln *.slnx 2>/dev/null                                   # kebab form
ls src/ 2>/dev/null; git grep -hoE '<RootNamespace>[^<]+' -- '*.props' '*.csproj' | sort -u   # Pascal form
head -3 README.md CLAUDE.project.md 2>/dev/null               # display form
cd <OLD-ui>; grep -m1 '"name"' package.json                   # package scope
```

Derive all OLD and NEW forms from the rules above. **Check:** every OLD form occurs in the base repo (`git grep -c "<form>" | head`); a form with zero hits is dropped from the replace list.

## Step 2. Discover the project-specific values (the profile)

Run these scans on **both** base repos and collect every hit into one profile table.

| Scan | What to run (in each repo) |
|---|---|
| A. Files that carry configuration | `git ls-files \| grep -iE '(appsettings[^/]*\.json\|launchSettings\.json\|web\.config\|\.env[^/]*\|environment[^/]*\.(ts\|js\|json)\|proxy[^/]*\.json\|\.(ps1\|sh\|cmd\|bat)$\|docker[^/]*\|\.ya?ml$)'` |
| B. Identity values that must be unique per project | `git grep -nE '<UserSecretsId>\|[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}' -- '*.csproj' '*.props' '*.json'` |
| C. Values inside the files of scan A | hosts and URLs, ports (`:[0-9]{4,5}`), database names and users (connection strings, script parameters), file paths (`[A-Z]:/`, `/var/`, `Inetpub`), e-mail addresses, IP addresses, pool/site names, seed values (keys containing `Seed`), display names (keys like `Name`, `DisplayName`, `CompanyName`, `Title`) |
| D. Defaults in code that carry a client value | `git grep -nE '\{ get; set; \} = "[^"]+"' -- '*.cs'` and `git grep -nE "export const [A-Z_]+ = '[^']+'" -- '*.ts'` (keep only names, prefixes, codes, keys and texts) |
| E. Codes built from OLD initials | `git grep -nE "\b<OLD Initials>[0-9]{3,}\b"` and string defaults equal to the OLD initials (member, order, invoice prefixes) |
| F. Storage and cookie keys | `git grep -nE "(localStorage\|sessionStorage\|STORAGE_KEY\|CookieName)"` |
| G. Contact data in UI texts | `git grep -nE "[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[a-z]{2,}\|\+?91[ -]?[0-9]{10}\|GSTIN" -- '*.json' '*.html' '*.ts'` |

Classify every hit into exactly one class and fill the NEW value by its rule:

| Class | How to recognise it | NEW value |
|---|---|---|
| **Name-derived** | contains an OLD name form | replace the form (automatic in Step 4) |
| **Initials-derived** | OLD initials as prefix or inside codes | NEW initials (ask if two clients could clash) |
| **Unique identity** | GUIDs that identify the project (user-secrets id, app ids) | a **new** random GUID, never copied |
| **Display text** | a human-readable name or title | NEW display name |
| **Environment** | production host, server IP, pool/site name, prod database/user, media paths, local ports | **ask**; default = OLD value with the name forms replaced; local ports default = unchanged |
| **Contact / company data** | e-mail, phone, address, tax ids | **ask**; default = empty or placeholder, never the OLD client's data |
| **Secret** | passwords, keys, tokens | never in the repo; the base's own secret setup (scripts or docs found in scan A) is re-run by the user |
| **Shared / neutral** | framework values, the company's own shared names | keep |

Show the table (key, file, OLD value, class, NEW value) in **one** message with the *ask* rows marked. **Check:** the user answered or said ok.

## Step 3. Repo access

1. Attach both base repos (`add_repo`), clone each once (`git clone --depth 1 <url> /home/claude/<repo>`, long timeout), `register_repo_root`.
2. "Not found / no access": tell the user to reconnect GitHub in claude.ai connectors and allow the org; no retry loop.
3. The session cannot create repos: ask for two **empty** private repos named like the base repos with the NEW kebab form, then `add_repo` them with push access.

**Check:** both base clones have a `HEAD`; `list_repos` shows both new repos with `can_push: true`.

## Step 4. Copy and replace the name forms (both repos)

```bash
mkdir /home/claude/<NEW-repo> && git -C /home/claude/<OLD-repo> archive HEAD | tar -x -C /home/claude/<NEW-repo>
cd /home/claude/<NEW-repo> && git init -q -b main && git add -A
git grep -lE "<OLD Pascal>|<OLD kebab>|<OLD snake>|<OLD lower>|<OLD UPPER>" | head -100      # look first
git grep -lE "<OLD Pascal>|<OLD kebab>|<OLD snake>|<OLD lower>|<OLD UPPER>" | xargs perl -pi -e 's/<OLD Pascal>/<NEW Pascal>/g; s/<OLD kebab>/<NEW kebab>/g; s/<OLD snake>/<NEW snake>/g; s/<OLD lower>/<NEW lower>/g; s/<OLD UPPER>/<NEW UPPER>/g'
find . -depth \( -name "*<OLD Pascal>*" -o -name "*<OLD kebab>*" \) -not -path "./.git/*" | while read p; do mv "$p" "$(dirname "$p")/$(basename "$p" | sed 's/<OLD Pascal>/<NEW Pascal>/g; s/<OLD kebab>/<NEW kebab>/g')"; done
```

Safety rules (they hold for any project):
- Replace only the joined forms. **Never** `sed` single display words or the initials: short tokens occur inside data and other words. Those are handled one by one in Step 5.
- Do not rename binary assets (images, fonts) unless every reference changes in the same commit.
- Data files (seed SQL, reference data, fixtures): scan, but edit only a hit that is clearly the OLD name.

**Check:** the look-first grep prints nothing; `find . -iname "*<OLD lower>*" -not -path "./.git/*"` prints only binary assets kept on purpose; every project the solution or workspace lists exists (`grep -o 'Path="[^"]*"' *.slnx`, `package.json` workspaces, `project.json` files).

## Step 5. Apply the profile (both repos)

1. Every Step 2 row that is not *Name-derived*: change the value in its file, keeping the file's format.
2. *Display text*: after Step 4 the Pascal form may sit where people read it. Find it with `git grep -nE "\"[^\"]*<NEW Pascal>[^\"]*\""` in JSON, HTML, i18n and TS texts and use the display form.
3. *Display phrase in UI texts*: `git grep -l "<OLD Display>" -- '*.ts' '*.html' '*.json'`, replace the whole phrase. Capital or split variants (brand lockups, two i18n keys) are found with `git grep -niE "<OLD word 1>.{0,5}<OLD word 2>"` and the per-word keys next to them, and fixed by hand.
4. Developer docs that name environment values (found by scan A or under `docs/`): use the profile values; OLD server, IP or publish-profile details that do not apply become "ask the owner".
5. Root README: if it is not about the application (for example a shared library's README), move it next to that library and write a short application README (what NEW is, that it started as a copy of OLD, layout, build command, sibling repo).
6. Project rule files (`CLAUDE.md`, `CLAUDE.project.md`, `AGENTS.md`, `.claude/`): fix names, paths, env vars and the "business docs" pointer; keep the rules.
7. List for the user what cannot be derived: logo and icon files, marketing copy that describes OLD's business, contact data left empty.

**Check (repeat until clean):**
```bash
git grep -niE "<OLD lower>|<OLD Display>|<OLD kebab>|<OLD snake>" | cut -c1-160      # only kept assets / listed items
git grep -nE "\b<OLD Initials>[0-9]{3,}\b"                                             # nothing
# every Unique-identity row now differs from the base repo
```
Run the build and lint commands that the repo's rule files or `package.json` name. If the tool chain is missing in the session, say so and ask the user to run them; never claim a build passed.

## Step 6. First commit and push (both repos)

```bash
git add -A && git commit -m "Initial copy of <OLD repo> renamed to <NEW>" -m "<attribution lines the session asks for>"
git remote add origin <new repo url>
git -c push.negotiate=true push -u origin main
git checkout -b docs/<NEW kebab>-setup && git -c push.negotiate=true push -u origin docs/<NEW kebab>-setup
git ls-remote origin
```
"push negotiation failed" followed by `[new branch]` is a success. **Check:** both branches exist in both repos.

## Step 7. Classify and clean the OLD docs (each repo that has docs)

Read the docs index the rule files point to and list every doc (`git ls-files docs`). Put each into one class by what it **is**, not by its name:

| Class | How to recognise it | Action |
|---|---|---|
| Rules and templates | referenced by the rule files as rules, templates, skills | keep |
| Developer guides | setup, deployment, release, how to work | keep (values fixed in Step 5) |
| Specs of modules that stay | the module's code stays | keep, clean the header, audit in Step 8 |
| Specs of rewritten modules | named by the user in Step 1 | delete |
| Plans | the plans folder | delete the files, keep the folder |
| Backlog | the project backlog | empty it, keep header and columns |
| OLD client's business docs | the index calls them source of truth or client docs | delete |
| Research, roadmap, batches, notes, reference sites | input or history of OLD | delete |
| Samples and designs | sample data, reference designs | keep if neutral, delete if they show OLD client data |

Show the classification before deleting. Then:
- Header of each kept spec: replace status / last-updated / source / plans lines with one status line ("carried over from the base project; describes the code as it is") and one source line (the NEW business docs, linked from the docs index when they arrive). Remove links to deleted files, keep their text.
- Rewrite the docs index, and every pointer in rule files, helper scripts and skills that names a deleted doc or old plans (`git grep -n "<each deleted file name>"`).

**Check:** `git grep -nE "<deleted file names>"` prints nothing outside the notes you wrote.

## Step 8. Audit the kept specs for OLD business (API repo)

The specs are the agent's source of truth. A kept spec still holds rules of OLD's business; unmarked, the agent builds on them. Rules are **marked, not deleted** (they still describe the code).

1. **Build the OLD term list from the base repo, not from memory:**
   - bold terms and headings of the deleted business docs and deleted specs (`git -C <OLD-api> show HEAD:<file> | grep -oE '\*\*[^*]{3,40}\*\*'`);
   - enum members and seed names of the business modules (`git -C <OLD-api> grep -nE "enum |Seed" -- '*.cs'`);
   - percentages, amounts and level names in the deleted docs (`grep -oE '[0-9]+(\.[0-9]+)?%|Level [0-9]+'`);
   - OLD display words, OLD initials codes, deleted file names and titles;
   - the plan-id patterns of the docs layout (`<module>#NN`, numbered legacy plans).
2. Scan every kept spec and guide with that list, and for **dead file references** (`docs/...`, `../...md`, `](...md)`, bare `*.md` names that `find` does not find).
3. **Encoding damage** (UTF-8 read as Windows-1252), found and repaired generically:
   ```python
   import re,sys
   for p in sys.argv[1:]:
       s=open(p,encoding="utf-8").read()
       def fix(m):
           try: return m.group(0).encode("cp1252").decode("utf-8")
           except Exception: return m.group(0)
       t=re.sub(r"[Â-ô][\u0080-˿‐-›€™]{1,3}",fix,s)
       if t!=s: open(p,"w",encoding="utf-8").write(t); print("fixed",p)
   ```
4. Insert in **every** kept spec, before its first `## ` heading, a block filled from the scan:
   ```markdown
   ## Base-project rules under review (<NEW Display>)

   - **Old references in this file:** <the kinds found: numbers of removed specs, removed business docs, plan ids, documents that never existed>. These are history only: do not look for them and do not build from them.
   - **How to work with the rules below:** rules not listed in the table describe the code as it is and stay valid. A listed rule also describes current code, but it belongs to the base project's business: do not extend it or build on it. A <NEW Display> feature that touches it changes or retires it through the spec flow, and its row is removed then.

   | Rules | What is from the base project | For <NEW Display> |
   | --- | --- | --- |
   ```
   One row per group of rules with the same reason (OLD products or business lines, OLD income or pricing rules, OLD website content, OLD client decisions, pointers into removed specs, UI or tooling history). No hit: keep the legend and write `| none | - | - |`.
5. Guides: remove links to deleted plans; where plan ids are only naming examples, add one line saying so.

**Check:** every kept spec has exactly one block; the encoding script prints nothing on a second run; the dead-reference scan finds only names explained in a legend. Commit, push, ask the user to open and merge the PR (the session cannot open PRs). Report per spec which rules were marked and why.

## Step 9. Comments that point to removed docs (API repo, after the docs PR is merged)

`git fetch origin main && git checkout -B main origin/main && git checkout -b chore/<NEW kebab>-comments`

1. Scan source comments with the Step 8 term list, the removed doc names and the plan-id patterns (comment syntax of each language).
2. Replace each hit with an exact-match edit (one occurrence per edit), keeping the meaning without the dead pointer.
3. Prove the change is comment-only: `git diff -U0 | grep -E '^[+-]' | grep -vE '^(\+\+\+|---)' | grep -vE '^[+-]\s*(///|//|\*|/\*|#)'` prints nothing.
4. If the repo has no code map for agents, write `docs/Code_Map.md` from a scan: layers and module folders, the shared library's base types, and for each kind of class (aggregate, event, handler, service, controller, configuration, background job, seed) one existing file to copy, found with `git grep -l` on the base type or attribute. Verify every path exists before committing.

**Check:** step 3 prints nothing; every path in the code map exists. Commit, push, ask the user to merge.

## Step 10. Hand-over

One short message, a table per repo:
- done: counts of renamed files, profile rows applied, docs deleted, specs marked;
- **not** verified: build, lint, UI screens;
- the user's part: merge the PRs in order (setup, docs, comments), re-run the base's secret setup (new unique ids), create the local database, provide the items of Step 5.7, confirm the *Environment* rows before the first production deploy.

Next (not part of this skill): new development through the repo's own flow (spec -> plan -> approval -> code).
