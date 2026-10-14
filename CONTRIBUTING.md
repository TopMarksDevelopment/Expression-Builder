# Contributing

Thank you for helping. This page gets you from a clone to a pull request. The conventions behind it are in
[AGENTS.md](AGENTS.md) and [docs/standards/](docs/standards/); how the packages fit together is in
[docs/architecture/overview.md](docs/architecture/overview.md).

## Set up

You need the **.NET 10 SDK, 10.0.402 or later** (`global.json`; the source generator references Roslyn 5.9, so an older
SDK fails with CS9057) and, only if you change the web filter builder's
script, **Node 22**.

```powershell
git clone https://github.com/TopMarksDevelopment/Expression-Builder.git
cd Expression-Builder
dotnet tool restore        # CSharpier, at the version the code was formatted with
dotnet build ExpressionBuilder.sln
dotnet test ExpressionBuilder.sln
```

The first build also builds the source generator and the test probe app; it takes a minute. Open `ExpressionBuilder.sln` in your
editor of choice (the repo includes VS Code tasks).

## Where to make a change

| I want to…                              | Look at                                                                                     |
| --------------------------------------- | ------------------------------------------------------------------------------------------- |
| fix a bug in how a filter is built      | `src/ExpressionBuilder.Core`, or the operation in `src/Operations/<Name>`                   |
| add an operation                        | [docs/adding-an-operation.md](docs/adding-an-operation.md)                                  |
| change the fluent methods of operations | the templates in `src/ExpressionBuilder.Generators/Templates`                               |
| work on the web filter builder          | [docs/web-filter-builder.md](docs/web-filter-builder.md#working-on-the-filter-builder-itself) |
| try the web filter builder              | `dotnet run --project samples/ExpressionBuilder.Web.Sample`, then open http://localhost:5099 |

## Before you open a pull request

1. Branch as `type/scope-description` (`fix/`, `feat/`, `refactor/`, `chore/`, `test/`, `docs/`, `perf/`) and write
   [Conventional Commit](https://www.conventionalcommits.org/) messages. See [docs/standards/git.md](docs/standards/git.md).
2. Add tests with the change. A bug fix needs a regression test that fails without the fix.
3. Run what CI runs:
   ```powershell
   dotnet build ExpressionBuilder.sln -c Release       # no warnings: they are errors in CI
   dotnet test ExpressionBuilder.sln -c Release --no-build
   dotnet csharpier check src tests samples tools
   ```
   and, if you changed `src/ExpressionBuilder.Web/client`: `npm ci && npm run check`, then commit the rebuilt
   `wwwroot/js/filter-builder.js`.
4. Update the docs and [ChangeLog.md](ChangeLog.md) (under `Unreleased`) in the same pull request.
5. Fill in the pull request template.

## Reporting problems

Use the issue templates. For a security problem, follow [SECURITY.md](SECURITY.md) instead of opening a public issue.
