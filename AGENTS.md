# AGENTS.md

Canonical instructions for anyone — human or AI — working in this repository. `CLAUDE.md` and
`.github/copilot-instructions.md` are thin pointers back here; edit **this** file, not those.

## What this is

Expression Builder is a .NET library for building LINQ expressions (filters) at run time: from code, from strings,
from saved JSON/protobuf, and now from a browser. A `Filter<T>` is turned into an `Expression<Func<T, bool>>` for
`IEnumerable` and `IQueryable` (EF Core). It ships as a set of NuGet packages: an API, a core, one small package per
operation, a "main" package that references them all, and a web package (Razor/MVC filter builder with a TypeScript
front end). Public docs live in the root `README.md` and under `docs/`.

## Where things are

| What                                      | Where                                                                  |
| ----------------------------------------- | ---------------------------------------------------------------------- |
| Interfaces and contracts for operations   | `src/ExpressionBuilder.Api/`                                           |
| `Filter<T>`, serialisation, registries    | `src/ExpressionBuilder.Core/`                                          |
| One package per operation                 | `src/Operations/<Name>/`                                               |
| The package that references everything    | `src/ExpressionBuilder/` (the `Operation` class lives here)            |
| Build-time generator for fluent methods   | `src/ExpressionBuilder.Generators/`                                    |
| Web filter builder (Razor, MVC, TS)       | `src/ExpressionBuilder.Web/` (`client/` is the TypeScript)             |
| Tests                                     | `tests/` — mirrors `src/`; `tests/Operations/<Name>/` per operation    |
| Shared test models and helpers            | `tests/ExpressionBuilder.TestBase/`                                    |
| Runnable sample of the web filter builder | `samples/ExpressionBuilder.Web.Sample/`                                |
| Benchmarks (not packed, not tests)        | `tools/ExpressionBuilder.Benchmarks/` — see its README                 |
| AI-facing skills/instructions/memory      | `ai/` — see [ai/README.md](ai/README.md)                               |
| Human docs                                | `docs/` — see the docs index below                                     |

Build settings are shared, not repeated: `Directory.Build.props` (language, version), `src/Directory.Build.props`
(package metadata, `AssemblyName` = `TopMarksDevelopment.<project name>`), `src/Operations/Directory.Build.props` (an
operation's title, description and references come from its project name), `tests/Directory.Build.props` and
`Directory.Packages.props` (every package version). A new operation's `.csproj` is normally one line.

## Code conventions

- **Comments explain why, not what.** Public API gets XML docs (the build turns missing ones into warnings, and
  warnings into errors in CI). Don't narrate a fix next to the code; that is for the commit message.
- **Formatting is owned by CSharpier** (`.csharpierrc`, version pinned in `.config/dotnet-tools.json`): run
  `dotnet csharpier format src tests samples tools` rather than hand-formatting. TypeScript follows the same 4-space style.
- **Don't copy code between operations.** The fluent extension methods (`filter.Equal(...)`) are generated: an
  operation is marked `[GenerateFilterExtensions(shape)]` and the templates under
  `src/ExpressionBuilder.Generators/Templates/` do the rest. Change a template, not 25 files.
- **Operations are found by name at run time** (`OperationRegistry`), including ones nothing has used yet — saved
  filters depend on it. Don't look types up with `AppDomain` scans of your own; use the registries.
- **A posted or stored filter is untrusted input.** The web builder's JSON names properties, operations and text only,
  never .NET types; it is validated against the schema. Don't add a path that lets a browser name a type or a method.
- **Don't break the public API silently.** The public surface of the packages is compared before and after a refactor
  (see [docs/standards/testing.md](docs/standards/testing.md)); additions go in the changelog, removals are breaking
  changes.
- **Never commit secrets** (NuGet API keys, tokens, `.env` files). Release publishing uses GitHub Actions secrets.

## Git conventions

- **Branches:** `type/scope-description`, kebab-case — `feat/`, `fix/`, `refactor/`, `chore/`, `test/`, `docs/`, `perf/`.
- **Commits:** Conventional Commits — `type(scope): imperative subject`, e.g. `fix(core): stop IFilterable.CloseGroup recursing`.
  Scope is a package or area (`core`, `api`, `web`, `generators`, `ops`, `ci`).
- **PRs:** squash-merge to `main`. Use `.github/pull_request_template.md`. Full detail:
  [docs/standards/git.md](docs/standards/git.md).

## Testing

Every behaviour change ships with tests at the lowest layer that proves it; every bug fix gets a regression test that
fails without the fix. Run before hand-off:

```powershell
dotnet build ExpressionBuilder.sln
dotnet test ExpressionBuilder.sln
cd src/ExpressionBuilder.Web/client; npm ci; npm run check   # only when the web client changed
dotnet test tests/ExpressionBuilder.Web.E2E                  # only when the builder's markup, script or packaging changed (needs browsers)
dotnet csharpier check src tests samples tools
```

See [docs/standards/testing.md](docs/standards/testing.md).

## Docs index

| Document                                                   | Purpose                                                    |
| ---------------------------------------------------------- | ---------------------------------------------------------- |
| `README.md`                                                | What the library is, how to install and use it             |
| `ChangeLog.md`                                             | Every released change, per package                         |
| `CONTRIBUTING.md`                                          | Local setup and the contribution workflow                  |
| `SECURITY.md`                                              | Reporting a vulnerability, and the trust boundaries        |
| `docs/architecture/overview.md`                            | How the packages fit together and why                      |
| `docs/adding-an-operation.md`                              | Adding an operation (and its tests) step by step           |
| `docs/web-filter-builder.md`                               | The Razor/MVC filter builder: guide and reference          |
| `docs/standards/{git,code-style,testing,docs-conventions}.md` | The mechanics behind the conventions above              |

## Engineering principles

Git/DevOps, SOLID, risk-based testing, DRY and KISS, applied concretely to this repo:
[ai/instructions/engineering-principles.md](ai/instructions/engineering-principles.md). **Standing instruction,
effective from the first message of every session:** call out when a request, an existing pattern, or something the
user proposes goes against one of these — say so plainly rather than complying silently, and give a real
recommendation rather than a neutral tradeoff list when asked for an opinion.

## AI-specific context

See [ai/README.md](ai/README.md) for skills, task instructions, and shared agent memory. Memory files never contain
secrets — see the note in that folder.
