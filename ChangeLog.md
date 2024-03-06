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

> ❗ Whilst in the beta phase, breaking changes will happen between "minor" releases. Because of this, patches and new features will happen at between "patch" releases.

## <!-- [0.2.1-beta] - -->UNPUBLISHED

<!-- <small>[Compare to previous release][comp:0.2.1-beta]</small> -->

### Package: TopMarksDevelopment.ExpressionBuilder(* All)

#### Fixed

-   Fixed the paths for the icons and the links to GitHub source code

#### Changed

-   Updated the copyright year for NuGet packages and `License.md`

### Package: TopMarksDevelopment.ExpressionBuilder.Operations.* (All Operations)

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
