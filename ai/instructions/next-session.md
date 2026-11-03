# Next session: build the report builder (slice a), after `rc.2` is tagged

Paste the prompt below to start. Sections 1 to 5 of [next-steps.md](next-steps.md) are done (its "Progress" section has the history;
section 5 is the design, [report-builder.md](../../docs/architecture/report-builder.md), reviewed and decided on 2026-10-16). This file
says where to start, what you need to do yourself, and what to watch for.

## Where things stand

- **`1.0.0-rc.2` is merged to `main`** (PR #25, squash commit `640cd87`; the older PR #24 was closed as superseded). It is dated
  2026-11-04 in the changelog and is **not tagged or published yet**: that is the release step below. This branch,
  `feat/report-builder-rc3`, is `main` plus the report builder design, benchmarks and plan, and is where `rc.3` is built.
- **rc.2 changed how related objects work, and the report builder is built on it:** a relationship is a field that leads to the schema
  of its class, a path is resolved one name at a time (`FilterSchema.FindField`, limited by `MaxNavigationDepth`, default 5), the
  field picker is a path above a list (properties, collections, relationships), and a group on a collection matches `Any` or `All`
  (`IFilterGroup.Match`). See "Relationships and paths" in the design and decision D7.
- `net10.0` only, SDK 10.0.402+ (`global.json`), version **`1.0.0-rc.2`** (`1.0.0-rc.1` was published on 2024-09-19).
- Done: .NET 10, package and tooling bumps, custom operations and manipulators, restyling by templates, accessibility fixes, browser
  tests, Entity Framework Core tests, values sent to the database as parameters, performance (benchmarks, README section, a draft
  announcement, two small speed-ups, the fix for saved filters holding values of the application's own types), CSharpier over the whole
  repository (`dotnet csharpier check .`), the `rc.2` version, and **the report builder design**.
- At the rc.2 merge, all of CI was green: `dotnet test ExpressionBuilder.sln` (Core 284, Web 596, and the rest), `npm run check` in the
  client folder (127), and `dotnet test tests/ExpressionBuilder.Web.E2E` (not in the solution; needs browsers) in Chromium, Firefox and WebKit.
- **The decisions on the report builder (2026-10-16):** D1 stays in `ExpressionBuilder.Web` (a second tag helper that renders the filter
  builder); **D2 ships as `1.0.0-rc.3` = slices a to d** (so the public surface of those slices becomes part of 1.0.0: keep it small);
  D3 refuse a shared report that uses a column the runner cannot see; D4 buttons to reorder columns (drag and drop is parked); D5 the order after
  `rc.3` is totals, saved reports, measures, grouping, computed columns, charts; D6 charts are data mainly, and a tiny built-in SVG
  renderer is to be considered when slice h is picked up. See "Outputs beyond columns" in the design.

## What you need to do yourself

1. **Publish `1.0.0-rc.2`** from `main`, on or after 2026-11-04 (`ai/skills/release.md`: the checks, the tag, the GitHub release that
   runs the Nuget Push CI workflow). Nothing is tagged yet. Then merge `main` into this branch if anything changed.
2. **Delete the branches that are done:** `refactor/repo-restructure-and-filter-builder` and `ccr-58c60985-dzr141` (both are the
   old rc.2 history, replaced by the squash), `feat/report-builder` (its content is on this branch) and `ccr-4be388e8-541d66`
   (merged).
3. **A human screen-reader pass** (NVDA or JAWS on Windows, VoiceOver on macOS and iOS) and keyboard use on a phone. The browser tests
   check names, roles, focus and axe, which is a proxy.
4. **Check the announcement and the README "Performance" numbers on your own machine before you publish anything.** They are from one
   machine on 2026-10-15. `cd tools/ExpressionBuilder.Benchmarks; dotnet run -c Release -- ef` takes a few minutes; the whole suite
   (`-- --filter "*"`) about 25. Close other programs first. Delete `announcement-draft.md` once it has been used. (It says `rc.2` now.)
5. **Re-run the report measurements on a quiet machine** (`-- report`, and the `Report*` / `NonGenericBuild*` benchmarks) if you rely on
   any ratio near 1; the design says which ones. They were taken with a game and a virtual machine running.
6. **Decided earlier, nothing left to do:** the filter schema is not cached (recorded in `RESULTS.md`, "Not cached"); the
   `FilterBuilderOptions` map of class names is parked (decide it together with the report builder's `AddRoot` / `Restrict<T>` if it
   comes back).

## The prompt

```text
Read AGENTS.md and ai/instructions/engineering-principles.md first (standing instruction: call out anything in my requests that
conflicts with them, and give a real recommendation when I ask for an opinion). Then read ai/instructions/next-session.md,
ai/instructions/next-steps.md ("Progress" and "Parked"), docs/architecture/report-builder.md (the whole of it: it is the spec),
docs/architecture/overview.md, docs/web-filter-builder.md, docs/standards/testing.md, SECURITY.md and
tools/ExpressionBuilder.Benchmarks/RESULTS.md.

State: the report builder design is reviewed and D1-D4 are decided (see "Decisions" in the design). The version is 1.0.0-rc.2 and
rc.3 will be slices a to d. First check that rc.2 is published (tagged) and that this branch includes main; if not, tell me what is left and stop.
Then work on this branch (feat/report-builder-rc3), or a short branch off it for one slice.

Do the three "before the first slice" changes as their own commits (split FilterCompiler.Compile(string) into reading and
Compile(FilterNode); a FilterBuilder view component overload that takes a built FilterSchema; nothing else), and then slice a ONLY:
several roots, the user's schema, the non-generic build, with the tests listed for slice a in the design. Keep the new public surface
as small as you can (internal until it is needed), because 1.0.0 will freeze whatever rc.3 makes public; list every public addition
for me. Do not start slice b.

Say what you will do and what "done" means before you start. Tests with every behaviour change (the security rows of the design are
tests, not intentions); keep mechanical changes in their own commits; keep the solution warning-free; run the AGENTS.md checks before
handing back. Commit in logical slices only when I have asked you to, and push only when I say so. Finish with: what changed, what you
verified and how, what you could not verify, and what you suggest next.
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
- **The public surface of `rc.3` becomes 1.0.0's promise.** The design's illustrative names (`ReportQuery`, `ReportRun`, the root
  builder) are the ones to review hardest during the soak.

## Gotchas that cost time before

- Line endings can differ between files in the working tree (`core.autocrlf=true`; the docs and `.cs` files checked on 2026-10-16
  were LF), so check before a multi-line `sed`/`perl` pattern, or use the editor tool. Large bash heredocs full of quotes can fail to
  parse and write nothing. `python` here is `/c/Python314/python`; `python3` is the Store shortcut and does not run.
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
- A background `dotnet test` can leave a `testhost` holding files for a while; the next build then retries copies and warns (MSB3026).
  Wait for it to exit before building again.
