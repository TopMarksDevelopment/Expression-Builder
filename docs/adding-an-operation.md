# Adding an operation

An operation is a small struct that turns a property and some values into a boolean expression. Everything around it —
the package, the fluent methods, registration — is derived, so most of the work is the operation and its tests.

Take `StartsWith` as the model; copy it.

## 1. The project

Create `src/Operations/<Name>/ExpressionBuilder.Operations.<Name>.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk" />
```

That is the whole file. The package id, assembly name, title, description and the references to the API and the generator
come from `src/Operations/Directory.Build.props`. Only add a `<ProjectReference>` if the operation builds on another one
(`Equal` uses `In`).

Add the project to the solution (`dotnet sln add --solution-folder src/Operations src/Operations/<Name>/*.csproj`) and to
`src/ExpressionBuilder/ExpressionBuilder.csproj` so the main package includes it.

## 2. The operation

`src/Operations/<Name>/<Name>.cs`:

```csharp
namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>The text ends with the value.</summary>
[GenerateFilterExtensions(FilterExtensionShape.Values)]
public struct EndsWith : IOperation
{
    /// <summary>Creates the <see cref="EndsWith"/> operation.</summary>
    public EndsWith() { }

    /// <inheritdoc />
    public readonly string Name => "EndsWith";            // how a saved filter finds the operation: unique, and never renamed

    /// <inheritdoc />
    public readonly OperationDefaults Defaults => new() { Match = Matches.Any, NullHandler = OperationNullHandler.NotNullAnd };

    /// <inheritdoc />
    public readonly Expression Build<TPropertyType>(Expression member, IFilterCollection<TPropertyType?> values, IFilterStatementOptions? options) => …;

    /// <inheritdoc />
    public readonly void Validate(IFilterStatement statement) { … }   // throw ArgumentOutOfRangeException for impossible statements
}
```

Pick the shape for `[GenerateFilterExtensions]` by how many values the operation takes:

| Shape                         | Values                       | Examples                              |
| ----------------------------- | ---------------------------- | ------------------------------------- |
| `FilterExtensionShape.Values` | one value, or an array       | `Equal`, `Contains`, `In`             |
| `FilterExtensionShape.Range`  | an array, or exactly two     | `Between`, `NotBetween`               |
| `FilterExtensionShape.None`   | none                         | `IsNull`, `IsEmpty`                   |

The six sets of fluent methods (`filter.EndsWith(...)` on `IFilter`, `IFilter<T>`, `IFilterable`, …) are generated; there
is nothing to write. If an operation needs a different overload set, add a shape (three templates per shape plus the enum
member) rather than writing the methods by hand.

## 3. Tell the main package, the registry and the web builder

- `src/ExpressionBuilder/Operation.cs`: add a property, `public static EndsWith EndsWith => new();`.
- `tests/ExpressionBuilder.Tests/OperationTests.cs`: add the name to `BuiltInOperations`. The test checks that the name,
  the `Operation` property and the registry all agree.
- `src/ExpressionBuilder.Web/Schema/OperationCatalog.cs`: describe it (label, number of values, the kinds of property it
  suits). `FilterBuilderModelTests.OfferedOperationsWork` then runs it against every kind you listed; if it throws for one,
  that kind does not suit the operation.
- `README.md`: add it to the list of built-in operations. `ChangeLog.md`: note it.

Nothing needs registering: the registry finds it by name.

## 4. Tests

Copy the test project of a similar operation: `tests/Operations/<Similar>/` → `tests/Operations/<Name>/`, renaming the
project to `ExpressionBuilder.Operations.<Name>.Tests.csproj` (its content is empty; references and usings come from
`tests/Operations/Directory.Build.props`). The files are:

- `AllTests.cs` — the cases, as `TestBuilder<T>` rows: the expression you expect, and the same filter built four ways
  (factory, filterable, queryable and fluent).
- `ExpressionMatchTests.cs` — each row's expression, compared as text.
- `ResultMatchTests.cs` — each row's results, compared against the seed data.
- `OperationTests.cs` — `Validate` and any other direct behaviour.

Cover, at least: the value types the operation is meant for, `null` values and members, several values with
`Matches.Any`/`All`, manipulators, and a nullable property (`int?`, `DateTime?`).

## 5. Before you open the pull request

```powershell
dotnet build ExpressionBuilder.sln
dotnet test ExpressionBuilder.sln
dotnet csharpier check .
```
