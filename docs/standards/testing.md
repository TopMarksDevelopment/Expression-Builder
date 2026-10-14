# Testing

## Stack

xUnit for .NET (with Coverlet for coverage), SQLite in-memory through EF Core for tests that must behave like a database, and
Node's built-in test runner with jsdom for the web client. Use the cheapest layer that proves the behaviour.

## Layout

`tests/` mirrors `src/`:

| Project                                             | What it proves                                                                                  |
| --------------------------------------------------- | ----------------------------------------------------------------------------------------------- |
| `ExpressionBuilder.Core.Tests`                      | `Filter<T>`, expressions, serialisation, registry, EF Core behaviour                            |
| `Operations/<Name>/…Tests`                          | One operation in isolation: its expression, its results, its validation                         |
| `ExpressionBuilder.Tests`                           | The main package: every operation is exposed, registered and found by name                      |
| `ExpressionBuilder.Tests.Probe`                     | A console app those tests run in a **separate process** (see below)                             |
| `ExpressionBuilder.Generators.Tests`                | The source generator, run in-process against sample code                                        |
| `ExpressionBuilder.Web.Tests`                       | The web builder: schema, compiler, round trip, and the sample app over HTTP                     |
| `ExpressionBuilder.Web.E2E`                         | Real browsers (Chromium, Firefox, WebKit): clicking, posting, axe, keyboard, published apps     |
| `ExpressionBuilder.TestBase`                        | Shared models (`Product`, `Category`…), seed data and the `TestBuilder` record                  |
| `src/ExpressionBuilder.Web/client/test`             | The script: pure model tests and DOM tests against the real Razor view                          |

## The operation test pattern

Each operation project has the same four files. A row in `AllTests` is a `TestBuilder<T>`: the expected expression plus the same
filter built through the factory, filterable, queryable and fluent APIs. `ExpressionMatchTests` compares the expression _text_;
`ResultMatchTests` runs it over the seed data and compares the _results_. Both are needed: the text catches a changed expression
shape, the results catch a wrong one. See [adding-an-operation.md](../adding-an-operation.md).

## What to prioritise

Prioritise behaviour that turns untrusted input into an expression, behaviour that depends on property types (nullable value
types, collections, navigation chains), and behaviour that depends on what is loaded at run time. Every bug fix gets a regression
test, and **it must fail without the fix** — revert the fix once to confirm.

Examples of that in this repo:

- `LoadingSavedFiltersTests` fails (a `TypeLoadException` in the probe) if the registry only looks at loaded assemblies.
- `NonGenericFilterableTests` drives the non-generic `IFilterable`/`IQueryFilterable` members that used to call themselves
  until the stack overflowed.
- `FilterBuilderModelTests.OfferedOperationsWork` is a matrix: every operation × every property kind it is offered for, built and
  run both as a delegate and as an expression tree. When it fails, either the operation or the catalog is wrong.

## Browser tests

`tests/ExpressionBuilder.Web.E2E` drives the real thing with Playwright in Chromium, Firefox and WebKit. It is **not** in
`ExpressionBuilder.sln`: it downloads the browsers and is slower, so `dotnet test ExpressionBuilder.sln` stays quick. CI runs it as
its own job (`browsers`). It has four parts:

- `FilterBuilderTests`: build a filter by clicking (every kind of box, collections, the app's own operation and adjustment), post
  it and compare the products shown with what LINQ says they should be.
- `AccessibilityTests`: axe over the builder in each state it can be left in, in light and dark. `KeyboardTests`: the whole flow
  with the keyboard alone, focus after every kind of removal, the names and roles a screen reader would read. This is a
  proxy for a screen reader, not a replacement: before a release, try NVDA/JAWS (Windows) and VoiceOver (macOS, iOS) by hand.
- `PublishedAppTests`: the packages are packed into a local feed and installed into a new MVC app (`MapStaticAssets`) and a new
  Razor Pages app (`UseStaticFiles`), which are published and run in **Production** under a Content-Security-Policy that allows
  nothing inline. The apps are the source under `Consumers/`; they are copied out and built by the test, so nothing is left in the
  repository.

```powershell
dotnet test tests/ExpressionBuilder.Web.E2E              # all three browsers (installs them the first time)
$env:EB_E2E_BROWSERS = "chromium"; dotnet test tests/ExpressionBuilder.Web.E2E   # just one, for speed
```

The Entity Framework Core side is in `ExpressionBuilder.Web.Tests/EfCoreTests.cs`: every operation the builder offers is run
against SQLite in memory and compared with the same filter run over the same objects, and the shapes people use (a collection, a
collection inside a collection, a collection of plain values, text adjustments, nullable and date comparisons) assert on the SQL.

## Process-isolated tests

A test that depends on _which assemblies are loaded_ cannot share a process with other tests. Put the code under test in
`ExpressionBuilder.Tests.Probe`, run it with `dotnet <probe>.dll` from the test, and assert on its output and exit code. Name such
projects `*.Probe`; `tests/Directory.Build.props` then leaves out the test packages.

## Test data

Keep shared models in `ExpressionBuilder.TestBase`. Tests that need a particular shape define a small model beside them
(`ExpressionBuilder.Web.Tests/Models/Person.cs` has one of every property kind). Never use a developer's real data, connection
string or credentials.

When two layers must agree (the server's schema and the script), share a **fixture** and check it in one place:
`client/test/fixtures/person-schema.json` is read by the script's tests, and `SchemaContractTests` fails if it is not exactly what
the server produces. To update it, run the .NET tests once with `UPDATE_FIXTURES=1`, check the script's tests, commit.

The contract also runs the other way: `client/test/contract.test.ts` fills in the real interface like a user and writes the document
it posts to `client/test/fixtures/posted/*.json`; `PostedFixtureTests` builds each one on the server and checks it finds the right
people. Regenerate those with `UPDATE_FIXTURES=1 npm test` in the client folder.

## Public API check

Before a refactor, and again before opening the pull request, snapshot the public API (one member per line, sorted) and
compare. The snapshot reads built assemblies without loading them, so take it from a build of the base commit and one of your
branch:

```powershell
dotnet build ExpressionBuilder.sln
dotnet run --project tools/ExpressionBuilder.ApiSnapshot -- tests/ExpressionBuilder.Tests/bin/Debug/net10.0 > api-after.txt
# ...the same on the base commit, into api-before.txt, then:
git diff --no-index api-before.txt api-after.txt
```

Additions are fine (note them in the changelog); anything removed or changed is a breaking change. This is how the restructure and
the move to generated fluent methods were shown to change nothing.

## Running tests

```powershell
dotnet test ExpressionBuilder.sln                        # everything on .NET
dotnet test tests/ExpressionBuilder.Web.Tests            # one project
dotnet test tests/ExpressionBuilder.Web.E2E              # the browser tests (not in the solution; see above)

cd src/ExpressionBuilder.Web/client
npm ci
npm test                                                 # script tests
npm run check                                            # type-check + bundle + tests
```

CI runs the .NET tests on Linux, Windows and macOS, and the client checks (including that the committed bundle is current) once.

## Coverage

Coverage is a diagnostic, not the target. CI collects it (Coverlet) and uploads the report; use it to find branches nobody has
exercised, then decide whether they matter. Don't add tests that only execute lines.
