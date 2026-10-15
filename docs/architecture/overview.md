# Architecture overview

How the packages fit together, and the decisions that are easy to undo by accident.

## Packages

```text
                       ┌───────────────────────────┐
                       │ ExpressionBuilder.Web     │  Razor/MVC filter builder (+ TypeScript)
                       └───────────┬───────────────┘
                                   │ references
                       ┌───────────▼───────────────┐
                       │ ExpressionBuilder         │  "main": every operation + the Operation class
                       └───────┬───────────┬───────┘
                               │           │
              ┌────────────────▼──┐   ┌────▼──────────────────────────┐
              │ ExpressionBuilder │   │ ExpressionBuilder.Operations.* │  one package per operation
              │ .Core             │   │ (Equal, Between, Contains …)   │
              └────────┬──────────┘   └────┬───────────────────────────┘
                       │                   │
                       └────────┬──────────┘
                       ┌────────▼──────────┐
                       │ ExpressionBuilder │  interfaces: IOperation, IFilter, IFilterable …
                       │ .Api              │
                       └───────────────────┘
```

| Package (`TopMarksDevelopment.` prefix)    | Holds                                                                                              |
| ------------------------------------------ | -------------------------------------------------------------------------------------------------- |
| `ExpressionBuilder.Api`                    | The contracts. Operations and your own extensions depend only on this.                             |
| `ExpressionBuilder.Core`                   | `Filter<T>`, groups and statements, serialisation (JSON, protobuf), `OperationRegistry`.           |
| `ExpressionBuilder.Operations.<Name>`      | One operation and its fluent methods. Small on purpose: install only what you use.                |
| `ExpressionBuilder`                        | References Core and every operation; adds the `Operation` class (`Operation.Equal`).               |
| `ExpressionBuilder.Web`                    | The browser filter builder. Depends on the main package.                                           |

`ExpressionBuilder.Generators` is a build-time source generator used by the operation projects. It is never referenced by
a consumer and ships in no package.

The dependency arrows only point down: operations know nothing of `Core`, `Core` knows nothing of any operation, and
nothing knows of the web package.

## Where an expression comes from

A `Filter<T>` is a tree: groups (optionally targeting a collection) containing statements. A statement is a property id
(`Birth.Country`, `Contacts[].Type`), an `IOperation`, values and options (match any/all, manipulators). `ToExpression()`
walks the tree: `ExpressionExtensions.GetExpression` resolves the property chain (adding null checks and `Any(...)` for
collections as the operation's `NullHandler` asks) and hands the member to the operation's `Build`, which returns the
boolean expression.

**Values are variables, not constants.** The operations write the values they compare with as constants, and then
`ToExpression()` replaces each plain value (text, numbers, dates, ids, enums, and lists of those) with a read of a small
holder (`ValueHolder<T>.Value`; `Extensions/ValueParameters.cs`). A database provider sees a captured variable and sends a
SQL parameter, where a constant would become text in the SQL: with constants every different value is a different query and
a new entry in EF Core's query cache. `null`, `true`/`false` and the empty text stay constants because they are part of the
query's shape. This is done once, over the finished tree, rather than in every operation, so a new operation gets it for free.
`FilterExpressionExtensions.InlineValues()` undoes it (the expression-text tests use it to compare with hand-written lambdas).
A filter that is used on objects in memory (`list.Where(filter)`) skips the pass altogether and compiles the constants: nothing
there sends values to a database, and the expression compiler is measurably slower with variables to fetch
(`tools/ExpressionBuilder.Benchmarks`).

## Operations are found by name

A saved filter (JSON or protobuf) stores an operation as text (`"Between"`). `OperationRegistry` turns the name back
into an operation. This used to scan `AppDomain.GetAssemblies()`, which only lists assemblies that are _already
loaded_ — so a filter loaded before any code touched `Operation.Between` failed with a `TypeLoadException`.

The registry now looks in three places, each only when the name is not yet known and something new has loaded:

1. assemblies that are loaded and reference `ExpressionBuilder.Api`;
2. the Expression Builder assemblies those reference (loaded on demand);
3. `TopMarksDevelopment.ExpressionBuilder*.dll` files next to the application (a package can be installed without any code
   using one of its types, so nothing references it).

Operations in your own assemblies are found when the assembly is loaded; otherwise register them
(`OperationRegistry.Register<T>()`). The same mechanism finds `IEntityManipulator`s (`ManipulatorRegistry`).

`tests/ExpressionBuilder.Tests.Probe` exists because this can only be tested honestly in a fresh process: inside a test
host another test has usually loaded everything already.

## The fluent methods are generated

`filter.Equal(x => x.Name, "a")` and friends exist for six receivers (`IFilter`, `IFilter<T>`, `IFilterable`,
`IFilterable<T>`, `IQueryFilterable`, `IQueryFilterable<T>`) and, per receiver, 16 (or 8) overloads. They used to be
copied by hand into every operation. An operation now declares only its shape:

```csharp
[GenerateFilterExtensions(FilterExtensionShape.Values)]   // Values, Range or None
public struct Equal : IOperation { … }
```

and `ExpressionBuilder.Generators` stamps out the six `…Extensions` classes from `Templates/<Shape>/<Receiver>.template`
(`__Op__` is the operation's name). Three shapes × six receivers = 18 templates.

Why templates and not one clever generator? The overload sets differ in small ways the compiler can't tell you are
accidental — for example the no-value operations build `Array.Empty<dynamic?>()` for string properties on `IFilter` but
`[]` elsewhere, which changes the generic type of the statement. The templates are the verbatim sources of `Equal`,
`Between` and `IsNull`, and the generated API was verified identical to the hand-written one
(`tools/ExpressionBuilder.ApiSnapshot`). Normalising the quirks is a separate, deliberate change.

## Package metadata is derived

`src/Directory.Build.props` sets `AssemblyName`/`PackageId` to `TopMarksDevelopment.<project name>`; an operation's title and
description come from its project name; versions are central (`Directory.Packages.props`). Adding an operation is a folder,
a one-line `.csproj` and a class — see [adding-an-operation.md](../adding-an-operation.md).

## The web filter builder

See [web-filter-builder.md](../web-filter-builder.md) for use. Design points that matter when changing it:

- **One schema, two readers.** `FilterSchema` is built by reflection and serialised for the script; the same object checks
  the posted filter, so the browser and the server cannot disagree about what is allowed.
- **Custom operations and manipulators are opt-in, by description.** An `IOperation` or `IEntityManipulator` is offered
  only when the app describes it (`OperationCatalog` / `ManipulatorCatalog`, or `FilterBuilderOptions.AddOperation` /
  `AddManipulator` for one builder). The schema built from those descriptions is also what a post is checked against, so
  a browser can only choose a described thing *by name*; what the name makes is decided by the descriptor's factory.
  Built-in and custom are kept apart (separate groups in the UI, and a custom one can't take a built-in name), so nothing
  can quietly replace `Equal` or `Trim`. We do not resolve a posted name through `OperationRegistry` /
  `ManipulatorRegistry` alone: that would make *every* installed manipulator reachable from a browser.
- **The wire format is not the library's JSON.** The library's JSON names .NET types (`propertyType`) and would let a browser
  pick one. The builder posts plain text (property path, operation name, string values) which `FilterCompiler` validates and
  only then turns into statements.
- **Presentation is not on `IOperation`.** Which kinds of property an operation suits, how many values it takes and what to
  call it live in `OperationCatalog`, so operations stay usable without a UI. A test runs every offered
  operation/kind pair end to end, which keeps the catalog honest.
- **The Razor view owns the markup, the script owns behaviour.** The script clones `<template>`s found by `data-eb-*`
  attributes, so an app can replace `Default.cshtml` freely.
- **The compiled script is committed.** `client/` builds `wwwroot/js/filter-builder.js`; consumers and the .NET build never need
  node. CI rebuilds it and fails if it differs.

## Known rough edges

Not fixed here, because each is a behaviour change that needs its own decision:

- `Filter<T>.CloseGroup` swallows any exception from closing the parent filter (a workaround for collection chaining);
  errors there are silent.
- `IsNotNullable` in `ExpressionExtensions` decides nullability from the presence of a `NullableAttribute` on the _type_,
  which is not what that attribute means; it works for the cases tested, but is fragile.
- The no-value operations build their statement with `object`/`dynamic` as the generic type on some receivers (see above), so
  the saved JSON's `propertyType` varies by which overload was used.
- `Filter<T>` JSON deserialisation reads a type name from the document (`Type.GetType`). Only load library-format JSON from
  storage you trust; use the web builder's format for anything that comes from a browser.
