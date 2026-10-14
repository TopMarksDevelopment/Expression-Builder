# Next session: performance (section 4 of next-steps.md)

Paste the prompt below to start. Everything that came before section 4 is done; [next-steps.md](next-steps.md) has the
history in its "Progress" section and the rest of the roadmap. This file only says where to start and what to watch for.

## Where things stand

- Branch `refactor/repo-restructure-and-filter-builder`, version `1.0.0-rc.1`, `net10.0` only, SDK 10.0.402+ (`global.json`),
  nothing released or pushed from this branch.
- Done: .NET 10, package and tooling bumps, custom operations and manipulators, restyling by templates, accessibility fixes,
  browser tests, Entity Framework Core tests, and values sent to the database as parameters.
- `dotnet test ExpressionBuilder.sln`: 1573 tests. `npm run check` in the client folder: 108. `dotnet test tests/ExpressionBuilder.Web.E2E`
  (not in the solution; needs browsers): 66, in Chromium, Firefox and WebKit.

## The prompt

```text
Read AGENTS.md and ai/instructions/engineering-principles.md first (standing instruction: call out anything in my requests that
conflicts with them, and give a real recommendation when I ask for an opinion). Then read ai/instructions/next-session.md and
ai/instructions/next-steps.md (its "Progress" section says what is done), docs/architecture/overview.md (especially "Values are
variables, not constants"), docs/web-filter-builder.md, docs/standards/testing.md and SECURITY.md.

We are on branch refactor/repo-restructure-and-filter-builder, version 1.0.0-rc.1, net10.0 only, nothing released. Sections 1, 2
and 3 are done, and so is the "constants" item of section 4 (ToExpression() keeps values in variables so EF sends parameters).

Do section 4 (performance), measuring first:
- Say what you will do and what "done" means before you start.
- A BenchmarkDotNet project under tools/ (not packed, not in the solution's test run), covering at least: building a Filter<T> and
  ToExpression() for small / medium / large filters, and the cost of the parameterising pass on its own (Extensions/ValueParameters.cs
  walks the finished tree once per ToExpression call); compiled filter vs hand-written LINQ; deserialising JSON and protobuf;
  OperationRegistry lookups, cold and warm; the web FilterCompiler (posted JSON -> Filter) and FilterSchema.For<T> (reflection per
  request: should it be cached, and keyed by type and options?); the Razor view with a large schema. Allocations as well as time.
- Measure what parameters save: the same N different values through EF with the old behaviour (InlineValues()) and the new, in time
  and in query-cache entries. Use real numbers; say what hardware and .NET version.
- Cache only what is safe to cache, and prove each cache with a benchmark. A filter schema depends on the options and on the
  operation/manipulator catalogs, which can change: say how a cache is invalidated, or why it cannot go stale.
- Only if the results are good, add a short, honest "Performance" section to the README (method and numbers) and draft an
  announcement. Do not claim anything the benchmarks do not show. Stop and ask before writing the README section.

Working rules: keep mechanical changes in their own commits; tests with every behaviour change; keep the solution warning-free;
run the AGENTS.md checks before handing back (note `dotnet csharpier check src tests samples tools`, and the browser tests if you
change the builder's markup, script or packaging). Commit in logical slices when I have asked you to, and never push. Stop and ask
where a decision is mine. Finish with: what changed, what you verified and how, what you could not verify, and what you suggest next.
```

## Open items worth raising (small; none block section 4)

- **A human screen-reader pass** (NVDA or JAWS on Windows, VoiceOver on macOS and iOS) and keyboard use on a phone. The browser
  tests check names, roles, focus and axe, which is a proxy.
- **The `browsers` CI job has not run yet.** Watch the first run on the pull request: browser installation with `--with-deps`, and the
  consumer apps restoring from the local feed.
- **Class names without copying the view.** Restyling needs a copy of `Default.cshtml` (for structure changes, such as a label after
  its box, that is the right tool). For a class-only change such as Bootstrap that is a lot of file; a `FilterBuilderOptions` map of
  class names would avoid the copy. Not built, because it is easy to add and awkward to remove: build it when a second user needs it.
- **A hook for an "empty" class.** A design that floats its labels with a class toggled by script (the `Empty` class in some form
  libraries) has no hook; `placeholder=" "` with `:placeholder-shown` works without one.
- **xunit v3** (section 6) and the custom-operation limits: an operation can only use the existing value shapes and property kinds.
- **`Directory.Build.props` and other root props files are not formatted** (the CSharpier check covers `src tests samples tools`).
  `dotnet csharpier check .` shows it; decide whether to widen the check.

## Gotchas that cost time before

- Working files are CRLF but `client/src` is LF: multi-line `sed`/`perl` patterns miss CRLF files. Use the editor tool, or a small
  node script that normalises line endings first. Large bash heredocs full of quotes can fail to parse and write nothing.
- `tests/ExpressionBuilder.Web.E2E` is not in the solution on purpose. Run it with `dotnet test tests/ExpressionBuilder.Web.E2E`
  (`EB_E2E_BROWSERS=chromium` for speed).
- In a file that has `using Microsoft.Playwright;` after a file-scoped namespace, `Program` means Playwright's: write `global::Program`.
- A filter's expression now holds `ValueHolder<T>` reads. Code (or a test) that looks for `ConstantExpression`s should call
  `InlineValues()` first.
