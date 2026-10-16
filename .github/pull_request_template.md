# Pull request

## Context

<!-- Why is this change needed? Link the issue or discussion. -->

## Changes

<!-- Summarise observable behaviour and any deliberate non-goals. -->

## Verification

- [ ] `dotnet build ExpressionBuilder.sln` — no warnings
- [ ] `dotnet test ExpressionBuilder.sln`
- [ ] `dotnet csharpier check .`
- [ ] Web client changed: `npm run check` in `src/ExpressionBuilder.Web/client`, and the rebuilt `wwwroot/js/filter-builder.js` is committed
- [ ] A bug fix has a regression test that fails without the fix
- [ ] Public API: additions and removals reviewed (see `docs/standards/testing.md`)

## Security

<!-- Note anything that turns untrusted input (a posted filter, stored JSON/protobuf) into an expression, loads types by name, or changes what the web builder accepts. Write "None" when not applicable. -->

## Risk and rollback

<!-- State likely failure modes and how to revert or disable the change. -->

## Documentation

- [ ] README / docs / `ChangeLog.md` are updated, or no documentation change is needed.
- [ ] The branch is up to date with `main` and checks were rerun after the update.
