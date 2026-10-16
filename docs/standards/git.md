# Git conventions

## Branches

`type/scope-description`, kebab-case, off `main`:

| Type        | Use for                                          |
| ----------- | ------------------------------------------------ |
| `feat/`     | New functionality                                |
| `fix/`      | Bug fixes                                        |
| `refactor/` | Restructuring with no behaviour change           |
| `chore/`    | Tooling, config, dependencies, repo housekeeping |
| `test/`     | Test-only changes                                |
| `docs/`     | Documentation-only changes                       |
| `perf/`     | Performance work with no functional change       |

Keep branches short-lived and scoped to one independently reviewable change. Start from the latest `main`; do not create
long-lived integration branches.

## Commits

[Conventional Commits](https://www.conventionalcommits.org/):

```text
type(scope): imperative subject

Optional body — the *why*, not a restatement of the diff.
```

Example: `fix(core): find operations that no code has used yet`.

Scope is the package or area touched (`api`, `core`, `ops`, `web`, `generators`, `ci`, `docs`) — omit it when the change is
genuinely repository-wide.

Keep mechanical changes (moves, renames, formatting) in their own commits, so a reviewer can skip them and `git blame`
still works (`git mv` first, then edit). Keep the `Co-Authored-By` trailer on commits made with AI assistance.

## Pull requests

Squash-merge to `main`. The pull-request title becomes the squash commit, so it must be a valid Conventional Commit
subject. Use `.github/pull_request_template.md` to record context, changes, verification, security impact, risk and
rollback.

A pull request must be up to date with `main` before merge. Sync `main` into the feature branch, resolve conflicts there,
and rerun the complete required check suite. Branch protection should require the **build and test** workflow and prevent
direct pushes to `main`.

Prefer small, incremental pull requests that can be understood, tested and reverted independently. Do not split work so
narrowly that an intermediate pull request leaves `main` broken.

## Workflow actions

Actions in `.github/workflows` are pinned to a full commit SHA with the release in a trailing comment
(`uses: actions/checkout@<sha> # v7.0.1`). A tag can be moved to different code after you reviewed it, and the release
workflow holds the NuGet API key. Dependabot's `github-actions` ecosystem updates the SHA and the comment together; review
those pull requests like any other dependency bump. When adding an action by hand, resolve the tag with
`gh api repos/<owner>/<repo>/commits/<tag> --jq .sha`.

## Releases

A release is a GitHub release, which runs the **Nuget Push CI** workflow: restore, build, test, pack, push every package to
nuget.org. All packages share one version (`VersionPrefix` and `VersionSuffix` in `Directory.Build.props`). Before
releasing: set the version, move the `Unreleased` section of `ChangeLog.md` under the new version with its date, and make
sure `npm run build` in `src/ExpressionBuilder.Web/client` leaves `wwwroot/js/filter-builder.js` unchanged. The procedure is
in [ai/skills/release.md](../../ai/skills/release.md).

## Hand-off

Agents leave completed changes uncommitted for human review unless explicitly asked to commit. Report the files changed and
exact validation performed. Before switching branches, merging or deleting a branch, check `git status` and preserve any
existing work.
