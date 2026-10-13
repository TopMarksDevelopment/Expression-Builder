# Code style

Formatting is owned by tooling, not by hand — don't hand-format against these tools' output, run them.

## Comments

Comments explain **why**, not **what** — well-named identifiers already say what the code does; a comment earns its place by
carrying information the code itself can't, such as a non-obvious constraint, an invariant, or the reason a workaround exists.
Default to no comment; add one only when its absence would leave a future reader confused. This applies to C#, TypeScript and
CSS alike.

- **Don't narrate a fix at the point of the diff.** What was broken and how it was resolved belongs in the commit or pull-request
  description. Git history carries the change narrative; comments carry durable constraints.
- **Don't reference the current task, issue number or PR** in a comment — `git blame` is the authoritative record.
- **A short why-comment still earns its place** when a line would otherwise look deletable or wrong to the next person who touches
  it. Keep it to the invariant, one or two lines.
- **XML docs describe public contracts.** Every public member has them (the build warns, and CI treats warnings as errors).
  `<inheritdoc />` is right for an interface implementation; write a real summary for anything else. Say what it does for a caller,
  not how.
- **Wrong comments are worse than no comments.**

## C#

- **CSharpier owns formatting** (`.csharpierrc`, pinned in `.config/dotnet-tools.json`):
  `dotnet csharpier src tests samples tools` to write, `dotnet csharpier --check src tests samples tools` to verify. The pinned version is the
  one the code was formatted with; bump it deliberately, in its own commit, because a new version reformats existing files.
  The pinned version is a .NET 8 tool and only the .NET 10 runtime is required by the repo, so run it with `DOTNET_ROLL_FORWARD=Major`
  (set in the CI formatting job) until the pin is bumped.
- **Nullable reference types are on.** Don't suppress with `!` unless the line below proves it; say why in a comment if it isn't
  obvious.
- **File-scoped namespaces**, one top-level type per file, file named after the type (`Filter_TClass.cs` for `Filter<TClass>`).
- **Prefer small, pure functions** for anything that turns input into a decision (parsing a posted value, picking an operation).
  They are the cheapest things to test.
- **Don't swallow exceptions.** If one must be caught, catch the narrowest type and say in a comment why it is safe to ignore.
- **Reflection and run-time type lookup go through the registries** (`OperationRegistry`, `ManipulatorRegistry`), not ad hoc
  `AppDomain` scans.
- **Culture:** parse and format with `CultureInfo.InvariantCulture` anything that is stored, posted or compared.

## Generated code

`src/ExpressionBuilder.Generators/Templates/` holds the fluent-method templates. They are C# sources with `__Op__` as the
operation's name and are **not** formatted by CSharpier. Edit a template, rebuild an operation project and confirm the public API
is what you intend (`tools/ExpressionBuilder.ApiSnapshot`).

## TypeScript (web client)

- **Strict, with `noUncheckedIndexedAccess`.** Avoid `any`; use `unknown` at trust boundaries and narrow it (`parseDocument` is the
  model).
- **Pure logic is separate from the DOM.** `model.ts` has no DOM and is tested without one; `builder.ts` binds it to markup.
- **Find parts by `data-eb-*` attributes, never by class or position**, and look up a group's own parts before adding its children
  (a nested group has parts with the same attribute).
- **No framework.** Native elements, `<template>`, `hidden`, CSS custom properties.
- **The bundle is generated.** Don't edit `wwwroot/js/filter-builder.js`; change `client/src` and run `npm run build`.

## CSS

Scoped to `.eb`, themed with custom properties, light and dark through `prefers-color-scheme` (and `data-theme` to force one).
Visible focus styles, no information conveyed by colour alone, layout works down to a 360px viewport.

## Secrets

Never commit NuGet API keys, tokens, `.env` files or private feed configuration. CI secrets belong in GitHub Actions secrets.
Tests use obvious dummy values. If a secret is exposed, revoke it immediately and document only the remediation, never the value.
