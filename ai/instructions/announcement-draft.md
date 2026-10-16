# Announcement draft: 1.0.0-rc.2

A draft for the release notes, a blog post or a discussion thread. Not published; delete this file once it has been used. Every
figure is from [tools/ExpressionBuilder.Benchmarks/RESULTS.md](../../tools/ExpressionBuilder.Benchmarks/RESULTS.md) (Core i5-12600KF,
Windows 11, .NET 10.0.13, EF Core 10.0.13 with SQLite in memory). Re-run the benchmarks and check them before posting if the hardware
or the code has changed since 2026-10-15.

---

## Expression Builder 1.0.0-rc.2

Expression Builder builds LINQ filters at run time (from code, from strings, from saved JSON or protobuf) and applies them to lists and
to Entity Framework Core queries. The second release candidate of 1.0 is out.

**A filter builder for ASP.NET Core.** `TopMarksDevelopment.ExpressionBuilder.Web` adds a ready-made editor for MVC and Razor Pages:
users pick a property, a condition and a value, group conditions, and you post the result to a page and apply it to your data. The
browser sends plain text that is checked against your class, never .NET types. You can add your own operations, restyle it with
templates (it adds no classes of its own), and it works under a strict Content-Security-Policy. It is tested in Chromium, Firefox and
WebKit, with axe and keyboard-only runs.

**Values are SQL parameters.** A filter's values used to be written into the expression as constants, so every different value was a
different query for EF Core to translate and another entry in its query cache. They are now read from variables. In our experiment,
the same number of queries with different values cost 9 times less per query at 10 values, 14 times at 100 and 26 times at 1,000, and
EF's cache stopped filling up. (That is time in your process, measured on SQLite; a database server will differ.)

**Faster where it was cheap to be.** Using a filter on a list now compiles it from constants, which makes the delegate 18-36% quicker to
create; a web filter builder schema is about 27% quicker to build for a large class. We looked at, and left alone, things that
are already small: moving the values into variables costs 0.7 µs for a three-condition filter and 30 µs for a hundred.

**Also:** .NET 10 only; the fluent methods are generated from templates rather than copied 25 times; operations are found by name even
if nothing has used them yet (saved filters used to fail to load); saved filters can now hold values of your own enums; and a
benchmark project, so you can check all of this yourself.

**Breaking:** `net8.0` is no longer supported, and the assemblies are named like their packages. The changelog has the rest.

Feedback and bug reports are welcome: <https://github.com/TopMarksDevelopment/Expression-Builder/issues>.
