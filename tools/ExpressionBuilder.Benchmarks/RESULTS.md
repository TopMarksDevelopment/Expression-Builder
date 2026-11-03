# Results

Numbers from the benchmarks in this folder, on one machine, on one day. They are for comparing one change with another on the same
machine; do not read them as promises about yours. To reproduce, see the [README](README.md).

| | |
| --- | --- |
| Date | 2026-10-15 |
| CPU | 12th Gen Intel Core i5-12600KF, 10 cores / 16 threads |
| OS | Windows 11 (10.0.26300) |
| .NET | SDK 10.0.402, runtime 10.0.13 (X64 RyuJIT, x86-64-v3) |
| Libraries | Expression Builder 1.0.0-rc.2 (this branch), BenchmarkDotNet 0.15.8, Entity Framework Core 10.0.13 with SQLite in memory |

Times are the mean of BenchmarkDotNet's default job (its own output has the error and standard deviation; the figures here are
rounded); the figure in brackets is managed memory allocated per call (1 KB = 1,024 bytes). "Small", "medium" and "large" filters are 3, 20 and 100 conditions (see `FilterShapes.cs`).

## What values-as-parameters saves a database

The same number of queries through EF Core, each with a different text and number (`Name == "itemN" && Quantity >= n`), a new
`DbContext` for each, 1,000 rows, after a few queries of the same shape so EF's own start-up is not counted. "Constants" is the old
behaviour (`ToExpression().InlineValues()`); "Parameters" is `ToExpression()`. Each row is the median of several runs (15 for one
query, down to a single run for 12,000). The `ef` mode of this project makes the table.

| Different values | Mode | Total | Per query | New query-cache entries |
| ---: | --- | ---: | ---: | ---: |
| 1 | Constants | 4.3 ms | 4.3 ms | 2 |
| 1 | Parameters | 0.80 ms | 0.80 ms | 0 |
| 10 | Constants | 35.9 ms | 3.6 ms | 20 |
| 10 | Parameters | 4.2 ms | 0.42 ms | 0 |
| 100 | Constants | 296 ms | 3.0 ms | 200 |
| 100 | Parameters | 21.3 ms | 0.21 ms | 0 |
| 1,000 | Constants | 2.57 s | 2.6 ms | 991 |
| 1,000 | Parameters | 99 ms | 0.10 ms | 0 |
| 12,000 | Constants | 31.2 s | 2.6 ms | 945 |
| 12,000 | Parameters | 1.03 s | 0.09 ms | 0 |

- With constants, each different value is a new query for EF to translate (about 2.6-4 ms), and adds two entries to its cache (20
  for 10 values). The cache stopped growing at about 950-990 entries, where 1,000 values would have made 2,000, and stayed there for
  12,000: it was full, and each new value pushed something else out. With parameters, the first query's plan serves every value.
- Per query, that is 5 times quicker for a single new value, 9 times at 10, 14 at 100 and 26-31 from 1,000 values on.
- The same value 1,000 times (both modes find their plan in the cache): 121 µs per query with constants and 103 µs with parameters.
  That is within what varies between runs (the order was the other way round in an earlier one), so the extra pass of `ToExpression()`
  is not visible next to a query.
- Not measured: a real database server, which has plan caches of its own. This says nothing about them.

## Building a filter and an expression

| Conditions | Make the filter | `ToExpression()` | The tree alone | The parameterising pass | Pass, as a share of `ToExpression()` |
| ---: | ---: | ---: | ---: | ---: | ---: |
| 3 | 0.31 µs (1.2 KB) | 5.3 µs (10.7 KB) | 4.3 µs (9.4 KB) | 0.67 µs (1.3 KB) | 13% |
| 20 | 2.0 µs (7.3 KB) | 29.0 µs (51.6 KB) | 23.4 µs (44.3 KB) | 5.0 µs (7.0 KB) | 17% |
| 100 | 10.6 µs (37.5 KB) | 171.7 µs (296 KB) | 133.4 µs (259 KB) | 29.7 µs (36.8 KB) | 17% |

`InlineValues()` (the opposite pass) takes 0.37 µs, 3.0 µs and 18.8 µs. Against turning the expression into a delegate, which is what
`list.Where(filter)` does, the pass is 0.2-0.4% (below).

## Filtering objects in memory

Turning an expression into a delegate (`Expression.Compile()`, not our code), with the values as variables (what `ToExpression()` gives
a database) and as constants:

| Conditions | `Compile()`, variables | `Compile()`, constants |
| ---: | ---: | ---: |
| 3 | 339 µs | 209 µs |
| 20 | 2.04 ms | 1.47 ms |
| 100 | 8.35 ms | 6.90 ms |

So `list.Where(filter)` and `ToFilterable()` compile constants. Using a `Filter<T>` as a delegate (`Func<T, bool> f = filter`), same
session, before and after the change:

| Conditions | Before | After | |
| ---: | ---: | ---: | ---: |
| 3 | 361 µs (16.8 KB) | 231 µs (14.9 KB) | -36% |
| 20 | 2,155 µs (74.6 KB) | 1,501 µs (62.0 KB) | -30% |
| 100 | 8,702 µs (451 KB) | 7,149 µs (365 KB) | -18% |

Running the delegate over 10,000 objects (`name contains`, `age >=`, `active`), against the same filter written by hand. The library's text
operations trim and lower-case both sides unless told not to, so the by-hand versions do too; the last row is the lambda most people
write, which does less and means something slightly different:

| | Time | Against the by-hand delegate |
| --- | ---: | ---: |
| By hand, a C# lambda | 233 µs | 1.00 |
| By hand, an expression tree compiled | 267 µs | 1.14 |
| The filter, values as variables | 263 µs | 1.13 |
| The filter, values as constants | 253 µs | 1.09 |
| By hand, no trimming or lower-casing | 76 µs | 0.33 |

The filter runs as fast as the same expression tree written by hand (and about 10% slower than a C# lambda, which is the cost of
compiling a tree). Variables against constants is a difference of about 4%, which is close to the noise of the run.

## Saved filters (JSON and protobuf)

| Conditions | JSON write | JSON read | Protobuf write | Protobuf read | JSON size | Protobuf size |
| ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| 3 | 0.9 µs | 7.9 µs | 1.1 µs | 4.0 µs | 425 chars | 136 bytes |
| 20 | 6.7 µs | 61 µs | 6.3 µs | 25 µs | 3,019 chars | 989 bytes |
| 100 | 33.7 µs | 321 µs (216 KB) | 31.7 µs | 126 µs (114 KB) | 15,566 chars | 5,133 bytes |

Reading is about 3 µs for each condition in JSON, and 1.3 µs in protobuf.

## Finding an operation by name

| | Time | Alloc |
| --- | ---: | ---: |
| `Operation.Equal` (no lookup) | 3 ns | 24 B |
| `OperationRegistry.Get("Equal")` | 19 ns | 24 B |
| `TryGet` of a name that does not exist | 2.0 µs | 563 B |
| `ManipulatorRegistry.TryGet` of a name that does not exist | 2.0 µs | 563 B |
| `OperationRegistry.GetAll()` | 3.6 µs | 2.5 KB |
| The first lookup of a process | 25.9 ms (95% CI ±1.4 ms) | |

The first lookup is 25 fresh processes, one call each; it finds and loads the operation assemblies, and includes the JIT.

## The web builder

`FilterSchema.For` finds, by reflection, what of a class can be filtered. The page writes it as JSON, and a post builds it again to
check the post. Small has 7 fields (6.2 KB of JSON); large has 126 fields in 6 types (38.6 KB).

| | Small | Large |
| --- | ---: | ---: |
| `FilterSchema.For` | 11.7 µs (17.7 KB) | 125 µs (140 KB) |
| `ToJson()` | 9.5 µs (13.7 KB) | 65 µs (82 KB) |
| Both, as a page does | 22.3 µs (31.4 KB) | 200 µs (222 KB) |

Where the time of building the large schema went: filtering the list of operations for each field was about 45% of it. It now does it
once for each kind of property in a schema (`FilterSchema.For`, same session, before and after):

| | Before | After |
| --- | ---: | ---: |
| Large | 129 µs (140 KB) | 95 µs (105 KB) |
| Small | 11.7 µs (17.7 KB) | 11.0 µs |

A post (`FilterBuilderModel.Build`: schema, then the check), before and after the same change; "check alone" is the compiler with a
schema already to hand:

| Conditions | Before | After | Check alone |
| ---: | ---: | ---: | ---: |
| 3 | 138.5 µs (145 KB) | 101.9 µs (107 KB) | 3.9 µs |
| 20 | 163.3 µs (173 KB) | 126.7 µs (135 KB) | 27.9 µs |
| 100 | 277.7 µs (306 KB) | 239.3 µs (268 KB) | 134.6 µs |

A whole request for a page, through an in-memory server (no network), so it includes routing, the controller and the Razor view; the
page with no builder is the floor:

| Page | Before the schema change | After |
| --- | ---: | ---: |
| No builder | 52.8 µs (24.9 KB) | 53.5 µs (24.9 KB) |
| Builder for the small class | 134 µs (85 KB) | 144 µs (85 KB) |
| Builder for the large class | 464 µs (344 KB) | 454 µs (306 KB) |

The page times are within their noise (the large page's error is about 10%); only the allocation moved. The HTML is 398 characters
without a builder, 11,550 with the small class and 43,927 with the large.

## Not cached

Nothing is cached between calls: no schema, no schema JSON, no registry result.

- **The schema is also what a post is checked against.** It depends on the class, on every setting of `FilterBuilderOptions` (several of
  which are objects or dictionaries that can be changed after use: the labels, `OperationLabels`, `ManipulatorLabels`), on the
  operation and manipulator catalogs (which can be changed at any time, and an operation is only offered if `OperationRegistry` can find
  it, which can change as assemblies load), and on the current UI culture (a `[Display]` name can come from a resource). A stale schema
  would offer, or accept in a post, something that has been excluded since, which is a security fault, not a slow page.
- **What it would save is small beside what a page or a post does:** 95 µs and 105 KB for a class with 126 fields, 11 µs and 18 KB
  for a typical one.
- What is shared is done within one schema (the operations for each kind of property), where it cannot outlive the inputs.
