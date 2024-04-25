<!--
Guiding Principles
- Changelogs are for humans, not machines.
- There should be an entry for every single version.
- The same types of changes should be grouped.
- Versions and sections should be linkable.
- The latest version comes first.
- The release date of each version is displayed.
- Mention whether you follow Semantic Versioning.

Types of changes
- Added for new features.
- Changed for changes in existing functionality.
- Deprecated for soon-to-be removed features.
- Removed for now removed features.
- Fixed for any bug fixes.
- Security in case of vulnerabilities.
- Breaking changes for break in new revision
- Other for notable changes that do not effect the functionality
 -->

# Changelog

All notable changes to this project will be documented in this file.

> ❗ Whilst in the beta phase, breaking changes will happen between "minor" releases. Because of this, patches and new features will happen between "patch" releases

## UNPUBLISHED

### Breaking Changes (Overview)

-   `IFilterStatement`
    -   Now has an `Options` property  
        This allows serialising to retain options applied to filters
    -   `Manipulators` has moved into the new `Options` property
-   `IEntityManipulator`
    -   Now requires methods to manipulate Properties/Fields AND raw values
-   `IOperation`
    -   Has new `Defaults` property
    -   `Match` has moved into `Defaults`
    -   `SkipNullMemberChecks` has moved into `Defaults` and is now called `NullHandler`
-   Operations can no longer have their null handler customised
-   Serialisation
    -   To accommodate the changes above, we now serialise `Options` - meaning current `Manipulators` will be lost.
    -   Serialisation could break if the order of types was different. To prevent this there is now a default order and a `TypeTracker.FilterStatementTypes` static property for customisation
-   `Equal` and `NotEqual`
    -   No longer do pre-emptive null checks as checking `null` is excluding actual matches
-   `DoesNotContain`, `DoesNotEndWith`, `DoesNotStartWith`
    -   Now do a pre-emptive "is null OR" check as it was excluding `null`s

### Package: TopMarksDevelopment.ExpressionBuilder(\* All)

#### Fixed

-   Actually fixed the paths for the icons and the links to GitHub source code

#### Changed

-   Compressed `icon.png` to reduce package size increase

### Package: TopMarksDevelopment.ExpressionBuilder.Operations.\*Not\* (All Not Operations)

#### Changes

-   Barring `NotIn`, all `Not` operations no longer rely on their counter operation. This was just unnecessary as the expression access was often simpler

### Package: TopMarksDevelopment.ExpressionBuilder.Operations.\* (All Operations)

#### Breaking changes

-   Reflected the changes made to `IOperation`:
    -   Has new `Defaults` property. `Match` has moved into `Defaults`. `SkipNullMemberChecks` has moved into `Defaults` and is now called `NullHandler`

### Package: TopMarksDevelopment.ExpressionBuilder.Api

#### Breaking changes

-   Made changes made to `IOperation`:
    -   Has new `Defaults` property. `Match` has moved into `Defaults`. `SkipNullMemberChecks` has moved into `Defaults` and is now called `NullHandler`

### Package: TopMarksDevelopment.ExpressionBuilder.Core

#### Breaking changes

-   Serialisation could break if the order of types was different. To prevent this there is now a default order and a `TypeTracker.FilterStatementTypes` static property for customisation

#### Fixed

-   Calling `ToString` on a manipulator now produces with the correct output string.
-   Manipulators that cause an underlying type change no longer throws an exception
-   Serialisation now adds core items based on the non-existence of `IFilterItem` rather than based on the values in the old `ProtoTypes` meaning serialisation could break in certain scenarios
-   Assigning an `OpenCollection` to a variable, then calling a further `OpenCollection` and `CloseCollection` in a chain would leave the variable in the `OpenCollection` state. This would apply further filters to the wrong class/property causing errors/miss-results

### Package: TopMarksDevelopment.ExpressionBuilder.Operations.DoesNotContain

#### Breaking changes

-   Tweaked null check to allow null or a match. As results are excluded if they're null

#### Changes

-   No longer depends on `Contains`

### Package: TopMarksDevelopment.ExpressionBuilder.Operations.DoesNotEndWith

#### Breaking changes

-   Tweaked null check to allow null or a match. As results are excluded if they're null

#### Changes

-   No longer depends on `EndsWith`

### Package: TopMarksDevelopment.ExpressionBuilder.Operations.DoesNotStartWith

#### Breaking changes

-   Tweaked null check to allow null or a match. As results are excluded if they're null

#### Changes

-   No longer depends on `StartsWith`

### Package: TopMarksDevelopment.ExpressionBuilder.Operations.Equal

#### Breaking changes

-   No longer does a null check first. As matching null will always return nothing

### Package: TopMarksDevelopment.ExpressionBuilder.Operations.In

#### Fixed

-   We no longer try to convert `members` to nullable types if they're already nullable

### Package: TopMarksDevelopment.ExpressionBuilder.Operations.IsNotEmpty

#### Breaking changes

-   Tweaked null check to allow null or a match. As results are excluded if they're null

#### Changes

-   No longer depends on `IsEmpty`

### Package: TopMarksDevelopment.ExpressionBuilder.Operations.IsNotNull

#### Changes

-   No longer depends on `IsNull`

### Package: TopMarksDevelopment.ExpressionBuilder.Operations.IsNotNullOrWhiteSpace

#### Changes

-   No longer depends on `IsNullOrWhiteSpace`

### Package: TopMarksDevelopment.ExpressionBuilder.Operations.NotBetween

#### Breaking changes

-   Tweaked null check to allow null or a match. As results are excluded if they're null

#### Changes

-   No longer depends on `Between`

### Package: TopMarksDevelopment.ExpressionBuilder.Operations.NotBetweenExclusive

#### Breaking changes

-   Tweaked null check to allow null or a match. As results are excluded if they're null

#### Changes

-   No longer depends on `BetweenExclusive`

### Package: TopMarksDevelopment.ExpressionBuilder.Operations.NotEqual

#### Breaking changes

-   No longer does a null check first. As results are excluded if they're null

### Package: TopMarksDevelopment.ExpressionBuilder.Operations.NotIn

#### Breaking changes

-   Tweaked null check to allow null or a match. As results are excluded if they're null

### Other (none-user facing)

#### Added

-   Completed adding all tests for the rest of the operations. Closes: #1
-   Added more tests to:
    -   `Core` package
    -   all the `Between` operations
    -   `DoesNotContain` operation
    -   `In` operation
    -   `NotIn` operation
    -   `SmartSearch` operation

#### Changes

-   To better identify and maintain tests that have been added after a fix, they have all been extracted into their own `Fix-Tests.cs` class - this should be the de facto moving forward
-   Converted tests to the preferred `TheroyData` method
-   Removed tests that are related to the (for-now) removed `SkipNullChecks` option
-   Improved on `build-and-test` by correcting `--no-restore` to `--no-build` during the test phase

## [0.2.1-beta] - 2024-03-06

<small>[Compare to previous release][comp:0.2.1-beta]</small>

### Package: TopMarksDevelopment.ExpressionBuilder(\* All)

#### Fixed

-   Fixed the paths for the icons and the links to GitHub source code

#### Changed

-   Updated the copyright year for NuGet packages and `License.md`

### Package: TopMarksDevelopment.ExpressionBuilder.Operations.\* (All Operations)

#### Changed

-   Moved all extension methods from the `TopMarksDevelopment.ExpressionBuilder.Api` namespace to `TopMarksDevelopment.ExpressionBuilder`, for easier access _(not technically "breaking" as the main package is always required to operate)_

## [0.2.0-beta] - 2024-02-28

<small>[Compare to previous release][comp:0.2.0-beta]</small>

### Package: TopMarksDevelopment.ExpressionBuilder.Api

#### Breaking Changes

-   Moved the `Connector` and `Matches` enumerators from `TopMarksDevelopment.ExpressionBuilder.Api` to `TopMarksDevelopment.ExpressionBuilder`, for easier access

### Package: TopMarksDevelopment.ExpressionBuilder

#### Fixes

-   Removed duplicated methods from `TopMarksDevelopment.ExpressionBuilder`, as they were already in the `TopMarksDevelopment.ExpressionBuilder.Api` namespace. This prevented you from using an `Add` extension method if you `use` both namespaces

### General changes (non-package related)

#### Changes

-   Added GitHub `FUNDING.yml` to hopefully get some support/funding 😜
-   (Non-user facing) Updated the GitHub workflows to use the current versions of `checkout` and `setup-dotnet`

## [0.1.0-beta] - 2024-02-28

**Initial release**

[0.2.1-beta]: https://github.com/TopMarksDevelopment/Expression-Builder/releases/tag/v0.2.1-beta
[comp:0.2.1-beta]: https://github.com/TopMarksDevelopment/Expression-Builder/compare/v0.2.0-beta...v0.2.1-beta
[0.2.0-beta]: https://github.com/TopMarksDevelopment/Expression-Builder/releases/tag/v0.2.0-beta
[comp:0.2.0-beta]: https://github.com/TopMarksDevelopment/Expression-Builder/compare/v0.1.0-beta...v0.2.0-beta
[0.1.0-beta]: https://github.com/TopMarksDevelopment/Expression-Builder/releases/tag/v0.1.0-beta
