# Documentation conventions

## Who a document is for

Each document has one audience and one reason to change:

| Folder / file        | Audience                                       |
| -------------------- | ---------------------------------------------- |
| `README.md`          | Someone deciding whether to use the library    |
| `docs/*.md`          | People using a feature in depth                |
| `docs/architecture/` | People changing the library                    |
| `docs/standards/`    | Contributors (humans and agents)               |
| `ChangeLog.md`       | People upgrading                               |
| `ai/`                | Agents (machine-first, committed, public)      |

Architecture and user documentation are updated in the same pull request as the behaviour they describe.

## Writing style

- Concise British English and sentence-case headings.
- Address the reader directly and use active voice.
- Fenced blocks with a language identifier for commands and code.
- Link to files and stable headings, not line numbers or absolute local paths.
- Write for the reader who wasn't in the room: name files, commands and decisions explicitly.
- Prefer rewriting a document in place over layering contradictory corrections on top.
- Link rather than restate. A decision has one home.

## The changelog

`ChangeLog.md` follows the structure in its header comment: newest first, grouped by package, with the types Added, Changed,
Deprecated, Removed, Fixed, Security and Breaking Changes. An entry says what a user of the package notices, not how the code
changed. Unreleased work goes under `Unreleased` until it is released.
