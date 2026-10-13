# Skill: add an operation

Use when asked to add a built-in operation (for example `IsToday`, `Matches`) to the library. The human-readable version, with
code, is [docs/adding-an-operation.md](../../docs/adding-an-operation.md) — read it first; this is the checklist to work through
and verify.

1. **Check it does not already exist** (`src/Operations/`, `README.md`'s list) and that an existing operation with options
   (`Match`, manipulators) can't express it. A new package is a permanent public commitment.
2. Copy `src/Operations/StartsWith` (or `Between` / `IsNull` for the other shapes) to `src/Operations/<Name>`; rename the
   `.csproj` to `ExpressionBuilder.Operations.<Name>.csproj` and make its content `<Project Sdk="Microsoft.NET.Sdk" />`
   (plus a `<ProjectReference>` only if it builds on another operation).
3. Implement `Name`, `Defaults`, `Build`, `Validate`; set `[GenerateFilterExtensions(FilterExtensionShape.Values|Range|None)]`.
   XML-document the type and constructor (`<inheritdoc />` on the rest). Do **not** write fluent extension methods.
4. Register the project: `dotnet sln add --solution-folder src/Operations …`, `src/ExpressionBuilder/ExpressionBuilder.csproj`,
   and `public static <Name> <Name> => new();` in `src/ExpressionBuilder/Operation.cs`.
5. Add the name to `BuiltInOperations` in `tests/ExpressionBuilder.Tests/OperationTests.cs`.
6. Describe it in `src/ExpressionBuilder.Web/Schema/OperationCatalog.cs` (label, arity, kinds).
7. Copy the matching test project to `tests/Operations/<Name>/` (rename the project; keep its content `<Project Sdk=… />`) and
   write cases: each supported value type, null values/members, several values with Any/All, manipulators, a nullable property.
8. Update `README.md` (built-in operations) and `ChangeLog.md`.
9. Verify: `dotnet build ExpressionBuilder.sln` (no warnings), `dotnet test ExpressionBuilder.sln`, `dotnet csharpier --check
   src tests samples tools`. The Web matrix test runs the new operation against every kind in its catalog entry — if it fails for a
   kind, remove that kind from the entry (the operation doesn't suit it).

Do not run `git commit` unless asked.
