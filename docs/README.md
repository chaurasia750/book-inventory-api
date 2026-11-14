# Docs index

Project-specific pointers read by `CLAUDE.md`. Edit this file per project; `CLAUDE.md` itself stays generic.

## Source of truth
- [bussiness/BRD-Book-Inventory-Phase1.md](bussiness/BRD-Book-Inventory-Phase1.md) - Book Inventory, Billing & Ledger System, Phase 1 business requirements (draft for client review). It wins over every other doc once the owner approves it.

## Development guide
- none in the repository yet beyond the developer guide below.

## For developers
- [Developer_Guide.md](Developer_Guide.md) - how to work with the AI agent from start to merge: flow, branches, checklist, and what a human must review.
- [Release_Checklist.md](Release_Checklist.md) - what to confirm before a release.

## Working docs
- `specs/<module>.md` - the one spec of each module (`iam`, `core`, `platform`, ...). Every feature is a rule group inside it. The three specs present were carried over from the base project (HautInHousing) and describe the code as it is.
- `plans/<module>/` - the plans of each module, `YYYY-MM-DD-NN-<slug>.md`, referred to as `<module>#NN`. No plans exist yet.
- `Backlog.md` - the one backlog of the whole project.
- `templates/` - templates for a module spec and a plan.
