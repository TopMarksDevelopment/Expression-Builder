# Expression Builder

[![Icon for GitHub actions workflow status](https://img.shields.io/github/actions/workflow/status/TopMarksDevelopment/Expression-Builder/build-and-test.yml?style=for-the-badge)](https://github.com/TopMarksDevelopment/Expression-Builder/actions/workflows/build-and-test.yml)
[![Icon for NuGet version](https://img.shields.io/nuget/v/TopMarksDevelopment.ExpressionBuilder?style=for-the-badge)](https://www.nuget.org/packages/TopMarksDevelopment.ExpressionBuilder)
[![Icon for NuGet download count](https://img.shields.io/nuget/dt/TopMarksDevelopment.ExpressionBuilder?style=for-the-badge)](https://www.nuget.org/packages/TopMarksDevelopment.ExpressionBuilder)

If you're looking for a library to help you easily build lambda expressions, look no further than the **Expression Builder** package.

With this library you can quickly create a filter that can be applied to a list/enumerable or database query; even dynamically. Plus, it's packed with some great features too!

## Contents

-   [Features](#features)
-   [Installation](#installation)
-   [Suggestions & Issues](#suggestions--issues)
-   [Conventions (ref by string)](#conventions-reference-by-strings)
-   [Examples](#examples)
    -   [Using `Filter<TClass>`](#using-filtertclass)
    -   [Matching lists](#matching-lists)
    -   [Complex matches](#complex-expressions)
    -   [Extending `IQueryable<TClass>` or `IEnumerable<TClass>`](#extending-iqueryabletclass-or-ienumerabletclass)
    -   [`SmartSearch`](#smartsearch)
-   [Serialization](#serialization)
-   [Built-in operations](#built-in-operations)
-   You can check these out too:
    -   [Changelog](./ChangeLog.md)
    -   [License](./LICENSE.md)

## Features

The Expression Builder offers a wide range of features, including:

-   Chain fluently from your `IQueryable`/`IEnumerable` collections. (See examples [here](#extending-iqueryabletclass-or-ienumerabletclass))
-   The ability to save queries for later re-execution by serializing them to JSON strings or a `byte[]` (for compact storage).
-   Method support, referred to as "manipulators", means you can do `x => x.Name.Replace(" ", "")`
-   Built-in null checks
-   The ability to handle complex expressions, including `IEnumerable<>` properties and groups (i.e. `(x || y) && z`).
-   The ability to reference properties in two ways: by property expression (`x => x.Name`) or by string, such as "Name" (when following these [conventions](#conventions-reference-by-strings)).
-   The ability to build queries in two ways: by using the `.Add()` method or by operation (like `.Equal(...)`)
    -   These calls can be chained together or added statement by statement; whichever you prefer
-   A variety of [built-in operations](#built-in-operations), including the powerful [`SmartSearch`](#smartsearch) method
    -   plus, build your own using the API!  
         _(All operations use the API. So any one of these can start you off on building your own)_
-   The ability to match lists of values, such as finding "John" and "Jess" (See example [here](#matching-lists))

## Installation

To install the Expression Builder, you can use the .NET CLI, Package Manager console, or another method of your choice. You can find these installation methods and more information about the package on the [NuGet package](https://www.nuget.org/packages/TopMarksDevelopment.ExpressionBuilder/).

But, for example, to install the package using the .NET CLI, run the following command:

> dotnet add package TopMarksDevelopment.ExpressionBuilder

## Suggestions & Issues

If you find any errors or realise there's a missing feature, feel free to leave comments or open issues.

## Conventions (reference by strings)

If you opt to add filters using `string` references, you must follow these conventions:

-   A property can be referenced by its name alone (so `Id`, `Name`, `Gender`, etc.)
-   One-to-one relationships need only a dot to separate them. This means a persons' "Birth Country" is referenced simply by `Birth.Country` - again, with correctly referenced names
-   A collection can also be referenced by placing `[]` after the name. So, a person (with multiple contact points) can have their "Contacts Type" referenced by `Contacts[].Type`
-   Any manipulators/methods must be applied to the `options` property (like the `.Replace(.., ..)` string method)

## Examples

All examples are based on the below set of classes

<details>
<summary>Example classes</summary>

```csharp
public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }
    public PersonGender Gender { get; set; }
    public BirthData Birth { get; set; }
    public List<Contact> Contacts { get; private set; }
    public Company Employer { get; set; }
}

public enum PersonGender
{
    Male,
    Female,
    Other,
    PreferNotToSay
}

public class BirthData
{
    public DateTime Date { get; set; }
    public string Country { get; set; }
}

public class Contact
{
    public ContactType Type { get; set; }
    public string Value { get; set; }
    public string Comments { get; set; }
}

public enum ContactType
{
    Telephone,
    Email
}

public class Company {
    public string Name { get; set; }
    public string Industry { get; set; }
}
```

</details>

### Using `Filter<TClass>`

We can use the main class of this package directly, the `Filter<TClass>` class.

```csharp
// Chain operation calls to build your query
var filter = new Filter<Person>();
filter.Between("Id", 2, 4)
        .And()
        .IsNotNull("Birth.Country")
        .And()
        .EndsWith("Contacts[].Value", "@email.com")
        .Or()
        .SmartSearch("Name", "\"John\"");

// OR

// Chain Add calls to build your query
var filter = new Filter<Person>();
filter.Add("Id", Operation.Between, 2, 4)
        .And()
        .Add("Birth.Country", Operation.IsNotNull)
        .And()
        .Add("Contacts[].Value", Operation.EndsWith, "@email.com")
        .Or()
        .Add("Name", Operation.SmartSearch, "\"John\"");

// OR

// Add each statement line by line
//    (this works for operation calls too)
var filter = new Filter<Person>();
filter.Add("Id", Operation.Between, 2, 4,  Connector.And);
filter.Add("Contacts[].Value", Operation.EndsWith, "@email.com", Connector.And);
filter.Add("Birth.Country", Operation.IsNotNull, Connector.Or);
filter.Add("Name", Operation.SmartSearch, "\"John\"");
var people = People.Where(filter);

// Now apply either of these filters
var people = People.Where(filter);
```

### Matching lists

If you find yourself needing the same operation but with different terms. Save yourself from repeating lines, thanks to list matching.

Just pass an array of values and it will query all those terms - on one line!

Let's say we want to find a "Bright Blue Bicycle"! Simple, split the term and filter by it.

```CSharp
var filter = new Filter<Products>();
var termArr = "Bright Blue Bicycle".Split(' ');
var options = new FilterStatementOptions() { Match = Matches.All };

// Connector and Matches are defaulted.
// So, giving an array works in the same way. No extra code required!
// Note: the default for "Contains" is any
filter.Contains(x => x.Name, termArr);

// or
// Declare `options`, so we're matching all!
filter.Contains("Name", termArr, options);

// or
// Declare `options` and `connector` too if you want/need to
filter.Contains("Name", termArr, options, Connector.And);
```

### Complex expressions

Complex expressions are handled by grouping filter statements, like in the example below.

_Here we are using the fluent API with operation calls on a filter class:_

```csharp
var filter = new Filter<People>();
filter
    .OpenGroup()
        .OpenGroup()
            .DoesNotContain(p => p.Name, "doe")
            .Or()
            .OpenGroup()
                .EndsWith(p => p.Name, "Doe")
                .Or()
                .StartsWith(p => p.Name, "Jo")
            .CloseGroup()
        .CloseGroup()
        .And()
        .IsNull(p => p.Employer)
    .CloseGroup()
    .Or()
    .Equal(p => p.Birth.Country, "GB");

// Now let's apply the filter to our DB Context
var people = myDbContext.People.Where(filter);
```

This would produce an expression like this: (Excluding all the `NotNull` checks and `.Trim().ToLower()` functions)

```CSharp
myDbContext.People
    .Where(p =>
        (
            (
                !p.Name.Contains("doe")
                    || ( p.Name.EndsWith("Doe")
                    || p.Name.StartsWith("Jo") )
            ) &&
            p.Employer == null
        ) &&
        p.Birth.Country == "USA"
    );
```

Every time you start a group that means all further statements will be at the same "level/parenthesis" until CloseGroup is called.

You can even add groups to groups! (For those super complex expressions)

### Extending `IQueryable<TClass>` or `IEnumerable<TClass>`

You can also filter directly from your enumerable to simplify and improve the readability of your code.

-   Call `.ToFilterable()` on your `IEnumerable`
-   Or query the DB context fluently after calling `.AsFilterable()` on your `IQueryable`

After this, you will have access to a bunch of new methods!

_This example uses the fluent API and (mostly) property expressions_

```CSharp
var filteredPeople =
    myDbContext.People
        .AsFilterable()
        .Equal(x => x.Id, 1)
        .Or()
        .OpenCollection(x => x.Contacts)
            // We're now bound to the Contact type
            .EndsWith(x => x.Value, "@email.com")
            .Or()
            .StartsWith(x => x.Comment, "Test")
        // We add this type to correct the reference
        .CloseCollection<Person>()
        //    or use an the expression `x => x.Person` (if that property exists)
        .Or()
        // We can use the string notion too
        .EndsWith("Contacts[].Value", "@email.com");
```

### `SmartSearch`

This package also includes a handy method called `SmartSearch`. This allows you to pass a single term (or a `string[]` of terms) to be subject to some "smart" checks:

-   A term inside quotes (i.e. `"Blue"`) will search for this exact term; meaning it's not buried inside a word (so, `Steelblue` will not be included)
-   A term with a leading hyphen (i.e. `-bright`) will exclude anything containing this term. You can also enclose the term in quotes to exclude an exact word/set of words (i.e. `-"bright"`)
-   Otherwise, it will search if it contains that term

> ⚠ A single value will be treated as a single term (regardless of spaces) - it must be parsed. Use `SmartSearch.SplitTerm(string input)` to parse a string (see the example)

_This example uses property expressions on a filterable `IQueryable`_

```csharp
var term = "Term1 Term2 -IgnoreTerm3 \"Exact term\" -\"Ignore exact term\"";

var smartTerms = SmartSearch.SplitTerm(term);
// smartTerms is now an array as shown below
// [ Term1, Term2, -IgnoreTerm3, "Exact term", -"Ignore exact term"];

var filteredPeople =
    myDbContext.People
        .AsFilterable()
        .SmartsSearch(x => x.Name, smartTerms);
```

## Serialization

You can serialize an expression so it can be reused later.

We support two different serialization methods:

-   a JSON string (for a human-friendly version)
-   a `byte[]`/stream (for more compact storage on disk or a DB)

### JSON serialisation

If you wanted to store the filter in a human-friendly way, you can use JSON serialisation.

#### Example

```CSharp
using System.Text.Json;

// Create the filter
var filter = new Filter<Category>();

// Build the filter
filter
    .OpenCollection(x => x.Products)
        .Equal(x => x.Name, "Product 2")
        .Or()
        .Equal(x => x.Id, 2)
        .Or()
        .OpenCollection(x => x.Categories)
            .Equal(x => x.Id, 1)
            .Or()
            .Equal(x => x.Id, 2)
        .CloseCollection<Category>()
    .CloseCollection<Category>()
    .And()
    .Equal(x => x.Id, 2);

// To serialise the filter
var jsonString =
    JsonSerializer.Serialize(_filter);

// To deserialise the filter
var filterFromJson =
    JsonSerializer.Deserialize<Filter<Category>>(jsonString);
```

### `Byte[]`/Stream serialisation (Protobuf)

When you want to optimise the storage size of your expression, you can serialise it into a `byte[]` or stream.

#### Example

```CSharp
// Using the `filter` that was built above

// Serialize to a Stream (specifically a FileStream)
using (var file = File.Create("./MyPath/File.dat"))
    filter.SerializeTo(file);

// Deserialize from a Stream (specifically a FileStream)
using var rFile = File.OpenRead("./MyPath/File.dat");
var filterFromFile = Filter<Category>.DeserializeFrom(rFile);

// Serialize to a byte[]
filter.SerializeTo(out var bytes);

// Deserialize from a byte[]
var filterFromBytes = Filter<Category>.DeserializeFrom(bytes);
```

#### GRPC / Protobuf

This storage method, combined with the [`.proto`](./Packages/Core/src/Proto/ExpressionBuilder.proto) file means you can actually transfer this data through a GRPC message. Just create a service that consumes the `FilterGroup` message and you're away.

## Built-in operations

These are all the operations included in the main package:

-   Between
-   NotBetween
-   BetweenExclusive (excludes the min and max)
-   NotBetweenExclusive (as above)
-   Contains
-   DoesNotContain
-   EndsWith
-   DoesNotEndWith
-   Equal
-   NotEqual
-   GreaterThan
-   GreaterThanOrEqual
-   LessThan
-   LessThanOrEqual
-   IsEmpty
-   IsNotEmpty
-   IsNull
-   IsNotNull
-   IsNullOrWhiteSpace
-   IsNotNullNorWhiteSpace
-   SmartSearch
-   StartsWith
-   DoesNotStartWith
