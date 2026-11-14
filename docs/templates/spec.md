# Spec: <module> - <display name>

- **Module:** `<module>`
- **Status:** Draft | Approved (rule ranges may differ: "R1-R20 Approved; R21-R30 Draft")
- **Last updated:** YYYY-MM-DD
- **Source docs:** links to the source-of-truth sections (see `docs/README.md`)
- **Plans:** links to `../plans/<module>/*.md`, oldest first, with status

> One spec per module. It says WHAT the module does and WHY, never implementation steps (those live in plans).
> It always describes the current behaviour: edit it in place. Each rule exists here exactly once; other sections refer to it by id.

## 1. Purpose and scope
What the module is responsible for, and explicitly what it is not (name the module that owns anything left out).

## 2. Concepts and glossary
Domain terms, roles, statuses and lifecycles (state diagram when useful). Defined once here; rules refer to them.

## 3. Business rules
Numbered, testable statements, one serial for the whole module: `R1`, `R2`, ... Group them under bold feature headings
(**Login**, **Password reset**). A new feature continues the numbering; ids are never reused or renumbered.

## 4. Data
Tables/columns the module owns, constraints, money precision and rounding when relevant.

## 5. API contract
Method + route, roles/policy, request/response, error cases (status + code).

## 6. User interface
Screens and what the user sees in each state (loading, empty, error, success). "None" with the reason if a feature has no UI.

## 7. Events
Events raised (past tense), who reacts, what each reaction does.

## 8. Dependencies
Other modules and ports this module needs.

## 9. Acceptance criteria
`AC1`, `AC2`, ... (one serial for the module), each checkable by a request, a screen action or a query, naming the rules it proves.

## 10. Decisions
| # | Decision |
|---|---|
| D1 | Who decided, when, and why. |

## 11. Open questions
`Q1`, ... unknown business rules, with who must answer. Never filled in by guessing.
