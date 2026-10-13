# Engineering principles — house rules

Git/DevOps, SOLID, risk-based testing and quality assurance, DRY and KISS, applied concretely to this repo — not
textbook definitions. For the mechanics (how CSharpier is configured, the exact branch rules, the test fixture pattern),
see `docs/standards/{git,code-style,testing}.md` — this file is the judgment layer above those: _when_ and _why_, and
how the same principle applies to the documentation in `docs/` and `ai/`, not just to code.

## Standing instruction: call out deviations

**Whenever a request, an existing pattern, or something the user proposes conflicts with a principle below, say so
explicitly and give a real recommendation** — don't comply silently, and don't answer "what do you think?" with a neutral
list of tradeoffs when a straight opinion is asked for. This is a standing behaviour, active from the first message of
every session — not something that only kicks in once a skill is invoked or a plan is being worked. See
`ai/memories/engineering-principles.md`.

## Git & DevOps

- **Prefer trunk-based development** — small, short-lived branches merged to `main` frequently — over long-lived feature
  branches. Mechanics: `docs/standards/git.md`.
- **Review continuously, not in one deferred batch.** The longer code sits unreviewed on a branch, the more expensive both
  the review and any rework become.
- **Publishing is a separate, deliberate step from merging.** Packages are published by a GitHub release (the "Nuget Push
  CI" workflow); merging to `main` is safe to do more often than shipping is.
- **Make authorship traceable.** Conventional Commit subjects explain the change, bodies explain non-obvious reasons, and
  commits created with AI assistance include the appropriate `Co-Authored-By` trailer.
- **Mechanical moves and behaviour changes go in separate commits.** A rename of 400 files and a bug fix in one commit
  can't be reviewed; `git mv` first, then change.

## SOLID

Use SOLID as pressure toward understandable boundaries, not as a reason to create an interface for every function.

- **Single Responsibility** — building an expression, finding an operation by name, (de)serialising a filter, and
  presenting a filter in a browser change for different reasons. They are in different packages and, within `Core`,
  different folders (`Registry/`, `Serialization/`).
- **Open/Closed** — a new operation is a new package that implements `IOperation`; nothing in `Core` changes. The web
  builder's operation list grows the same way (`OperationCatalog.Register`).
- **Liskov / Interface Segregation** — `IOperation` is deliberately small (name, defaults, build, validate). Presentation
  concerns (labels, how many values, which kinds of property) are **not** added to it; they live in the web package's
  `OperationDescriptor`, so operations stay usable without a UI.
- **Dependency Inversion** — operations depend on `Api`, never on `Core`. The web package depends on `Core` and the main
  package, never the reverse.
- **Docs follow the same rule** — user guidance, contributor setup, security policy and architecture each have a distinct
  audience and reason to change.

## Testing and quality assurance

Tests ship with behaviour in the same pull request, and the verification is clear before implementation. Write tests first
where doing so clarifies the contract (parsing, validation, "what should this reject"); for rendering and wiring, a focused
implementation followed immediately by a test is fine.

Coverage is a diagnostic, not the target. Prioritise the branches where a defect would be costly:

- anything that turns **untrusted input** into an expression (the web builder's compiler, JSON/protobuf deserialisation);
- **operation behaviour across property types** (nullable value types, collections, navigation chains) — the matrix test in
  `FilterBuilderModelTests` runs every operation the builder offers against every kind it is offered for;
- **things that depend on what is loaded at run time** (operation discovery): these need a fresh process to be tested
  honestly — see `tests/ExpressionBuilder.Tests.Probe`.

A bug fix gets a regression test that fails without the fix. If it is hard to write one, that is information about the
design, not a reason to skip it.

## DRY

- **Don't duplicate logic.** The fluent methods of 25 operations were once 25 × 6 hand-copied files; they are templates now.
  Package metadata is declared once in `Directory.Build.props`, package versions once in `Directory.Packages.props`.
- **Don't duplicate decisions.** The same rule stated in two places (which operations suit which kind of property; the shape
  of the posted JSON) drifts silently. The web builder's server and script share one schema, and a fixture
  (`client/test/fixtures/person-schema.json`) makes drift a failing test.
- **Docs have one owner per decision.** Link to the architecture doc rather than copying its explanation.
- **But don't force a shared abstraction on things that merely look alike.** The 18 generator templates are separate files
  on purpose: the overload sets differ in small, load-bearing ways (see the quirks noted in
  `docs/architecture/overview.md`), and one parameterised template would have changed behaviour.

## KISS

- **Don't build for hypothetical scale.** Add an abstraction when the second use appears, not before.
- **Don't introduce tooling ahead of the pain that justifies it.** The web client uses `tsc`, `esbuild` and node's built-in
  test runner (with jsdom) — no framework, no bundler configuration, no test framework beyond that.
- **Prefer the platform.** `<template>`, `hidden`, native `<select>`/`<input type=date>`, CSS custom properties and
  `prefers-color-scheme` rather than a component library.
- **Keep documentation proportional.** Record durable architecture and user behaviour in the repository; keep transient
  coordination in issues and pull requests.
