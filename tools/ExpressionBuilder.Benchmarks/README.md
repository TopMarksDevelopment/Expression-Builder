# Expression Builder benchmarks

[BenchmarkDotNet](https://benchmarkdotnet.org/) benchmarks for the library, and one experiment (`ef`) for what values-as-parameters
saves a database. Not packed, not a test project (`dotnet test` does not run it), but it is in the solution so it always builds.
Numbers from a run, with the machine they were taken on, are in [RESULTS.md](RESULTS.md).

## Running

Always `-c Release`; BenchmarkDotNet refuses to produce numbers it trusts from a Debug build. Close other programs first: a
benchmark is only as steady as the machine.

```powershell
cd tools/ExpressionBuilder.Benchmarks
dotnet run -c Release -- --filter "*"                                  # everything (about 25 minutes)
dotnet run -c Release -- --filter "*SchemaBenchmarks*"                 # one class
dotnet run -c Release -- --filter "*FilterBuildBenchmarks.ToExpression*" --job short   # a quick look, not for quoting
dotnet run -c Release -- ef                                            # EF Core: constants against parameters (a few minutes)
dotnet run -c Release -- report                                        # EF Core: the shapes of a report's rows (a few minutes)
```

Results are written to `BenchmarkDotNet.Artifacts/` (ignored by git).

## What is measured

| Class                      | Measures                                                                                                                       |
| -------------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| `FilterBuildBenchmarks`    | Making a `Filter<T>` of 3 / 20 / 100 conditions, `ToExpression()`, the tree on its own, the parameterising pass on its own, `Compile()` |
| `FilterRunBenchmarks`      | A compiled filter against the same filter written by hand, over 10,000 objects                                                |
| `SerializationBenchmarks`  | JSON and protobuf, write and read, at the same three sizes                                                                    |
| `RegistryBenchmarks`       | `OperationRegistry` once everything is known: a hit, a miss, `GetAll`                                                         |
| `ColdRegistryBenchmarks`   | The first lookup of a process (25 new processes, one call each, JIT and assembly loading included)                           |
| `SchemaBenchmarks`         | `FilterSchema.For` and `ToJson()` for a small class and a large one (126 fields)                                              |
| `SchemaPartsBenchmarks`    | Where the time of building a schema goes                                                                                       |
| `PostedFilterBenchmarks`   | A posted filter turned into a `Filter<T>`: the whole of `FilterBuilderModel.Build`, and the compiler alone                     |
| `RazorBenchmarks`          | A whole request for a page with the builder (in-memory test server): a page without one, with a small class, with a large one |
| `EfExperiment` (`-- ef`)   | The same number of queries that differ only in their values, through EF Core and SQLite, with constants and with parameters   |
| `NonGenericBuildBenchmarks` | A report's class is chosen at run time: a checked filter turned into an expression with the class as a type argument, with generics closed when the class was registered, and with reflection on every request |
| `ReportComposeBenchmarks`  | Putting a report together: the projection to the columns chosen (plain and null-guarded), and the whole query (filter, order, tie-break, page, projection) for 5 and 20 columns and 1 and 3 sorts |
| `ResolveColumnsBenchmarks` | Finding the columns and sort keys a user chose in the schema, worst case (the end of a 126-field class)                       |
| `AuthorisationHookBenchmarks` | What a per-property "may this user see it" hook adds to making a schema, measured on its own                              |
| `ReportExperiment` (`-- report`) | Through EF Core and SQLite: does a projection to columns chosen at run time translate (and what SQL), what each way of shaping a row costs a request, what a new set of columns costs the first time, and whether page numbers need to be variables. See `docs/architecture/report-builder.md` |

All of them use the classes in `Models.cs` and the filters in `FilterShapes.cs`, so a number from one class can be compared with
one from another. `FilterShapes.Build(n)` makes a filter of about `n` conditions: mostly flat, with a bracketed group in every ten
and a collection (with a collection inside it) in every twenty-five.

## Reading the numbers

- `ToExpression()` is the tree being built followed by `ValueParameters.Parameterise` over it. `TreeOnly` calls the private builder
  directly, so the difference is the pass.
- `Compile()` is the .NET expression compiler, not Expression Builder's code; it is there because `list.Where(filter)` pays it.
- The EF experiment gives EF a query cache of its own for each run, made the way EF makes its own (10,240 units; 5% dropped
  when full), so the two modes cannot see each other's entries. Its "Memory kept" is what the garbage collector still holds after the
  queries; it is rough.
- A saved filter whose values are of an enum (or any type) from the application cannot be read back: the type's name is saved
  without its assembly. The serialisation benchmarks therefore use a filter without enums (`FilterShapes.Build(n, enums: false)`).
