# Docs index

Project-specific pointers read by `CLAUDE.md`. Edit this file per project; `CLAUDE.md` itself stays generic.

## Source of truth
- [lead-to-client-payment-system-final.md](lead-to-client-payment-system-final.md) - final business document. Wins over every other doc.

## Development guide
- [developer-docs-module-wise/](developer-docs-module-wise/) - one file per business area. Input for specs and plans; never overrides the source of truth.

## For developers
- [Developer_Guide.md](Developer_Guide.md) - how to work with the AI agent from start to merge: flow, branches, checklist, and what a human must review.

## Working docs
- `specs/<module>.md` - the one spec of each module (`iam`, `core`, `platform`, ...). Every feature is a rule group inside it.
- `plans/<module>/` - the plans of each module, `YYYY-MM-DD-NN-<slug>.md`, referred to as `<module>#NN`.
- `Backlog.md` - the one backlog of the whole project.
- `templates/` - templates for a module spec and a plan.

## Legacy (read-only history)
- `plans/0001-*.md` ... `plans/0016-*.md` at the root of `plans/` - plans with the old global numbering. No new numbers are added there.
