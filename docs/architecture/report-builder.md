# Report builder: design

**Status: draft for review. Nothing in this document is built.** The only code written for it is measurement code in
`tools/ExpressionBuilder.Benchmarks` (see [Measurements](#measurements)). Names of types and members below are illustrative.

A report builder lets a user pick **what to report on** (one of several classes), build the filter for it with the existing
[filter builder](../web-filter-builder.md), choose **columns** and **sorting**, and get **results** (a page of them, or a CSV
file). It is section 5 of `ai/instructions/next-steps.md`.

- [What "done" means for this document](#what-done-means-for-this-document)
- [Recommendations at a glance](#recommendations-at-a-glance)
- [What exists and what is reused](#what-exists-and-what-is-reused)
- [Where the design departs from the brief](#where-the-design-departs-from-the-brief)
- [How a request flows](#how-a-request-flows)
- [Roots: the allow-list](#roots-the-allow-list)
- [One gate: the schema built for the user](#one-gate-the-schema-built-for-the-user)
- [The posted document, version 2](#the-posted-document-version-2)
- [Building a plan without naming the class](#building-a-plan-without-naming-the-class)
- [Running a plan: the shape of a row](#running-a-plan-the-shape-of-a-row)
- [Paging, sorting and cost limits](#paging-sorting-and-cost-limits)
- [Security](#security)
- [The interface](#the-interface)
- [Export](#export)
- [Delivery in slices](#delivery-in-slices)
- [Saved reports and built-in reports](#saved-reports-and-built-in-reports)
- [Measurements](#measurements)
- [Decisions for you](#decisions-for-you)
- [Documents that change](#documents-that-change)

## What "done" means for this document

It is done when a reviewer can answer, without reading code: what will be built and in which order; what each slice delivers
on its own and how it is tested; what an application has to write to use it; what the security model is and what stays the
application's job; and what it costs, measured. Every choice carries a recommendation and the main alternative; the ones that
are the project owner's to make are marked **DECISION** and collected [at the end](#decisions-for-you).

## Recommendations at a glance

| # | Choice | Recommendation | Main alternative | Why, in a line |
| --- | --- | --- | --- | --- |
| R1 | Which classes can be reported on | An allow-list registered in code, chosen by a stable **key** (`"orders"`), never a type name | Resolve a type name or table name from the post | The browser must never name a type (`AGENTS.md`) |
| R2 | How the data is reached | The **source** (the app's scoped `IQueryable<T>`, with its tenant and soft-delete rules) is registered with the root, and cannot be omitted | The app passes a query in when it runs a plan | A root with no scope cannot exist, so no root can be run unscoped by forgetting |
| R3 | Schema | **One `FilterSchema` per root**, built for the current user on each request; changing the root is a page navigation | One schema holding every root, switched by the script | Smaller pages, per-user by construction, no change to the existing schema contract |
| R4 | Who may see which property | Rules attach to the **type's property**, not to a root, and are evaluated when the schema is built | Rules per root; separate checks for filter, columns and sort | A rule per root leaks the property through another root's navigation; one gate cannot be forgotten |
| R5 | Posted document | **One** version 2 document: `root`, `filter`, `columns`, `sort`. No page number. **Unknown members are refused** | Separate form fields; the page inside the document | A definition is what gets saved; refusing the unknown makes additions safe |
| R6 | Building without the class as a type argument | Close the generics **once, when the root is registered** | Reflection (`MakeGenericType`) on each request | Measured: see [below](#building-a-plan-without-naming-the-class); also keeps the browser from steering reflection |
| R7 | Shape of a row | A projection built as an expression tree to **`object?[]`** | A registered typed result; a generated class; a dictionary; the whole entity | Translates, selects only the chosen columns, and nothing else was measurably quicker |
| R8 | Who runs the query | The **app** (`ToListAsync`); the library only composes `IQueryable`s | The library takes a dependency on EF Core, or defines an executor interface | No EF dependency, no new abstraction until a second user wants one |
| R9 | Paging | **Offset** paging, one extra row to know there is another page, a total only on request, a maximum offset | Keyset paging | Random access and simplicity; keyset needs a total order on mixed, nullable columns |
| R10 | Column headings | The schema's labels only | Headings the user types | Less to encode, escape and store |
| R11 | Export | **CSV**, streamed, a row cap checked **before** streaming, text cells neutralised against formulas | Truncate silently or at the cap; XLSX | A partial file that looks complete is worse than a refusal |
| R12 | Grouping and totals | **Not in the first release.** The row shape will have to change for it | Build it now on `object[]` | Measured: `GroupBy` on an `object[]` key does not translate |
| R13 | Saved reports | An `IReportStore`, a document stored as plain text, **re-validated for whoever runs it**; an EF store later, as its own package | Run a stored report with its author's rights | A shared report must not be a way to borrow someone's access |
| R14 | Reordering columns | Move up / move down **buttons** | Drag and drop | **DECISION D4**: keyboard, touch and screen-reader use |
| R15 | Where the code lives | In `ExpressionBuilder.Web`, in a `Reports/` folder | A new package now | **DECISION D1** |
| R16 | When it ships | After 1.0.0, as 1.1.0 | Inside 1.0.0 | **DECISION D2** |
| R17 | A shared report that uses a column the runner may not see | Refuse the report | Leave the column out and say so | **DECISION D3** |

## What exists and what is reused

| Piece | Today | In the report builder |
| --- | --- | --- |
| `FilterSchema.For(Type, FilterBuilderOptions)` | Builds, by reflection, what of one class can be filtered, with the id of each class reached. About 95 µs for a 126-field class; not cached on purpose (`RESULTS.md`, "Not cached") | Used as it is, once per root per request. It gains one internal input: whether a property is allowed for this user |
| `FilterCompiler` | Checks a posted filter against a schema and makes statements. Reflection is already used for the property type of each statement | Used as it is. One mechanical change first: a method that takes the parsed filter node, so the report document can hand it its `filter` |
| `FilterBuilderModel.Build<T>` | The typed entry point | Unchanged. A report has its own model (`ReportBuilderModel`) with a non-generic `Build` |
| `Filter<T>.ToExpression()` | Values become variables, so a database sends parameters | Used as it is. The report adds nothing that carries a user's value into a query except through it |
| Posted document `version: 1` | `{ version, root: <group> }` | Left alone. A report uses `version: 2` (and `root` becomes a key; see [the document](#the-posted-document-version-2)) |
| The script | Builds the filter UI from a schema and templates; `FilterBuilder.attach`, `.json`, `.load`, `eb:change` | The report script **wraps** one filter builder through that API; it does not fork it |
| `FilterBuilderIgnore`, `Exclude<T>` | Hide a property from everyone | Still hide a property from everyone, in every root |

## Where the design departs from the brief

Section 5 of `next-steps.md` set constraints and a suggested direction. These are the places this design differs, so they can be
argued with:

1. **One schema per root, not one schema with several roots (R3).** The brief says "extend [the schema] to several roots" and
   changing the root resets the filter. A single schema holding every root means every page carries every root's fields (for a
   126-field class that is 38.6 KB of JSON; five such roots about 190 KB), is built for every root whether or not it is used, and
   changes the schema contract the script's fixtures pin. Because the filter, the columns and the sort all reset when the root
   changes anyway, a round trip costs the user nothing they would keep. The alternative is right if the product needs no-reload
   switching; it can be added later by sending several schemas, since each is already complete.
2. **Buttons, not drag and drop, to reorder columns (R14).** The brief says "drag to order". Drag and drop has no keyboard
   equivalent in the platform, is poor on touch, and the project's browser tests hold the filter builder to keyboard-only
   operation and axe. Move up / move down buttons are the platform (`AGENTS.md`: prefer the platform). **DECISION D4.**
3. **The library composes queries but does not run them (R8).** The brief says "gets results". Running needs `ToListAsync`, which
   is EF Core's. Composing `IQueryable<object?[]>` and leaving three lines of materialising to the app keeps EF Core out of the
   package and out of its version policy. If every app writes the same three lines, add a convenience package then (KISS: the
   second use, not the first).
4. **The first release stops at columns, sorting and paging (R12).** Grouping stays in the brief as "only if still wanted"; the
   measurements show it cannot use the row shape recommended here, which is a reason to wait and see.
5. **The long-lived branch (house rule: trunk-based, review continuously).** `refactor/repo-restructure-and-filter-builder` holds
   sections 1 to 4 and has not been merged; its `browsers` CI job has never run. Do not add this on top of it. Merge it first,
   then branch each slice from `main` (see [delivery](#delivery-in-slices)).
6. **The version.** `1.0.0-rc.1` was published on 2024-09-19 (tag `v1.0.0-rc.1`, NuGet). The branch still says `rc.1`, and several
   documents say nothing has been released. The next candidate is `1.0.0-rc.2`; do that bump in its own commit before anything else.

## How a request flows

```text
GET  /reports?root=orders
  roots the user may use ─────────────────────────────▶ links: Orders · Customers   (the picker)
  ReportSchema for "orders", for this user ───────────▶ filter schema + columns + limits, in the page

POST /reports   (the definition, version 2)
  1. size, JSON, version, unknown members            refuse
  2. root key → the registered root                  not there / not allowed: one message
  3. schema for this root and this user              ◀── the only gate: filter, columns and sort all go through it
  4. filter  → statements (FilterCompiler)
     columns → fields of the schema that can be shown      limits: count, duplicates
     sort    → fields of the schema that can be ordered
  => ReportPlan: immutable, nothing left in it that is text from the browser

RUN (plan, page, size)                        in the root's closed generic code
  q = root.Source(context)                    the app's scope: tenant, soft-delete, row rules
  q = q.Where(userFilter)                     a separate Where: the user's Or can never reach the scope
  q = q.OrderBy(sort…).ThenBy(root.Key)       the key makes pages stable
  q = q.Skip(offset).Take(size + 1)
  rows  = q.Select(x => new object?[] { … })  only the chosen columns
  => ReportQuery { Page, Matching, Export }   IQueryables; the app materialises them
```

## Roots: the allow-list

```csharp
// Illustrative. Registered once, at start-up.
services.AddReportBuilder(reports =>
{
    reports.AddRoot<Order>("orders", "Orders")
        .Source(ctx => ctx.Services.GetRequiredService<ShopDb>().Orders
            .AsNoTracking()
            .Where(o => o.TenantId == ctx.User.GetTenantId()))   // the app's own, mandatory rules
        .Key(o => o.Id)                                          // unique; orders pages
        .Authorise(ctx => ctx.User.IsInRole("Sales"))            // may this user use this root at all
        .MaxPageSize(100);

    reports.AddRoot<Customer>("customers", "Customers")
        .Source(ctx => ctx.Services.GetRequiredService<ShopDb>().Customers.AsNoTracking())
        .Key(c => c.Id);

    // Rules follow the type, wherever it is reached from (R4)
    reports.Restrict<Customer>(c => c.CreditLimit, ctx => ctx.User.IsInRole("Finance"));   // one property
    reports.Restrict<Supplier>(ctx => ctx.User.IsInRole("Buyers"));                        // a whole type, and every navigation to it
    reports.Exclude<Order>(o => o.InternalNotes);                                           // nobody, in every root
});
```

- **The key is the contract.** `[a-z][a-z0-9-]{0,39}`, case-sensitive, unique, checked when registering (a duplicate or a
  malformed key throws at start-up, not on the first request). It appears in URLs and, later, in stored reports, so it is
  permanent: renaming a key breaks the reports saved under it. The CLR type is held by the registration and is never sent to the
  browser or read from it.
- **`Source` is required (R2).** `AddRoot` returns a builder that has no way to produce a root without one, and `Key`, too.
  *Alternative:* the app passes the query in when it runs a plan. That puts a `switch` on the root key in the app, which is exactly
  where one root's tenant filter gets forgotten. The cost of the recommendation is that the source is a factory
  (`ctx => IQueryable<T>`) because a `DbContext` is scoped to the request: the `ReportContext` carries the user and the request's
  services.
- **Mandatory filters come first and cannot be widened.** The user's filter is applied as its own `Where` after the source, so
  `scope AND (a OR b)`. A test pins this with a filter that is true for every row.
- **`Authorise` decides whether a user may use the root at all.** A root the user may not use is absent from the picker and is
  refused with the same message as a key that does not exist (no way to learn which roots exist).
- **`Key` is a tie-break.** Every ordering ends with it so that two pages never overlap or skip a row. It should be a surrogate
  (an id), because it orders rows within ties even when it is not a column the user can see.

## One gate: the schema built for the user

The web builder's central idea is *one schema, two readers*: the schema that draws the form is the one that checks the post. The
report builder keeps that and uses it as its security model: **filter, columns and sort all resolve a property path through the
schema built for this user and this root.** There is no second list to keep in step and no separate check to forget.

```text
FilterSchema.For(root type, options, allowed: property => rule is satisfied for this user)
```

- A property the user may not see is **not in the schema**. A post that names it in a filter, a column or a sort gets the message
  a property that does not exist gets: `"X" is not a property that can be filtered on.` (the existing wording for an unknown or
  ignored property; the new messages for columns and sort say the same kind of thing). Hidden and missing cannot be told apart.
- **Rules follow the type (R4).** `Restrict<Customer>(c => c.CreditLimit, …)` applies wherever `Customer` is reached: as the root
  `customers`, or as `Customer.CreditLimit` from `orders`. A rule attached to a root would let `orders` show what `customers`
  hides. `[FilterBuilderIgnore]` and `Exclude<T>` still win, and apply to everyone.
- **The rule is a synchronous predicate over the context**, evaluated once per property when the schema is built. A rule on a
  whole type hides the type's properties and every navigation that leads to it. Keep it to
  claims and roles; do not call a database from it. Its cost, measured on its own, is in [Measurements](#measurements).
- The schema is built per request and per user and is **not cached** (`RESULTS.md`, "Not cached": a stale schema is a security
  fault, not a slow page). A per-user schema makes that more true, not less.
- The page carries the schema of **one root** (R3). The columns and sort lists are derived from the same fields (not related
  objects or collections, which are not a value to show or to order by), so they are not a second source of truth.

## The posted document, version 2

```json
{
  "version": 2,
  "root": "orders",
  "filter": { "type": "group", "connector": "And", "items": [
    { "type": "condition", "field": "Placed", "operation": "GreaterThanOrEqual", "values": ["2026-01-01"] }
  ] },
  "columns": [ { "field": "Reference" }, { "field": "Customer.Name" }, { "field": "Total" } ],
  "sort":    [ { "field": "Placed", "direction": "Desc" } ]
}
```

- **`filter` is exactly the version 1 `root` node**, so the existing compiler reads it unchanged. `root` changes meaning (it was
  the group; it is now the key), which is the one reason this is version 2 and not an addition to version 1. The standalone
  filter builder keeps reading and writing version 1.
- **Plain text, validated against the schema.** A `field` is looked up, never used to reach into the class; `direction` is `Asc`
  or `Desc`; there is no member that holds a type name, a method name, an expression or SQL.
- **No page number, no page size, no format.** Those belong to *running* a report, not to what the report *is*. The definition is
  what gets saved and shared; the page comes from the request and is clamped by the root's limits.
- **Unknown members are refused (R5)**, where the version 1 reader ignores them. A stored or shared report that carried a member
  an older server does not understand must fail, not run as something it is not. That also makes additions safe: `columns` and
  `sort` can arrive in a later slice than `root` and `filter` without a version bump, because a server without them refuses a
  document with them. A change in what an existing member *means* bumps the version.
- **Headings are the schema's labels (R10).** A user-typed heading is text from a browser that ends up in HTML and in a CSV
  header (see [Export](#export)). It can be added with saved reports if it is wanted.
- **Upgrade path:** a stored document is read through `ReportDocument.Upgrade(json)`, which is a chain of small, pure functions
  from each older version. Version 2 is the first, so the chain starts empty; the seam exists so storing documents does not
  freeze the format.
- Limits reuse `FilterBuilderOptions` (`MaxPayloadLength`, `MaxConditions`, depth, values) and add the report's (below).

## Building a plan without naming the class

A `Filter<T>` needs `T`, and a report's `T` is chosen at run time. The question is where the generic code runs.

**Recommendation (R6): close the generics where the class is known.** `AddRoot<T>` is called with the type, in the application's
code. It creates a `ReportRoot<T>` that implements a non-generic `ReportRoot` base: the generic methods (`new Filter<T>`,
`ToExpression`, `Where`, `OrderBy`, `Select`) are ordinary code inside it, and everything else sees `ReportRoot`. The posted
text only ever selects *which* registered object to use.

**Alternative:** keep `FilterBuilderModel`-style code and call `typeof(Filter<>).MakeGenericType(type)`,
`Activator.CreateInstance` and `MakeGenericMethod(type).Invoke` on each request. It works, and `FilterCompiler` already does the
same for each statement.

The cost of the two, measured on the same checked filter (see [Measurements](#measurements)): reflection on each request added
about 2 µs for a 3-condition filter (13.1 to 15.2 µs) and nothing that can be told apart from noise for 20 and 100 conditions; a
whole post, schema and check included, differed by 2% or less (281 and 282 µs, 456 and 463 µs, 1,282 and 1,305 µs).

So the choice is not about speed; it is about what the browser can influence (nothing, in the recommended one) and about
`IFilter.ToExpression<T>()` and `IFilter.Items`, which the alternative would call through reflection. No change to the `Api`
interfaces is needed. (Adding a non-generic `ToExpression(Type)` to `IFilter` would break anyone who implements it.)

## Running a plan: the shape of a row

A plan names columns by path. Running it means a `Select` whose members are known only at run time.

| Option | How it works | Result |
| --- | --- | --- |
| **O1. `object?[]` by expression tree** (recommended) | `x => new object?[] { x.Reference, x.Customer.Name, … }`, built per request from the plan | Translates; selects only the chosen columns; the headings are held once, by the plan |
| O2. A dictionary per row | `new Dictionary<string, object?> { … }`, or O1 and then a dictionary | Translates both ways; about 20% more memory a row (125 KB against 104 KB for a page of six columns) and no speed difference beyond noise. Every row repeats the names the plan already holds |
| O3. A registered typed result | The app writes `OrderRow` and the root's `Select`; the user's column choice hides columns | The query is always one shape; every column is always read; per-user columns need conditionals in the projection; a column the user did not choose is still fetched |
| O4. A generated class | `Reflection.Emit` a class for each set of columns, then a `MemberInit` | Translates; making the class and a way to read it took 3.3 to 5.3 ms for each new set of columns (an `object[]` projection took 30 to 60 µs), and it was not faster once built. It also keeps a type for every set a user tries |
| O5. The whole entity, then pick | Read `x`, then read columns from it with compiled getters | About twice as slow a request and 2.4 times the memory (247 KB against 104 KB for six columns), and it reads columns nobody asked for, including ones the user may not see |

**Recommendation: O1.** It is the only one that needs no per-set machinery, adds nothing to keep, reads only what was asked
for, and was not measurably slower than the compile-time shapes it stands in for. The numbers are under
[Measurements](#measurements); the facts the choice rests on:

- **It translates** in EF Core 10 (SQLite): `Select(x => new object[] { … })` after `Where`, `OrderBy`, `Skip` and `Take`, with the
  filter's values still parameters, only the chosen columns in the `SELECT`, and a join only for a related object that a chosen
  column reaches.
- **The null guard is for objects in memory, not for the database.** `x.Customer.Name` throws `NullReferenceException` over a list
  when the customer is missing; `x.Customer == null ? null : x.Customer.Name` does not, and EF Core produced identical SQL with and
  without it. The projection therefore always guards: one code path, a microsecond or two to build, and no change to the SQL. The filter side already does this
  (`ExpressionExtensions.GetExpression`).
- **A new set of columns costs a translation** (about 6 ms measured here) and two entries in EF's query cache. This is true of every
  option and is the real cost of letting users choose columns: a report that is run again with other values is the cheap path
  (one cached plan); a report with a new column set is not. EF's cache is bounded (10,240 units, 5% dropped when full, as the earlier experiment made its own), so
  a user cycling through column sets cannot grow it without limit, but can push other queries out. That is what the column and
  request limits are for.
- **EF Core already turns `Skip` and `Take` constants into parameters**: 400 requests over 40 different pages added no cache
  entries as constants or as variables. No holder is needed for the page, unlike for the filter's values.
- **Grouping cannot use O1.** `GroupBy(x => new object[] { … })` does not translate (EF cannot compare arrays). A `ValueTuple` key,
  a generated class and an anonymous type did (tested with two columns). Grouping is therefore a different projection design, not
  an extension of this one, which is why it is deferred (R12).

Rows come back as `object?[]` in the order of `plan.Columns`. A `ReportFormatter` turns a cell into text by the column's kind
(an enum member's label, `Yes`/`No` from `FilterBuilderLabels`, dates by culture for the page and ISO for the file).

**Who materialises (R8).** The library returns a `ReportQuery` of ordinary `IQueryable`s; the app runs them:

```csharp
// Illustrative
var query = plan.Compose(context, new ReportRun(page: 2, size: 50));
using var cts = query.LinkToken(HttpContext.RequestAborted);          // applies the time limit

var rows  = await query.Page.ToListAsync(cts.Token);                   // size + 1 rows
var total = run.IncludeTotal ? await query.Matching.CountAsync(cts.Token) : (int?)null;
var page  = query.ToPage(rows, total);                                 // trims the extra row; sets HasMore
```

*Alternative:* an executor interface the app registers (`UseEntityFramework()`), so `await plan.RunAsync(…)` is one line. It is a
good convenience and a bad first abstraction; it can be added as a small package without changing anything above.

## Paging, sorting and cost limits

| Limit | Default | Why |
| --- | ---: | --- |
| Columns in a report | 25 | Rows are fetched whole; width is cost |
| Sort columns | 3 | |
| Conditions in the filter | 30 (the filter builder's own default is 100) | A report is run more often than a search form is |
| Page size | 50 (maximum 200; a root can lower it) | |
| Maximum offset | 10,000 rows | `OFFSET` makes the database read and discard; beyond this, narrow the filter or export |
| Time limit for a page | 30 s | The app passes it as a cancellation token and should also set the command timeout |
| Rows in an export | 50,000 (a root can change it) | |
| Time limit for an export | 120 s | |
| `RequireFilter` (per root, off) | off | For a very large table: a report must have at least one condition |

These are proposals to confirm, not measurements; each is a setting. The time limits are only honoured if the app passes the
token to the provider and sets `CommandTimeout`; the documentation says so.

- **Offset paging, one extra row (R9).** `Take(size + 1)` says whether there is a next page without a `COUNT`. A total is a second
  query over the same filter and scope, run only if asked. *Alternative:* keyset paging avoids the deep-offset cost but needs a
  strict total order over user-chosen, nullable, mixed-direction columns and cannot jump to page 7. Revisit if a root has millions
  of rows.
- **Ordering is stable.** Every `OrderBy` ends with the root's `Key`. Without it, two rows with equal sort values can swap between
  pages. Sorting by text uses the database's collation, nulls sort where the provider puts them, and an enum sorts by its number,
  not its label; the guide says so.
- **A column may be sorted if the user may see it, whether or not it is shown.** (Sorting by a column the user can see cannot
  reveal anything they could not read.) *Alternative:* only shown columns. Simpler to explain, and it makes no difference to what can be learned.
- **Result size.** The page and export caps bound rows; the HTML table shortens long text (a `Notes` column of megabytes would
  otherwise be fetched and rendered). Roots with very large text columns should leave them out of the schema.

## Security

The posted report is untrusted input, like the posted filter. The model: *the browser names things by key and path; the server
decides, for this user, what each key and path means.*

| Threat | Control | Pinned by a test that… |
| --- | --- | --- |
| The browser names a type, method or table | Roots by key; fields by schema path; no member can hold a type or SQL; unknown members refused | posts a type name in each member and a `$type` |
| A root the user may not use | `Authorise`; same refusal as an unknown key | compares the two responses |
| Filtering on a column the user cannot see (leaks its values) | The schema is per user; the filter resolves through it | tries every restricted property in a condition |
| **Sorting on** a hidden column (leaks its order) or **choosing** it | The same schema | tries every restricted property as a sort and as a column |
| **Aggregating** a hidden column (later; `Sum(Salary)` leaks) | Aggregates resolve through the same schema | (slice e) |
| A user widens the app's scope (`tenant = 1 OR true`) | The source is the root's; the user's filter is a separate `Where` | runs a true-for-all filter and counts only the tenant's rows, for the page, the **count** and the export |
| A hidden column is read though not shown | The projection selects only the chosen columns | asserts the SQL's `SELECT` list |
| Related rows from another scope (`Order.Customer` in another tenant) | **The app's job**: global query filters on the related entity. The documentation says so, and the sample does it | the sample's test has a customer from another tenant |
| A property rule on one root bypassed through another | Rules follow the type | a hidden property is absent from every root that reaches it |
| Resource exhaustion | The limits above; `RequireFilter`; the filter builder's own limits | posts one over each limit |
| A page of enormous text | Truncation in the table; leave such columns out | |
| **CSV injection** in an export (`=HYPERLINK(…)`) | Text cells that start with `=` `+` `-` `@` tab or CR are prefixed with `'`; only text cells, never numbers | a golden file with each trigger |
| Script injection through a heading or cell | Razor encodes; the schema is JSON in a non-executing block with `<` escaped, as now; headings are ours | renders `<script>` as a value |
| A stored report used to borrow rights | It is parsed and checked again, for the **runner**, on every run | runs a Finance report as a user who is not in Finance |
| A stored report silently running as something else | An unknown member or version is refused; a refused condition refuses the report (a dropped condition widens the result) | |
| Learning what is hidden from an error message | One message for hidden, missing and not allowed; the detail goes to the log with the report id | compares the messages |
| Cross-site request | POSTs carry the anti-forgery token, as the filter builder's do. Running a saved report by id is a GET and read-only | |
| Results leaking between users through a cache | Documentation: key any cache by user and scope; the library caches nothing | |
| Unaudited exports | A structured log entry per run (root, user id, hash of the definition, rows, duration — never values) | |

**Reduce the blast radius at the database:** the report's connection should be a read-only login (or a replica). A bug in the
projection or the scope then cannot write. This is the app's job and the guide says it first.

## The interface

The page for a root, from top to bottom, in the same markup contract as the filter builder (`data-eb-*` attributes and
`<template>`s that the view owns and the script clones):

1. **Root picker.** A list of links (`<nav aria-label="Report on"><ul>`, the current one `aria-current="page"`) to `?root=key`,
   showing only the roots the user may use. It is a GET: no script, no form (a `<select>` inside the user's form would submit the
   whole filter into the URL). If the filter has conditions, the script asks before leaving.
2. **The filter builder**, unchanged, for the chosen root's schema.
3. **Columns.** A `<select>` of the fields that can be shown (grouped like the filter builder's field list), an *Add column*
   button, and an ordered list of the chosen ones, each with move up, move down and remove buttons (R14). Focus goes to a sensible
   neighbour after every removal or move: this is where the filter builder had bugs (focus lost after removing a condition), and the
   keyboard tests will cover it from the start.
4. **Sort.** Up to three rows of field, direction and remove, with *Add sort*.
5. **Run.** *Run*, *Download CSV*, and the results table with a pager (buttons that post the definition again with a page number,
   so no state sits in the URL).

New templates: `column`, `sort`. All the text is in a `ReportBuilderLabels` beside `FilterBuilderLabels`, translated the same way,
and a run announces its result (`role="status"`: "50 rows"). The report script (`report-builder.js`, a second bundle that imports the filter builder's
module, so a page with only a filter builder does not load it) assembles one hidden input from the filter builder's `json`, the
columns and the sort. The Razor view can be replaced as the filter builder's can. It works under the same strict CSP.

## Export

- CSV (RFC 4180), UTF-8 **with** a byte-order mark by default (the people who export a report open it in Excel), switchable.
- Dates are ISO 8601, numbers invariant, enums are their labels, booleans `true`/`false`, so the file does not depend on the
  server's culture.
- Text cells are neutralised against formulas (see Security). Numbers are written as numbers.
- It streams: `Export` is an `IQueryable` the app turns into an `IAsyncEnumerable` (`AsAsyncEnumerable`), and the writer takes
  that, so memory does not grow with the rows.
- **The row cap is checked before streaming (R11).** A `COUNT` over the same filter and scope runs first; if it is over the cap
  the response is a refusal that says to narrow the filter. Truncating at the cap would hand over a file that looks complete.
  *Alternative:* stream to the cap and append a marker row; it saves a query and makes a worse file.
- The file name is made from the root key and the date, never from user text. `Content-Disposition: attachment` and
  `X-Content-Type-Options: nosniff`.
- Not in the first release: Excel files (a dependency), JSON (a few lines in the app from `object?[]`).

## Delivery in slices

**Before the first slice** (each its own commit or branch, none is feature work):

1. Merge the current branch to `main` once the first CI run is green (it has not run the `browsers` job yet); branch each slice
   from `main`.
2. Bump to `1.0.0-rc.2`: `Directory.Build.props`, and the documents that say rc.1 or "nothing released".
3. Mechanical: split `FilterCompiler.Compile(string)` into reading and `Compile(FilterNode)`; no behaviour change.

Each slice stands on its own: after it, the library does something useful, the documentation says what, and `main` can be
released. They are released behind documentation, not a flag.

### Slice a. Several roots, the user's schema, a non-generic build

*Delivers:* `AddReportBuilder`, `AddRoot<T>(key, label)` with `Source`, `Key`, `Authorise`, `Restrict`, `Exclude`; the roots a
user may use; `FilterSchema` built for a user and a root; `ReportBuilderModel.Build(context)` that returns a `ReportPlan` holding
the checked filter; `plan.Source(context)`, the scoped and filtered query for the root as an untyped `IQueryable`. Useful alone:
a page that filters one of several classes and shows the entities.

*Tests:*

- Registration: duplicate key, malformed key, missing `Source`, missing `Key`, a non-class: each throws, at start-up, with a
  message that says what to do.
- Roots: a user sees only the roots they may use; **an unknown key and a root the user may not use produce the same error**.
- The schema per user: for every root × user × field, *offered if and only if allowed* (a matrix, like
  `OfferedOperationsWork`); a type rule reached through a navigation; `FilterBuilderIgnore` beats a permission; the JSON for a
  root with no rules is identical to `FilterSchema.ToJson()` (the existing fixture still passes).
- The document: wrong version, no root, unknown member at each level, oversize, deeply nested, `$type`.
- Non-generic build: two roots in one process; each plan's expression has its own root type; each result matches LINQ over the
  same data (the oracle approach of `EfCoreTests`).
- **Scope:** the app's scope and a filter that is true for every row return only the scope's rows, in memory and through SQLite.
- EF Core: the filter's values are parameters, and different values share one cache entry (as `EfCoreTests` already proves for
  one class).
- Public API check (`docs/standards/testing.md`): additions only; list them in the changelog.

### Slice b. The root picker

*Delivers:* `<report-builder>` renders the picker and the chosen root's filter builder; `?root=` handled; the script confirms
before discarding conditions; the sample shows several classes. Useful alone: a multi-class search page.

*Tests:* the tag helper lists only the allowed roots, marks the current one, and handles an unknown or disallowed `?root=` by
showing the first allowed root and no error that distinguishes the two; client tests for the confirm (model and DOM, against the
real view); the sample app over HTTP; **browser tests** (Chromium, Firefox, WebKit): choose a root with the keyboard, the
builder shows that root's fields, the filter is reset, axe in light and dark; the strict-CSP published-app test still passes.

### Slice c. Columns and sort

*Delivers:* `columns` and `sort` in the document; their validation; the column and sort UI; `plan.Columns`, `plan.Sort`. Useful
alone: an app gets a **checked, ordered list of columns and sort keys** it can use with its own projection.

*Tests:* a hidden property refused as a column and as a sort with the same message as a missing one; non-value fields (related
objects, collections) refused; duplicates; limits (columns, sorts); order preserved; the matrix extended to columns and sort; the
fixtures in both directions (`client/test/fixtures/posted/` gains report documents, and a .NET test builds each);
client model and DOM tests; browser tests for the whole flow with the keyboard alone, **focus after every add, move and remove**,
names and roles, axe.

### Slice d. Execution and export, in the sample app

*Delivers:* `plan.Compose(context, run)` returning `ReportQuery`; the projection, ordering, tie-break, paging and limits; the
formatter; the results table and pager; the CSV writer; the sample app on EF Core with SQLite and several roots (a tenant scope,
a soft-delete global filter, a Finance-only column, a customer from another tenant).

*Tests:*

- An **oracle**: generated definitions (seeded; many combinations of columns, sorts, filters and pages) run through SQLite and
  compared with LINQ over the same objects.
- SQL assertions: the `SELECT` list is exactly the chosen columns (a hidden column is never read); values and page numbers are
  parameters; different values and pages share one cache entry; a new column set adds two.
- Paging: no overlap or gap across pages with many ties; `HasMore`; the maximum offset; the total is scoped.
- The projection over objects in memory with a missing related object (the case that throws without the guard).
- Limits: each refused; the time limit cancels (a provider that honours the token).
- Export: a golden file with commas, quotes, newlines, Unicode, a BOM, and every formula trigger; the cap refuses before
  streaming; streaming does not buffer (memory stays flat for a large export).
- Browser tests: run a report; download the CSV and compare; the published-app test under a strict CSP.
- Benchmarks kept in `tools/ExpressionBuilder.Benchmarks` (the ones used for this document).

### Slice e. Grouping and totals (only if still wanted)

Not designed here beyond the constraints it must meet. The row shape changes (a `ValueTuple` or generated key; see above).
Aggregates are an allow-list by kind (count for all; sum and average for numbers; minimum and maximum for numbers and dates) and
go through the same schema, because `Sum(Salary)` leaks Salary as surely as a filter does. A group count is limited. Tests: an
oracle, and every restricted property refused as a group key and as an aggregate.

### Slice f. Saved reports and built-in reports

See the next section.

## Saved reports and built-in reports

*From the brief:* any report can be saved as a "built-in" report for the application (a dashboard, a "Your reports" page), with a
name, stored, and optionally **global** (shown to everyone).

**What is stored: the version 2 document, as text** (R13). Not the library's own JSON, which names .NET types, and not a
compiled form. It is already validated on the way in and is the same thing the browser posts.

```text
SavedReport
  Id               guid
  Name             text(100)     encoded wherever shown
  Description      text(500)
  RootKey          text(40)      copied from the document, for listing and for finding reports a removed root orphaned
  Definition       text          the document; capped at MaxPayloadLength
  DefinitionVersion int          so out-of-date documents can be found and upgraded
  Scope            Private | Global       (Shared with roles: later)
  OwnerId          text          from the claim the app names
  TenantId         text?         the store is itself a row-scoped data set
  Created, Updated datetimeoffset;  UpdatedBy text
  Revision         int           optimistic concurrency
```

**The store.** `IReportStore`: list for a context, get, save (with the expected revision), delete. Every method takes the
`ReportContext` and **the store applies its own row rules**: a user lists their own, the global ones, and nobody else's, within
their tenant. Implementations: an in-memory one (tests and the sample), and an EF Core one as its own package
(`…Reports.EntityFrameworkCore`: the entity, `modelBuilder.ApplyReportStore()`; the app owns its migrations). A custom store is
the same interface. *Alternative:* no interface, only an EF entity; it ties every app to EF Core and makes the in-memory tests
different from production.

**Running a saved report:** load the document; **parse and check it again, for the user who is running it, against today's
schema**; run it. A report is never run with its author's rights, and a stored document is not trusted merely because it was
once valid (R13): the schema may have changed, the user may be someone else, and the row in the database is writable by more
people than the page.

- A condition that cannot be satisfied (a property removed, or one the runner may not see) **refuses the whole report**. Leaving
  it out would widen the result, which is the wrong direction to fail in.
- A column that cannot be shown: refuse (recommended) or leave it out with a visible note. **DECISION D3.** Refusing means the
  author of a global report must stick to columns everyone has; leaving out is friendlier and quietly changes what the report
  says.
- The message to the runner is the single generic one; the detail (which property, which report) goes to the log.

**Saving.** A user may save privately. Choosing *Global* needs a permission the app defines
(`reports.CanPublishGlobal(ctx => …)`), because a global report is visible to everyone and a definition can contain literal values
(a filter on someone's name). The UI says so. Limits: reports per user, name length, definition size.

**Built-in reports in code.** `reports.AddBuiltIn("orders-this-month", "Orders this month", json)`: read-only, global (or with
`Authorise`), validated **at start-up** with a context that has every permission so a typo is found on deploy, then validated
again for each runner. They sit in the same list as saved ones.

**Showing them.** A "Your reports" page is a list from `IReportStore` plus the built-ins, each linking to
`/reports/run/{id}`. A dashboard tile is a view component (`<report-table report="orders-this-month" />`) that runs a saved or
built-in report and renders the first page; it is subject to the same checks. Editing loads a stored document into the builder
(like `FromFilter`, it refuses rather than show something that means something else).

*Storage needs, in short:* one table (above) and one index each on `(OwnerId)` and `(Scope, TenantId)`; no stored results; nothing
secret. Slice f depends on c and d.

## Measurements

Costs were measured with throwaway code in `tools/ExpressionBuilder.Benchmarks` before any recommendation here relied on them.

### Machine and method

| | |
| --- | --- |
| Date | 2026-10-15 |
| CPU | AMD Ryzen 5 3600X, 6 cores / 12 threads |
| OS | Windows 11 (10.0.26200) |
| .NET | SDK 10.0.402, runtime 10.0.13 (X64 RyuJIT, x86-64-v3) |
| Libraries | Expression Builder 1.0.0-rc.1 (the branch), BenchmarkDotNet 0.15.8 (default job), Entity Framework Core 10.0.13 with SQLite in memory |

- **This is not the machine of `tools/ExpressionBuilder.Benchmarks/RESULTS.md`** (an i5-12600KF). Do not mix the two: building the
  126-field schema took 222 µs here and 95 µs there. Every comparison below is within this one session.
- **The machine was not quiet**: a game and a virtual machine were running throughout. BenchmarkDotNet's error bars measure its
  own process, not that. The EF experiment therefore prints the lowest and highest of its runs, and the ranges are wide. Only
  differences much larger than the range are claimed here; where a number is not affected by load (memory allocated) it is quoted
  in preference. A second run of the experiment agreed with the first on every ordering below. **Re-run on a quiet machine before
  relying on any ratio near 1.**
- The code is throwaway: `ReportSpike.cs` (the projection, ordering and paging, built the way this design would), `ReportBenchmarks.cs`
  (BenchmarkDotNet), `ReportExperiment.cs` (`-- report`, through EF Core) and `ReportModels.cs`, all in `tools/ExpressionBuilder.Benchmarks`.
  None of it is library code, and the numbers cover the *shape* of the work, not an implementation.

```powershell
cd tools/ExpressionBuilder.Benchmarks
dotnet run -c Release -- report                                  # the EF experiment (a few minutes)
dotnet run -c Release -- --filter "*NonGenericBuild*" "*ReportCompose*" "*ResolveColumns*" "*AuthorisationHook*" --buildTimeout 1500
```

### 1. A class chosen at run time

The same checked filter (`ICollection<IFilterItem>`) turned into an expression. "Typed" is `new Filter<Large> { Items = … }`;
"closed when registered" is a delegate made once; "reflection" finds the generic type and method on every request. "Whole post"
adds the schema and the check.

| Conditions | Typed | Closed when registered | Reflection each request | Whole post, typed | Whole post, reflection |
| ---: | ---: | ---: | ---: | ---: | ---: |
| 3 | 13.1 µs (10.8 KB) | 13.4 µs (10.8 KB) | 15.2 µs (11.1 KB) | 281 µs (117 KB) | 282 µs (118 KB) |
| 20 | 85.4 µs (52.0 KB) | 83.4 µs (51.7 KB) | 86.1 µs (51.7 KB) | 456 µs (189 KB) | 463 µs (190 KB) |
| 100 | 496 µs (297 KB) | 496 µs (298 KB) | 489 µs (297 KB) | 1,282 µs (574 KB) | 1,305 µs (577 KB) |

Reflection is not measurably dearer. Choose by what the browser can influence, not by speed (R6).

### 2. Composing the query

Everything the library would do to a root's query for one request, without running it (over an empty list): the user's filter,
the order, the tie-break, the page, and the `object[]` projection.

| Columns | Sorts | Projection alone | Projection, null-guarded | The whole query |
| ---: | ---: | ---: | ---: | ---: |
| 5 | 1 | 2.4 µs (1.6 KB) | 2.9 µs (1.9 KB) | 33 µs (20.7 KB) |
| 5 | 3 | 2.4 µs (1.6 KB) | 2.8 µs (1.8 KB) | 43 µs (28.2 KB) |
| 20 | 1 | 8.7 µs (5.1 KB) | 11.1 µs (6.1 KB) | 40 µs (24.1 KB) |
| 20 | 3 | 8.8 µs (5.1 KB) | 10.3 µs (5.9 KB) | 52 µs (32.1 KB) |

- Building the projection is a few microseconds; the null guard adds 0.5 to 2.4 µs. Most of the rest of the whole query is
  probably the lookup of `Queryable` methods by name that `Expression.Call` does each time (not measured separately); an
  implementation would look them up once, but the figure to budget is this one.
- Beside a request through EF Core that takes at least 0.9 ms (below), composing is 2 to 6%.
- Resolving the columns and sort keys in the schema (the same lookup a filter's fields use): 0.9 µs for 5 paths, 3.3 µs for 20 and
  3.8 µs for 23, taken from the end of a 126-field class (the slowest place; the lookup is a linear scan).
- The per-user hook that decides whether a property is allowed, measured on its own (a dictionary lookup of the rule and a set
  lookup of the permission) for the 94 properties of the classes in the 126-field schema: **1.3 µs**, against 222 µs and 105 KB
  to build that schema (0.6%). Not measured inside the schema builder, because it is not written.

### 3. A request through EF Core

A page of 50 from 10,000 orders (35 columns, a related customer, an owned address), with a new `DbContext` for each request and
a filter whose values differ each time, ordered by `Placed` descending then `Id`, from an index that matches that order. Each shape
ends as a list of rows of cells. 300 requests; median of 9 runs; the shapes in a different order each run. Allocation is per request.

| Columns | How the rows are shaped | Per request | Lowest to highest of the runs | Allocated |
| ---: | --- | ---: | ---: | ---: |
| 1 | `object[]` (the floor: almost nothing to shape) | 900 µs | 622 to 1,898 µs | 86 KB |
| 6 | Whole entity, then read the columns | 2,793 µs | 2,445 to 6,664 µs | 247 KB |
| 6 | Anonymous type (compile time) | 1,816 µs | 915 to 3,927 µs | 107 KB |
| 6 | Registered result class (compile time) | 1,449 µs | 1,071 to 2,699 µs | 107 KB |
| 6 | **`object[]` built per request** | 1,551 µs | 885 to 3,144 µs | 104 KB |
| 6 | `object[]`, null-guarded | 2,363 µs | 979 to 13,194 µs | 105 KB |
| 6 | `object[]`, then a dictionary per row | 1,779 µs | 1,049 to 3,762 µs | 125 KB |
| 6 | Generated class, made once per set | 1,330 µs | 1,033 to 4,564 µs | 113 KB |
| 20 | Whole entity, then read the columns | 3,185 µs | 2,235 to 22,817 µs | 261 KB |
| 20 | **`object[]` built per request** | 2,486 µs | 1,389 to 5,197 µs | 139 KB |
| 20 | `object[]`, null-guarded | 2,381 µs | 1,464 to 5,812 µs | 142 KB |
| 20 | `object[]`, then a dictionary per row | 2,346 µs | 1,669 to 15,222 µs | 188 KB |
| 20 | Generated class, made once per set | 2,696 µs | 1,706 to 3,069 µs | 153 KB |

What this supports, and what it does not:

- **Supported:** the whole entity is the dearest shape in time (about twice) and in memory (2.4 times at six columns, 1.9 at twenty),
  and it fetches what was not asked for. The compile-time shapes and every run-time shape sit in the same band, and their
  memory is within 10% of each other (dictionaries +20%). Building the `object[]` projection on every request is not what a
  request costs.
- **Not supported:** any ordering inside that band. The anonymous type, the registered class and `object[]` swap places between
  runs, and the ranges overlap. Do not read "`object[]` is faster than the anonymous type".
- **The database is the cost when there is no index.** The same experiment with no index for the order (every request sorts the
  10,000 rows) took 7.0 ms for one column and 9 to 11 ms for six in every shape except the whole entity (14.6 ms); the shapes
  cannot be told apart at all. A report on a large table is only as fast as the app's indexes.
- SQLite compares `decimal` with a function that calls back into .NET, which made a first version of this experiment (a
  filter on a `decimal`) cost 15 ms a request whatever the shape. The filter in these runs is on a date and an integer. A
  real server will have its own habits; none was measured.

### 4. The first request for a new set of columns

40 random sets of six of the 35 columns, each run twice (two different filters). Each variant has its own EF query cache.

| How the rows are shaped | Making the projection | First request | Second request | New query-cache entries per set |
| --- | ---: | ---: | ---: | ---: |
| `object[]` built per request | 59 µs | 7.7 ms | 1.5 ms | 2.0 |
| Generated class, made once per set | 3,283 µs | 10.9 ms | 2.5 ms | 2.0 |

A new set of columns costs EF about **6 ms** to translate (7.7 less 1.5) and two entries in its query cache; an earlier run of
the same experiment on a busier machine gave about 9 ms. Running the same set again with other values is the cheap path. This is
the same for every shape and is the real price of choosing columns at run time; the cache is bounded, so the worst a user can do
is push other queries out, which the column and request limits (and, in the app, a rate limit) are for.

### 5. Does it translate? (EF Core 10, SQLite)

| Query | Result |
| --- | --- |
| `Select(x => new object[] { … })` for six columns after filter, order, skip and take | Translates |
| The same, null-guarded (`x.Customer == null ? null : x.Customer.Name`) | Translates; the SQL is identical to the unguarded query |
| `Select` into a class generated for the columns | Translates |
| `Select(x => new Dictionary<string, object?> { { "Reference", x.Reference }, … })` | Translates |
| `GroupBy(x => new object[] { x.Status })` then `Count()` | **Does not translate** (`InvalidOperationException`) |
| `GroupBy` a `ValueTuple` of two columns; a generated class; an anonymous type | Translate (two columns each; more were not tried) |
| Over a list in memory, `x.Customer.Name` with no customer | **`NullReferenceException`** unguarded; fine guarded |
| `Skip` / `Take` as constants, over 40 different pages (400 requests) | No new cache entries: EF parameterises them itself. As variables, the same |

The SQL for six columns, with a page of 50 at offset 100 (the values are parameters, only the chosen columns are read, and the join
is there because a chosen column reaches the customer):

```sql
SELECT "o0"."Reference", "o0"."Placed", "o0"."Total", "o0"."Status", "c"."Name", "o0"."ShipTo_City"
FROM (
    SELECT "o"."Id", "o"."CustomerId", "o"."Placed", "o"."Reference", "o"."Status", "o"."Total", "o"."ShipTo_City"
    FROM "Orders" AS "o"
    WHERE "o"."Placed" >= @Value AND ef_compare("o"."Total", @Value1) >= 0
    ORDER BY "o"."Placed" DESC, "o"."Id"
    LIMIT @Value3 OFFSET @Value2
) AS "o0"
INNER JOIN "Customers" AS "c" ON "o0"."CustomerId" = "c"."Id"
ORDER BY "o0"."Placed" DESC, "o0"."Id"
```

(That query was taken from the first version of the experiment, which filtered on `Total`, a `decimal`; see the note above.
The `SELECT` list, the parameters and the join are the same with the later filter.)

### 6. Not measured

The CSV writer and the results table (they are not written, and are bounded by the row caps, not by a request); deep offsets and
`COUNT` over a large table (they depend on the database); a real database server (SQL Server, PostgreSQL), including how it caches
plans; the stored-report store; the browser side (columns and sort UI); a quiet machine. The cost limits in this document are
proposals, not results.

## Decisions for you

| # | Decision | Recommendation | Consequence |
| --- | --- | --- | --- |
| **D1** | **Where the code lives.** In `ExpressionBuilder.Web` under `Reports/`, or a new package | Stay in `Web` | The schema and compiler are in `Web` (a `Reports` package would depend on a package full of Razor); splitting them out is a refactor of its own. Cost: someone who wants reports without MVC still references the MVC package. Revisit when a second consumer needs the core without the UI |
| **D2** | **When it ships.** Inside 1.0.0, or after it as 1.1.0 | After: merge, `1.0.0-rc.2`, soak, `1.0.0`; the slices then land on `main` and ship as `1.1.0` | A stable 1.0.0 then does not carry a new, unsoaked feature. If it goes inside 1.0.0 instead, the release candidates need another soak and the slices have to merge first |
| **D3** | **A shared report that uses a column the runner may not see.** Refuse, or leave the column out with a note | Refuse | Simplest and never changes what a report means; authors of global reports use common columns |
| **D4** | **Reordering columns.** Buttons, or drag and drop | Buttons | Keyboard, touch and screen readers work from day one; drag could be added later on top (with the buttons kept) |

## Documents that change

**`SECURITY.md`** (with slice a, and again with c, d and f):

- *Trust boundaries* gains rows: *what the report builder posts* (**no**: root key, paths and text only, checked against a schema
  built for the user); *a stored report* (**no**: checked again, for whoever runs it); *a root key* (looked up in an allow-list
  the app registered, never a type name).
- A new section, *What the report builder enforces*: roots by key; one per-user schema for filter, columns and sort; unknown
  members refused; the scope applied before the filter and the count; the limits; one message for hidden, missing and not allowed;
  CSV cells neutralised; no result caching.
- *What it can't decide for you* gains: **the scope** (`Source`: tenant, soft delete, row rules, `AsNoTracking`); **related rows**
  (global query filters on entities reached through a navigation); **which properties are restricted** (`Restrict`,
  `Exclude`, `[FilterBuilderIgnore]`); a **read-only database login** for reports; the **time limit** (token and
  `CommandTimeout`); **rate limits** on running and exporting; indexes for the tables people will filter; who may **publish a
  global report**; and **auditing** (the log entry the library writes is the start of it).
- *Reporting a vulnerability* and *Supported versions* are unchanged.

**Others:** `docs/report-builder.md` (the guide, written with each slice, linked from `AGENTS.md`'s docs index);
`docs/architecture/overview.md` (a "Reports" section when slice a lands, linking here rather than repeating it);
`docs/web-filter-builder.md` (a pointer, and the note that the standalone builder is version 1); `docs/standards/testing.md` (the
new test projects or folders and the benchmark classes); `ChangeLog.md` (Added, per slice, under Web); `README.md` (a short
"Reports" paragraph when slice d ships); `tools/ExpressionBuilder.Benchmarks/README.md` (the new classes and `-- report`).
