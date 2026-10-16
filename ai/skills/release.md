# Skill: cut a release

Use when asked to prepare a release. Publishing itself is a human action (creating the GitHub release) — never push, tag or
publish unless explicitly asked, and never handle the NuGet API key: it is a GitHub Actions secret (`NUGET_KEY`).

1. **Start clean.** `git status` must be clean on an up-to-date `main` (or a `chore/release-x.y.z` branch off it).
2. **Version.** One version for every package: `VersionPrefix` / `VersionSuffix` in the root `Directory.Build.props`
   (`1.0.0` + `rc.1`; an empty suffix is a stable release).
3. **Changelog.** In `ChangeLog.md`, rename `## Unreleased` to `## [x.y.z] - YYYY-MM-DD`, add its `[comp:x.y.z]` compare link at
   the bottom (copy the previous entry), and start a new empty `## Unreleased`. Check each package that changed has an entry.
4. **Verify everything CI will.**
   ```powershell
   dotnet build ExpressionBuilder.sln -c Release -p:ContinuousIntegrationBuild=true   # warnings are errors
   dotnet test ExpressionBuilder.sln -c Release --no-build
   dotnet csharpier check .
   cd src/ExpressionBuilder.Web/client; npm ci; npm run check; git diff --exit-code ../wwwroot
   cd ../../..; dotnet pack ExpressionBuilder.sln -c Release
   ```
5. **Inspect the packages** (`**/bin/Release/*.nupkg`; a `.nupkg` is a zip): the operation packages depend on `Api` (and `In`
   where they use it) and **not** on the generator; the web package contains `staticwebassets` for `js/` and `css/`; each has
   `README.md`, `LICENSE.md` and the icon.
6. Commit as `chore(release): x.y.z` (with the `Co-Authored-By` trailer when AI-assisted), open the PR, and leave publishing to the
   human: create a GitHub release for tag `vx.y.z`, which runs the **Nuget Push CI** workflow.
