# Next steps: a working prompt and to-do list

Written at the end of the restructure / web filter builder session so work can continue elsewhere. It is a to-do list to work
through over several sessions, not one task. Tick items off here (and delete the file when it is done).

## Progress

- **Section 1 (.NET 10): done.** `net10.0` only; CI, docs and changelog updated. Found and fixed on the way: C# 14 binds
  `array.Contains(x)` in an expression lambda to the span overload (tests now use `Enumerable.Contains`), and the main package's
  `build/*.props` was never committed (`.gitignore` ignores `build/`; CI now runs `dotnet pack`).
- **Section 2 (packages): done.** EF Core, Sqlite and Mvc.Testing to 10.0.13; CSharpier to 1.3.0 (commands are now
  `dotnet csharpier check|format`; it also formats `.csproj`/`.props` XML; reformatted in its own commit); the other NuGet
  packages; the client's npm packages (TypeScript 7, jsdom 30, Node 24 with `@types/node` 24 and in CI); the GitHub Actions
  (majors, pinned by commit SHA); the audits (no vulnerabilities in NuGet or npm). `global.json` now requires SDK 10.0.402+
  (Roslyn 5.9), which is what lets the generator reference `Microsoft.CodeAnalysis.CSharp` 5.9.0. xunit v2 is flagged "Legacy"
  on NuGet; migrating to xunit.v3 is in section 6.
- **Customisation (between sections 2 and 3): done.** Custom operations and manipulators are opt-in by description
  (`OperationCatalog` / `ManipulatorCatalog`, or `options.AddOperation` / `AddManipulator` for one builder), listed apart from the
  built-in ones, cannot take a built-in name, and any builder can leave things out (`ExcludeOperations` / `ExcludeManipulators`).
  The posted form of an adjustment is now `{ name, args[] }`. The script adds no classes of its own: everything it creates is
  cloned from a `<template>` in the view, and `data-eb-invalid-class` names the incomplete-condition classes.
- **Section 3 (prove it works for real): done, except a human screen-reader pass.** `tests/ExpressionBuilder.Web.E2E` (not in the
  solution; its own CI job `browsers`): Playwright in Chromium, Firefox and WebKit, axe (light and dark), keyboard-only flows, and
  apps built from the packed packages, published and run in Production under a strict CSP. `EfCoreTests` runs every offered
  operation through SQLite and compares with in-memory. Found and fixed: the list had `role="presentation"` children (connector
  now lives inside its item), focus was lost after removing a condition, group or value, the options toggle and error had no
  `aria-controls` / `role="alert"`, remove buttons were not numbered. `MapStaticAssets` and `UseStaticFiles` both serve the
  package's files once published; the only thing that broke a strict CSP was ASP.NET's own `asp-validation-summary` (an inline
  `style`), now documented. **Still to do by hand:** NVDA/JAWS and VoiceOver; keyboard use on a phone.
- **Values are SQL parameters (from section 4): done.** `ToExpression()` now keeps values in variables
  (`Extensions/ValueParameters.cs`), so EF sends parameters and one query-cache entry serves every value
  (`EfCoreTests.DifferentValuesShareACacheEntry`). `FilterExpressionExtensions.InlineValues()` restores constants; the
  expression-text tests use it. Measured: see Performance below.

- **Performance (section 4): done.**
  `tools/ExpressionBuilder.Benchmarks` (BenchmarkDotNet, plus `-- ef` for EF Core); numbers and method in its `RESULTS.md`.
  Parameters: 5-31 times quicker per query for different values and no growth of EF's query cache; the parameterising pass is
  0.7 / 5 / 30 µs for 3 / 20 / 100 conditions (13-17% of `ToExpression()`, 0.2-0.4% of compiling it). Changed: `list.Where(filter)`
  and `ToFilterable()` compile constants (making a delegate is 18-36% quicker than from variables); a schema works out the operations for a kind of
  property once (`FilterSchema.For` 129 -> 95 µs for a 126-field class). Deliberately not done: a schema cache (it is the gate for what a
  post may do, and 95 µs for the largest class is small), the registry's 2 µs miss, the pass itself. **Found, and fixed afterwards:** a saved filter
  (JSON or protobuf) whose values are of an enum or class from the application cannot be read back, because `propertyType` is saved
  without its assembly (`Type.GetType(name)` only finds corelib and Core types); now it says its assembly (`TypeNames.For`, tests in
  `SavedTypeTests`). The README has a "Performance" section, and `ai/instructions/announcement-draft.md` is a draft announcement.

- **Report builder (section 5): design reviewed and decided (2026-10-16); nothing built.** [docs/architecture/report-builder.md](../../docs/architecture/report-builder.md):
  recommendations with alternatives, the slices with their tests, saved reports, and the measurements (throwaway code in
  `tools/ExpressionBuilder.Benchmarks`: `-- report`, and the `Report*` / `NonGenericBuild*` classes). Decided: **D1** stay in
  `ExpressionBuilder.Web` (a second tag helper that renders the filter builder); **D2** it ships as **`1.0.0-rc.3`** (slices a to
  d), with `rc.2` being this branch; **D3** refuse a shared report that uses a column the runner cannot see; **D4** buttons to
  reorder columns (drag and drop is parked). The review also asked about totals, sums, custom tables, computed columns and charts:
  designed as a ladder in "Outputs beyond columns" (slices e1 to e3, g, h), and two further decisions: **D5** the order after
  `rc.3` is e1 totals, f saved reports, e2 measures, e3 grouping, g computed columns, h charts; **D6** charts are data mainly (the
  application draws), and a tiny built-in SVG renderer is to be considered when slice h is picked up.
- **Version and formatting (2026-10-16): done.** `Directory.Build.props` says `1.0.0-rc.2` (`1.0.0-rc.1` was published on
  2024-09-19). CSharpier covers the whole repository (`dotnet csharpier check .`); the commands in the docs, CI and the VS Code task
  say so.

## Parked

Ideas that were decided against for now, with what would bring them back. Good candidates for a GitHub Discussion, so they are
not lost in a file.

- **A `FilterBuilderOptions` map of class names**: settings looked up per class rather than passed to each builder. Build it
  when a second user needs it. The report builder (section 5, [design](../../docs/architecture/report-builder.md)) registers
  settings per class of its own (`AddRoot`, `Restrict<T>`), which is the likeliest second user; decide the two together.
- **Caching the filter schema.** Decided against: it is the gate for what a post may do, and it is 11-95 µs. What it would take is in
  `tools/ExpressionBuilder.Benchmarks/RESULTS.md` ("Not cached").
- **Drag and drop to reorder report columns** (decided 2026-10-16: the report builder ships with move up / move down buttons).
  Bring it back when users ask for it after the soak. It must sit **on top of** the buttons, never replace them: the platform has no
  keyboard or screen-reader equivalent for dragging, touch is poor, and the browser tests hold the builders to keyboard-only use and
  axe. It would need its own keyboard model and announcements (`aria-live`), and tests in all three browsers.

For the very next session, use [next-session.md](next-session.md): it has a prompt that is ready to paste, and the current state.
The prompt below is the original, covering every section (section 5's design is done; its slices are in
[the design](../../docs/architecture/report-builder.md)).

```text
Read AGENTS.md and ai/instructions/engineering-principles.md first (standing instruction: call out anything in my requests that
conflicts with them, and give a real recommendation when I ask for an opinion). Then read docs/architecture/overview.md,
docs/web-filter-builder.md and SECURITY.md. We are on (or branching from) the branch
refactor/repo-restructure-and-filter-builder, which restructured the repo, replaced the hand-copied fluent methods with a source
generator, added OperationRegistry, fixed several bugs and added the TopMarksDevelopment.ExpressionBuilder.Web package (Razor/MVC
filter builder with a TypeScript front end). The version is 1.0.0-rc.1 and nothing has been released from this branch.

Work through the to-do list below one section at a time. For each section: say what you will do and what "done" means before you
start; keep mechanical changes (moves, renames, version bumps) in their own commits; add or update tests with every behaviour
change (a bug fix gets a regression test that fails without the fix); keep the solution warning-free; run the checks in AGENTS.md
before handing back; and do not commit unless I ask. Stop and ask me where a decision is mine (marked DECISION).

## 1. Move to .NET 10 and drop .NET 8 (DONE - skip)
- net10.0 only
- Change TargetFramework in Directory.Build.props (and the analyzers/generator stay netstandard2.0). Update CI (setup-dotnet 10.x,
  matrix), global.json if you add one, the README "requires" line, CONTRIBUTING, docs/standards, the release skill and the changelog.
- Check everything on .NET 10: build with warnings as errors, all tests, the sample app, `dotnet pack` and inspect the packages
  (dependency groups, staticwebassets, no generator dependency), the probe test (fresh-process operation discovery), and the
  Roslyn generator on the new SDK (Microsoft.CodeAnalysis versions).
- Look for .NET 10 / C# 14 breakages and opportunities: span overload ambiguity in tests (we compare against List<int> for this
  reason), ASP.NET Core 10 changes that affect the tag helper / view component / static web assets, anything now obsolete.
  Do not adopt new language features just because they exist.

## 2. Bump packages (DONE - skip)
- Directory.Packages.props (xunit, Microsoft.NET.Test.Sdk, coverlet, EF Core and SQLite to the 10.x line, Mvc.Testing,
  protobuf-net, Roslyn, MetadataLoadContext), the client's package.json (typescript, esbuild, jsdom, @types/*), the pinned
  CSharpier (.config/dotnet-tools.json) and the GitHub Actions versions. Remember CSharpier is pinned to 0.29.2 because newer
  versions reformat the whole codebase: if bumping it, do that in its own commit and reformat everything in that commit only.
- Run `dotnet list package --outdated --vulnerable --include-transitive` and `npm audit`; fix or justify each finding.
- One commit per ecosystem (nuget, npm, actions, tools) so a regression is easy to bisect.

## 3. Prove the web builder works for real (DONE - skip; see Progress)
- Add a Playwright smoke test (new tests project, or run from the client folder) that opens the sample app, builds a filter by
  clicking, posts, and checks the results. Cover Chromium, Firefox and WebKit, including date and datetime-local inputs.
- Accessibility: run axe in those tests, then do a keyboard-only and screen-reader pass; fix what it finds (focus after add/remove,
  announcements, labels, contrast in both themes).
- Run builder-produced filters through EF Core (SQLite in memory is fine) in the web tests: collection Any, nested collections,
  text operations, nullable comparisons, dates. Check the generated SQL is sensible and that nothing falls back to client evaluation.
- Install the built .nupkgs into a brand new MVC app and a Razor Pages app from a local feed, and run them *published and not in
  Development* (static web assets), and with a strict Content-Security-Policy header. Fix anything that only works from the sample.

## 4. Performance: measure first, then optimise, then (if it is good) say so
- Add a BenchmarkDotNet project under tools/ or tests/ (not packed). Benchmark: building a Filter<T> and ToExpression() for small /
  medium / large filters; compiling vs hand-written LINQ; deserialising JSON and protobuf; OperationRegistry lookups (cold and
  warm); the web FilterCompiler (posted JSON -> Filter) and FilterSchema.For<T> (reflection per request: should it be cached?);
  the Razor view rendering with a large schema; allocations as well as time.
- Constants: DONE (see Progress): values reach EF as parameters. Still to do: measure what it saves, and benchmark the
  parameterising pass itself (it walks the finished tree once per ToExpression call).
- Cache what is safe to cache (schema per type and options, reflection results) and prove it with the benchmarks.
- Only if the results are good: add a short, honest "Performance" section to the README with the method and the numbers (hardware,
  .NET version, what was compared), commit the benchmark project so people can reproduce it, and draft an announcement.
  Do not claim anything the benchmarks do not show.

## 5. A more dynamic builder: reports that can start from many different classes
Goal: in my app I want a report generator. The user picks *what* to report on (one of several tables/classes), builds the filter
for it, chooses columns, sorting and (later) grouping, and gets results. Today the builder is bound to one class via
type="typeof(Product)". Design this properly before coding, write the design to docs/architecture/report-builder.md and ask me to
review it. Constraints and a suggested direction:
- Roots are an allow-list, chosen by a stable key (for example "orders"), never a .NET type name from the browser. Something like
  options.AddRoot<Order>("orders", "Orders") / AddRoot<Customer>(...), with per-root and per-property authorisation hooks.
- The schema already supports many types (types by id, collections lead to other types); extend it to several *roots*. The page
  gets a root picker; changing the root resets the filter. A non-generic server path (Build(rootKey) returning an IFilter /
  LambdaExpression) alongside Build<T>().
- Version the posted document (it has "version": 1) and add root, columns (paths, labels, order), sort (path, direction), then
  later grouping/aggregates. Keep it plain text validated against the schema, as now.
- Executing: projecting to selected columns means building a Select expression for a root only known at run time. Decide between
  an expression-tree projection to a generated/anonymous shape, a dictionary/object[] shape, or a typed result you register.
  Make it translate in EF Core, support paging, and think about cost limits (max columns, max rows, timeouts).
- Security is the main risk: column and row-level authorisation per user, never allow a column the user cannot see to be used in a
  filter or sort (it leaks values), apply the app's own mandatory filters (tenant, soft-delete) before the user's filter, and
  limit result sizes. Update SECURITY.md.
- UI: root picker, column chooser (drag to order), sort rows, saved/shared reports (FromFilter already shows a saved filter),
  export (CSV at least). Keep the markup overridable and the data-eb-* contract.
- Deliver in slices: (a) multi-root schema and non-generic build with tests, (b) root picker UI, (c) columns and sort, (d) execution
  and export in the sample app, (e) grouping/aggregates only if still wanted. Each slice stands on its own and is released behind
  docs, not a flag.
- With "export" in mind, allow any report that's been generated to be saved as a "built-in" report for the app that's using it (pluging it in to a dashboard or a "Your reports" page). This would mean teh report would need a anem and for the data to be stored (maybe even a custom storage mechanism to allow a report to be "global" - shown to everyone)

## 6. Other things I would do (decide with me which, and in what order)
- Fix the rough edges listed in docs/architecture/overview.md: Filter<T>.CloseGroup swallowing exceptions, IsNotNullable using
  NullableAttribute on the type, and the object/dynamic generic type used by some no-value overloads (normalise in the templates,
  with tests, as a deliberate change).
- Harden library-format JSON: replace Type.GetType on the "propertyType" in the document with an allow-list of value types
  (primitives, enums registered by the app, DateTime...) and a registry for custom ones. Then SECURITY.md can say more than
  "trusted storage only".
- Package quality: SourceLink and symbol packages (snupkg), deterministic builds, EnablePackageValidation against the last stable
  version once 1.0.0 is out, and a package README per package that explains the one-line purpose.
- Public API tracking: commit an API snapshot (tools/ExpressionBuilder.ApiSnapshot) or adopt PublicApiAnalyzers, and have CI fail
  on unreviewed changes.
- Collapse the duplicated per-operation test files (ExpressionMatchTests/ResultMatchTests exist in 25 drifting copies) into shared
  source, keeping each operation's AllTests data.
- Move the tests from xunit v2 to xunit.v3 (NuGet marks v2 "Legacy"; 2.9.3 is its last release and works). v3 test projects are
  executables and the runner package changes, across ~30 projects, so do it centrally (`tests/Directory.Build.props`,
  `Directory.Packages.props`) and ideally after the duplicated test files above are collapsed, which leaves less to migrate.
  Check coverlet and the CI coverage step still produce a cobertura report.
- Reconsider the 25 operation packages: keep, or ship one package plus the Api for extenders? (Breaking: only before 1.0.0.)
- Localisation: ship resource-based label sets (en-GB, cy, fr, de...) for the builder, plus right-to-left check.
- Native AOT / trimming: document what is not supported (reflection, registries) and add the annotations or a trimming test.
- Docs site and a hosted demo of the sample app (GitHub Pages cannot host ASP.NET; use a container host or record a short GIF).
- More operations people ask for (for example date-part comparisons: IsToday, InLastNDays; regex; collection Count) via the
  documented "add an operation" skill.
- A release candidate plan: 1.0.0-rc.2 from this work, 1.0.0-rc.3 with the report builder (slices a to d), a soak period, then
  1.0.0. Update ChangeLog.md and use the release skill (ai/skills/release.md).

Finish each section by telling me what changed, what you verified and how, what you could not verify, and what you suggest next.
```
