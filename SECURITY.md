# Security policy

## Reporting a vulnerability

Please **do not open a public issue** for a security problem. Use GitHub's private reporting:
[Report a vulnerability](https://github.com/TopMarksDevelopment/Expression-Builder/security/advisories/new). Include what you
found, how to reproduce it, and the package and version. You will get a reply as soon as it can be looked at, and a fix and
disclosure will be agreed with you.

## Supported versions

The latest release (including release candidates, until the stable release) is supported. Fixes are not back-ported.

## Trust boundaries

The library turns descriptions of a filter into expressions that run against your data, so what it is willing to _read_ matters.

| Input                                                       | Trusted?                | Notes                                                                                                                                  |
| ----------------------------------------------------------- | ----------------------- | -------------------------------------------------------------------------------------------------------------------------------------- |
| Filters you build in code                                   | Yes                     |                                                                                                                                        |
| A filter's own JSON or protobuf (`JsonSerializer`, `DeserializeFrom`) | **Only from storage you trust** | The JSON names a CLR type for each value (`propertyType`) and an operation or manipulator by name. Don't deserialise it from a request. |
| What the web filter builder posts (`FilterBuilderModel.Json`) | **No**                 | Plain text only. Properties, operations and values are validated against the schema before anything is built; see below.               |
| Operation and manipulator names                             | Looked up, never created | `OperationRegistry`/`ManipulatorRegistry` only return types that implement the interface. A _method_ manipulator (`x => x.Name.Trim()`) is stored as a CLR type and method name and run on the value; static methods are refused when a filter is read, but that is a second reason to treat the library's own JSON as trusted-only. |

### Operations and manipulators of your own

The web filter builder offers an operation or manipulator of yours only if your app has described it (`OperationCatalog`,
`ManipulatorCatalog`, or the options of one builder), and a browser can only choose it by name. Installing a package that
contains one is not enough, and a name that was not described is refused, so a post cannot reach any other type or method.
Custom ones are listed apart from the built-in ones and cannot take a built-in name. Once described, though, your code
runs on what the user typed: check it in `IEntityManipulator.Validate` / your operation's `Validate`, throw an
`ArgumentException` with a message fit for the user, and remember that text can be up to `MaxValueLength` characters.

### What the web filter builder enforces

A property path is only ever looked up in the schema (never used to reach into the class); an operation must suit the property;
values are parsed as the property's type (enum values must be a member name); adjustments are chosen from a fixed list and apply
to text only; and the sizes of a filter (conditions, values, value length, group depth, adjustments, payload) are limited
(`FilterBuilderOptions`). Posted text is never rendered as markup, and the form is protected by ASP.NET Core's anti-forgery token
in the usual way.

### What it can't decide for you

Which properties are safe to search. Being able to filter on a value reveals it ("Cost over 10", "Token starts with a"), even if it
is never displayed. Mark such properties `[FilterBuilderIgnore]` (or `options.Exclude<T>(…)`), and apply your own authorisation to
the data being filtered (`db.Orders.Where(mine).Where(filter)`) — a filter narrows a query, it must never be what widens it.

A filter can also be expensive. The limits above bound the size of one, but a database should still have a command timeout and the
page should be rate limited like any other search.
