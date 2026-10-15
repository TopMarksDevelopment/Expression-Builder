# Next session: the report builder design (section 5 of next-steps.md)

Paste the prompt below to start. Sections 1 to 4 of [next-steps.md](next-steps.md) are done (its "Progress" section has the history);
this file says where to start, what you need to do yourself, and what to watch for.

## Where things stand

- Branch `refactor/repo-restructure-and-filter-builder`, version `1.0.0-rc.1`, `net10.0` only, SDK 10.0.402+ (`global.json`). Nothing is
  released. The branch has been pushed; whether it has a pull request open is yours to check.
- Done: .NET 10, package and tooling bumps, custom operations and manipulators, restyling by templates, accessibility fixes, browser
  tests, Entity Framework Core tests, values sent to the database as parameters, **and performance (section 4)**: the benchmark project
  (`tools/ExpressionBuilder.Benchmarks`, with `RESULTS.md`), the README "Performance" section, a draft announcement
  ([announcement-draft.md](announcement-draft.md)), two small speed-ups, and the fix for saved filters whose values are an enum or class
  of the application.
- `dotnet test ExpressionBuilder.sln`: 1583 tests. `npm run check` in the client folder: 108. `dotnet test tests/ExpressionBuilder.Web.E2E`
  (not in the solution; needs browsers): 66 across Chromium, Firefox and WebKit; 26 in Chromium alone, which passed when this was written.

## What you need to do yourself

1. **Check the pull request and its first CI run.** The `browsers` job has never run: watch browser installation (`--with-deps`) and the
   consumer apps restoring from the local feed. Squash-merge only when it is green (see `docs/standards/git.md`).
2. **A human screen-reader pass** (NVDA or JAWS on Windows, VoiceOver on macOS and iOS) and keyboard use on a phone. The browser tests
   check names, roles, focus and axe, which is a proxy.
3. **Check the announcement and the README "Performance" numbers on your own machine before you publish anything.** They are from one
   machine on 2026-10-15. `cd tools/ExpressionBuilder.Benchmarks; dotnet run -c Release -- ef` takes a few minutes; the whole suite
   (`-- --filter "*"`) about 25. Close other programs first. Delete `announcement-draft.md` once it has been used.
4. **Decide, when you are ready (none block the next session):**
   - whether to cache the filter schema. The recommendation is no; what it would take is in `RESULTS.md` ("Not cached");
   - whether the CSharpier check should cover the root `*.props` files (`dotnet csharpier check .` shows them);
   - whether to build a `FilterBuilderOptions` map of class names (build it when a second user needs it);
   - when to publish `1.0.0-rc.1` (publishing is a separate step from merging: `ai/skills/release.md`).

## The prompt

```text
Read AGENTS.md and ai/instructions/engineering-principles.md first (standing instruction: call out anything in my requests that
conflicts with them, and give a real recommendation when I ask for an opinion). Then read ai/instructions/next-session.md and
ai/instructions/next-steps.md (its "Progress" section says what is done, and section 5 is this session's job),
docs/architecture/overview.md, docs/web-filter-builder.md, docs/standards/testing.md, SECURITY.md and
tools/ExpressionBuilder.Benchmarks/RESULTS.md (the costs you will want to budget against).

We are on branch refactor/repo-restructure-and-filter-builder (or a branch from it if it has been merged), version 1.0.0-rc.1, net10.0
only, nothing released. Sections 1 to 4 are done.

Do section 5, and only the design: a report builder, where the user picks what to report on (one of several classes), builds the
filter for it, chooses columns and sorting, and gets results. Do not write the feature yet.
- Say what you will do and what "done" means before you start.
- Write the design to docs/architecture/report-builder.md, following the constraints and suggested direction in section 5 of
  next-steps.md: roots are an allow-list chosen by a stable key and never a .NET type name from the browser; a versioned posted
  document; execution by projection (compare an expression-tree projection, a dictionary/object[] shape and a registered typed result,
  and recommend one); paging, cost limits; and security first (column and row-level authorisation, never filtering or sorting on a
  column the user cannot see, the app's own mandatory filters applied before the user's, result-size limits). Say what changes in
  SECURITY.md.
- For each design choice, give a recommendation and the main alternative, not a neutral list. Where the choice is mine, mark it
  DECISION and stop.
- Break the delivery into slices that each stand alone (multi-root schema and non-generic build; root picker; columns and sort;
  execution and export in the sample app; grouping only if still wanted), with the tests each needs. Include the "save a report as a
  built-in report" idea at the end of section 5 and what storage it needs.
- Use the benchmark project to cost anything on a hot path (a non-generic build, a projection per request) before recommending it; do
  not claim a cost you have not measured.
- Then stop and ask me to review the document. No code, and no commits until I have.

Working rules: keep mechanical changes in their own commits; tests with every behaviour change; keep the solution warning-free; run
the AGENTS.md checks before handing back. Commit in logical slices only when I have asked you to, and push only when I say so. Finish
with: what changed, what you verified and how, what you could not verify, and what you suggest next.
```

## Open items worth raising (small; none block the next session)

- **A hook for an "empty" class.** A design that floats its labels with a class toggled by script has no hook; `placeholder=" "` with
  `:placeholder-shown` works without one.
- **xunit v3** (section 6) and the custom-operation limits: an operation can only use the existing value shapes and property kinds.
- **Still rough in the library** (section 6 and `docs/architecture/overview.md`): `Filter<T>.CloseGroup` swallows exceptions;
  `IsNotNullable` uses `NullableAttribute` on the type; no-value operations use `object`/`dynamic` as the generic type on some receivers;
  library-format JSON still calls `Type.GetType` on a name in the document, so it stays "trusted storage only".
- **Not done on purpose in section 4:** a schema cache; making the registry's 2 µs miss cheaper; speeding up the parameterising pass
  (0.7 / 5 / 30 µs); the JSON reader, which parses each item twice (about 3 µs per condition). All are small beside what a request does.

## Gotchas that cost time before

- Working files are CRLF but `client/src` is LF: multi-line `sed`/`perl` patterns miss CRLF files. Use the editor tool, or a small
  node script that normalises line endings first. Large bash heredocs full of quotes can fail to parse and write nothing.
- `tests/ExpressionBuilder.Web.E2E` is not in the solution on purpose. Run it with `dotnet test tests/ExpressionBuilder.Web.E2E`
  (`EB_E2E_BROWSERS=chromium` for speed).
- In a file that has `using Microsoft.Playwright;` after a file-scoped namespace, `Program` means Playwright's: write `global::Program`.
- A filter's expression holds `ValueHolder<T>` reads. Code (or a test) that looks for `ConstantExpression`s should call `InlineValues()`
  first. Filters used on lists (`list.Where(filter)`, `ToFilterable()`) are compiled from constants and never see the holders.
- **Benchmarks:** BenchmarkDotNet builds the project again from the files on disk for every run, so do not switch branches, stash or
  build while one is running, and do not run anything else heavy at the same time: the numbers are for the whole machine. To compare a
  change, stash it, run the benchmark, restore it, and run again, in the same session.
- BenchmarkDotNet's `--filter` takes several patterns, but the registry class names overlap (`*RegistryBenchmarks*` also matches the
  cold one); name the method too.
- The EF experiment needs a warm-up of a few queries of a different value, or the first timed query carries about 47 ms of start-up.
- A saved filter's `propertyType` for an application type is `Namespace.Type, AssemblyName` (no version). Older saves of such types could
  never be read, so there is nothing to migrate.
