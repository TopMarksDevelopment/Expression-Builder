# Next steps: a working prompt and to-do list

Written at the end of the restructure / web filter builder session so work can continue elsewhere. It is a to-do list to work
through over several sessions, not one task. Tick items off here (and delete the file when it is done).

The prompt below is paste-ready.

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

## 1. Move to .NET 10 and drop .NET 8
- DECISION first: net10.0 only, or net8.0;net10.0 for one more release? .NET 8 support ends November 2026; .NET 10 is LTS. My
  current lean: net10.0 only, because we are still pre-1.0 and dropping a target later is a breaking change. Confirm the plan
  with me, then do it.
- Change TargetFramework in Directory.Build.props (and the analyzers/generator stay netstandard2.0). Update CI (setup-dotnet 10.x,
  matrix), global.json if you add one, the README "requires" line, CONTRIBUTING, docs/standards, the release skill and the changelog.
- Check everything on .NET 10: build with warnings as errors, all tests, the sample app, `dotnet pack` and inspect the packages
  (dependency groups, staticwebassets, no generator dependency), the probe test (fresh-process operation discovery), and the
  Roslyn generator on the new SDK (Microsoft.CodeAnalysis versions).
- Look for .NET 10 / C# 14 breakages and opportunities: span overload ambiguity in tests (we compare against List<int> for this
  reason), ASP.NET Core 10 changes that affect the tag helper / view component / static web assets, anything now obsolete.
  Do not adopt new language features just because they exist.

## 2. Bump packages
- Directory.Packages.props (xunit, Microsoft.NET.Test.Sdk, coverlet, EF Core and SQLite to the 10.x line, Mvc.Testing,
  protobuf-net, Roslyn, MetadataLoadContext), the client's package.json (typescript, esbuild, jsdom, @types/*), the pinned
  CSharpier (.config/dotnet-tools.json) and the GitHub Actions versions. Remember CSharpier is pinned to 0.29.2 because newer
  versions reformat the whole codebase: if bumping it, do that in its own commit and reformat everything in that commit only.
- Run `dotnet list package --outdated --vulnerable --include-transitive` and `npm audit`; fix or justify each finding.
- One commit per ecosystem (nuget, npm, actions, tools) so a regression is easy to bisect.

## 3. Prove the web builder works for real (the things not yet covered)
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
- Investigate how constants are emitted. Filters embed values as Expression.Constant, which EF Core cannot parameterise, so every
  distinct value can mean a new SQL string and a new entry in EF's query cache. Measure it. If it hurts, emit closure members
  (Expression.Property on a small holder object) so values become parameters, behind tests that compare the produced SQL.
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
- Reconsider the 25 operation packages: keep, or ship one package plus the Api for extenders? (Breaking: only before 1.0.0.)
- Localisation: ship resource-based label sets (en-GB, cy, fr, de...) for the builder, plus right-to-left check.
- Native AOT / trimming: document what is not supported (reflection, registries) and add the annotations or a trimming test.
- Docs site and a hosted demo of the sample app (GitHub Pages cannot host ASP.NET; use a container host or record a short GIF).
- More operations people ask for (for example date-part comparisons: IsToday, InLastNDays; regex; collection Count) via the
  documented "add an operation" skill.
- A release candidate plan: 1.0.0-rc.2 from this work, a soak period, then 1.0.0. Update ChangeLog.md and use the release skill
  (ai/skills/release.md).

Finish each section by telling me what changed, what you verified and how, what you could not verify, and what you suggest next.
```
