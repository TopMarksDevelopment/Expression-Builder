# Copilot instructions

The canonical instructions for this repository live in [`AGENTS.md`](../AGENTS.md) at the repo root. Read that file
first — this one exists only so GitHub Copilot picks up project context automatically.

Project conventions, code style, git workflow and testing rules all live in `AGENTS.md` and the docs it links to under
`docs/`. Do not duplicate them here.

Everything below is Copilot-specific; project/code guidance belongs in `AGENTS.md`, not here.

- Skills, task instructions and shared agent memory relevant to this repo live under [ai/](../ai/README.md).
- Unlike this file, `AGENTS.md`'s own pointers into `ai/` aren't auto-loaded — GitHub Copilot specifically should still
  treat [AGENTS.md](../AGENTS.md)'s "Engineering principles" section (and the file it points to) as required reading
  every session, not just when a skill is invoked.
