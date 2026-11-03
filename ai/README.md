# /ai

Shared context for AI agents working in this repository, kept separate from `docs/` because it's written for machine
consumption first (loaded into agent context, referenced by tooling) rather than for a person reading top-to-bottom.
Start at [AGENTS.md](../AGENTS.md) — that's the entry point every agent should read first; this folder holds the
supporting material it points to.

```text
ai/
├── README.md          — this file
├── skills/            — reusable procedures (adding an operation, cutting a release)
├── instructions/      — task-specific instruction sets that don't fit the "skill" shape
│                        (next-steps.md: the roadmap; next-session.md: the prompt for the very next session)
└── memories/          — durable, shared agent memory — facts worth carrying between sessions
```

## Rules for this folder

- **Never store secrets here.** No NuGet API keys, tokens, passwords or local environment files. This folder is
  committed to git; treat it as public.
- **`memories/` is for durable, project-level facts** an agent would otherwise have to re-derive every session —
  architectural decisions, gotchas, "don't do X because Y happened" notes. It is not a scratchpad and not a log; keep
  entries current, remove or correct ones that go stale rather than layering corrections on top.
- **`skills/`** entries should be self-contained enough that an agent with no other context can follow them — name the
  files/commands involved explicitly rather than assuming prior conversation.
- If something belongs to a human audience (user guidance, an architecture decision or a contributor runbook) it belongs
  in `docs/`, not here.

Populate this folder as real, recurring needs show up rather than pre-writing speculative content. Keep the current
standing behaviour in `memories/engineering-principles.md` aligned with its detailed instruction.
