namespace ExpressionBuilder.Web.Tests;

using ExpressionBuilder.Web.Tests.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using TopMarksDevelopment.ExpressionBuilder.Web;
using static ExpressionBuilder.Web.Tests.TestData;

/// <summary>
/// What the filter builder posts, run against a database (SQLite, in memory) through Entity Framework Core. EF
/// only translates what it can: a filter it can't translate throws rather than quietly running in memory, so a
/// test that passes here was turned into SQL. Each result is also compared with the same filter run over the same
/// accounts in memory.
/// </summary>
[Collection("Operation catalog")]
public class EfCoreTests(Shop shop) : IClassFixture<Shop>
{
    FilterBuildResult<Account> Build(
        string json,
        FilterBuilderOptions? options = null
    )
    {
        var result = new FilterBuilderModel { Json = json }.Build<Account>(
            options
        );

        Assert.True(result.Succeeded, string.Join("; ", result.Errors));

        return result;
    }

    /// <summary>The ids the database finds, the ids the same filter finds in memory, and the SQL.</summary>
    (List<int> Database, List<int> Memory, string Sql) Run(
        string json,
        FilterBuilderOptions? options = null
    )
    {
        var filter = Build(json, options).Filter!;
        var query = shop.Context.Accounts.Where(filter);

        return (
            [.. query.Select(a => a.Id).ToList().Order()],
            [.. shop.Accounts.Where(filter).Select(a => a.Id).Order()],
            query.ToQueryString()
        );
    }

    #region Every operation, on every kind it is offered for

    public static TheoryData<string, string> OfferedOperations()
    {
        var data = new TheoryData<string, string>();
        var schema = FilterSchema.For<Account>();

        foreach (
            var field in schema
                .Types[schema.Root]
                .Fields.Where(f => f.Kind != FieldKind.Collection)
        )
        foreach (var operation in field.Operations)
            data.Add(field.Path, operation);

        return data;
    }

    [Theory(
        DisplayName = "An offered operation becomes SQL, and finds what it finds in memory"
    )]
    [MemberData(nameof(OfferedOperations))]
    public void EveryOfferedOperationTranslates(
        string path,
        string operationName
    )
    {
        var schema = FilterSchema.For<Account>();
        var field = schema
            .Types[schema.Root]
            .Fields.Single(f => f.Path == path);
        var operation = schema.Operations.Single(o => o.Name == operationName);

        var (database, memory, sql) = Run(
            Doc(Cond(path, operationName, ValuesFor(field, operation)))
        );

        // "Is set" on a column that can't be null is true for every row, so EF leaves the test out altogether
        Assert.Equal(memory, database);
        Assert.Contains("SELECT", sql);
    }

    #endregion Every operation, on every kind it is offered for

    #region Shapes of filter

    [Fact(DisplayName = "A collection: any order over 100 becomes EXISTS")]
    public void CollectionAny()
    {
        var (database, memory, sql) = Run(
            Doc(
                Group(
                    [Cond("Total", "GreaterThan", ["100"])],
                    collection: "Orders"
                )
            )
        );

        Assert.Equal<int>([1, 2], database);
        Assert.Equal(memory, database);
        Assert.Contains("EXISTS", sql);
    }

    [Fact(
        DisplayName = "A collection inside a collection: any order with a line of 10 or more"
    )]
    public void NestedCollections()
    {
        var (database, memory, sql) = Run(
            Doc(
                Group(
                    [
                        Group(
                            [Cond("Quantity", "GreaterThanOrEqual", ["10"])],
                            collection: "Lines"
                        ),
                    ],
                    collection: "Orders"
                )
            )
        );

        Assert.Equal<int>([1], database);
        Assert.Equal(memory, database);
        Assert.Equal(2, CountOf(sql, "EXISTS"));
    }

    [Fact(
        DisplayName = "A collection filtered with several conditions looks at the same item"
    )]
    public void CollectionConditionsShareAnItem()
    {
        // Account 1 has an order over 100 and an order from before February, but not one that is both
        var (database, memory, _) = Run(
            Doc(
                Group(
                    [
                        Cond("Total", "GreaterThan", ["100"], connector: "And"),
                        Cond("Placed", "GreaterThan", ["2025-02-01"]),
                    ],
                    collection: "Orders"
                )
            )
        );

        Assert.Empty(database);
        Assert.Equal(memory, database);
    }

    [Fact(
        DisplayName = "A collection of plain values: a tag that contains text"
    )]
    public void CollectionOfPlainValues()
    {
        var (database, memory, sql) = Run(
            Doc(Group([Cond("", "Contains", ["vi"])], collection: "Tags"))
        );

        Assert.Equal<int>([1, 4], database);
        Assert.Equal(memory, database);
        Assert.Contains("json_each", sql);
    }

    [Fact(
        DisplayName = "Text: contains, starts with and ends with are SQL, not memory"
    )]
    public void TextOperations()
    {
        var (contains, memoryContains, containsSql) = Run(
            Doc(Cond("Name", "Contains", ["ring"]))
        );
        var (starts, memoryStarts, _) = Run(
            Doc(Cond("Name", "StartsWith", ["alan"]))
        );
        var (ends, memoryEnds, _) = Run(
            Doc(Cond("Email", "EndsWith", ["example.com"]))
        );

        Assert.Equal<int>([1], contains);
        Assert.Equal<int>([1], starts);
        Assert.Equal<int>([1, 3], ends);
        Assert.Equal(memoryContains, contains);
        Assert.Equal(memoryStarts, starts);
        Assert.Equal(memoryEnds, ends);
        Assert.Matches("INSTR|LIKE", containsSql.ToUpperInvariant());
    }

    [Fact(
        DisplayName = "Text adjustments: trim, lower case and replace become TRIM, LOWER and REPLACE"
    )]
    public void TextAdjustments()
    {
        var (database, memory, sql) = Run(
            Doc(
                Cond(
                    "Name",
                    "Equal",
                    ["ada lovelace"],
                    manipulators:
                    [
                        new { name = "Trim" },
                        new { name = "ToLower" },
                    ]
                )
            )
        );

        Assert.Equal<int>([3], database);
        Assert.Equal(memory, database);
        Assert.Contains("TRIM", sql.ToUpperInvariant());
        Assert.Contains("LOWER", sql.ToUpperInvariant());

        var (replaced, memoryReplaced, replaceSql) = Run(
            Doc(
                Cond(
                    "Name",
                    "Equal",
                    ["AlanTuring"],
                    manipulators:
                    [
                        new { name = "Replace", args = new[] { " ", "" } },
                    ]
                )
            )
        );

        Assert.Equal<int>([1], replaced);
        Assert.Equal(memoryReplaced, replaced);
        Assert.Contains("REPLACE", replaceSql.ToUpperInvariant());
    }

    [Fact(
        DisplayName = "Nullable values: a missing value doesn't match a comparison, and is found by 'is not set'"
    )]
    public void NullableComparisons()
    {
        var (older, memoryOlder, olderSql) = Run(
            Doc(Cond("Age", "GreaterThan", ["30"]))
        );
        var (younger, memoryYounger, _) = Run(
            Doc(Cond("Age", "LessThan", ["30"]))
        );
        var (missing, memoryMissing, missingSql) = Run(
            Doc(Cond("Age", "IsNull"))
        );
        var (present, memoryPresent, _) = Run(
            Doc(Cond("LastSeen", "IsNotNull"))
        );

        Assert.Equal<int>([1, 3], older);
        Assert.Equal<int>([4], younger);
        Assert.Equal<int>([2], missing);
        Assert.Equal<int>([1, 3, 4], present);
        Assert.Equal(memoryOlder, older);
        Assert.Equal(memoryYounger, younger);
        Assert.Equal(memoryMissing, missing);
        Assert.Equal(memoryPresent, present);
        Assert.Contains("IS NULL", missingSql.ToUpperInvariant());
        Assert.Contains("\"Age\"", olderSql);
    }

    [Fact(
        DisplayName = "Dates and times: a date, a date and time, a time and a range"
    )]
    public void DatesAndTimes()
    {
        var (born, memoryBorn, _) = Run(
            Doc(Cond("Born", "LessThan", ["1910-01-01"]))
        );
        var (seen, memorySeen, _) = Run(
            Doc(
                Cond(
                    "LastSeen",
                    "Between",
                    ["2025-01-01T00:00", "2025-12-31T23:59"]
                )
            )
        );
        var (opens, memoryOpens, _) = Run(
            Doc(Cond("Opens", "Equal", ["09:00"]))
        );

        Assert.Equal<int>([2, 3], born);
        Assert.Equal<int>([1, 4], seen);
        Assert.Equal<int>([1, 3], opens);
        Assert.Equal(memoryBorn, born);
        Assert.Equal(memorySeen, seen);
        Assert.Equal(memoryOpens, opens);
    }

    [Fact(DisplayName = "A property of a related object, and of an owned one")]
    public void RelatedObjects()
    {
        var (employer, memoryEmployer, employerSql) = Run(
            Doc(Cond("Employer.Name", "StartsWith", ["Bab"]))
        );
        var (city, memoryCity, _) = Run(
            Doc(Cond("Home.City", "Equal", ["London"]))
        );
        var (noEmployer, memoryNone, _) = Run(Doc(Cond("Employer", "IsNull")));

        Assert.Equal<int>([3], employer);
        Assert.Equal<int>([3], city);
        Assert.Equal<int>([2, 4], noEmployer);
        Assert.Equal(memoryEmployer, employer);
        Assert.Equal(memoryCity, city);
        Assert.Equal(memoryNone, noEmployer);
        Assert.Contains("JOIN", employerSql.ToUpperInvariant());
    }

    [Fact(DisplayName = "Enums, yes/no, and guids")]
    public void EnumsBooleansAndGuids()
    {
        var (tier, memoryTier, _) = Run(Doc(Cond("Tier", "Equal", ["Gold"])));
        var (flag, memoryFlag, _) = Run(
            Doc(Cond("Verified", "Equal", ["true"]))
        );
        var (guid, memoryGuid, _) = Run(
            Doc(Cond("Reference", "Equal", [GuidOne.ToString()]))
        );
        var (tiers, memoryTiers, _) = Run(
            Doc(Cond("Tier", "In", ["Silver", "Gold"]))
        );

        Assert.Equal<int>([1], tier);
        Assert.Equal<int>([1, 4], flag);
        Assert.Equal<int>([1], guid);
        Assert.Equal<int>([1, 2], tiers);
        Assert.Equal(memoryTier, tier);
        Assert.Equal(memoryFlag, flag);
        Assert.Equal(memoryGuid, guid);
        Assert.Equal(memoryTiers, tiers);
    }

    [Fact(DisplayName = "And, or and a group in brackets")]
    public void ConnectorsAndGroups()
    {
        var (database, memory, sql) = Run(
            Doc(
                Cond("Active", "Equal", ["true"], connector: "And"),
                Group([
                    Cond("Age", "GreaterThan", ["40"], connector: "Or"),
                    Cond("Tier", "Equal", ["Basic"]),
                ])
            )
        );

        Assert.Equal<int>([1, 3], database);
        Assert.Equal(memory, database);
        Assert.Contains(" OR ", sql.ToUpperInvariant());
    }

    [Fact(
        DisplayName = "Values are sent as parameters, not written into the SQL"
    )]
    public void ValuesAreParameters()
    {
        var (database, _, sql) = Run(
            Doc(Cond("Name", "Equal", ["Alan Turing"]))
        );

        Assert.Equal<int>([1], database);

        // The value is in the parameters (the comment at the top), and the query only mentions the parameter
        var query = sql[sql.IndexOf("SELECT", StringComparison.Ordinal)..];

        Assert.DoesNotContain("Alan Turing", query);
        Assert.Matches(@"WHERE ""a"".""Name"" = @", query);
    }

    [Fact(DisplayName = "A list of choices is sent as parameters too")]
    public void ListsAreParameters()
    {
        var (database, memory, sql) = Run(
            Doc(Cond("Name", "In", ["Alan Turing", "grace hopper"]))
        );
        var query = sql[sql.IndexOf("SELECT", StringComparison.Ordinal)..];

        Assert.Equal<int>([1, 2], database);
        Assert.Equal(memory, database);
        Assert.DoesNotContain("Alan Turing", query);
    }

    [Fact(DisplayName = "Different values share one query in EF's cache")]
    public void DifferentValuesShareACacheEntry()
    {
        var cache = (MemoryCache)shop.Context.GetService<IMemoryCache>();

        // Once, so what EF caches for any first query is already there
        Run(Doc(Cond("Name", "Equal", ["first"], match: null)));

        var before = cache.Count;

        foreach (
            var name in new[]
            {
                "second",
                "third",
                "fourth",
                "fifth",
                "Alan Turing",
            }
        )
            Run(Doc(Cond("Name", "Equal", [name])));

        Assert.Equal(before, cache.Count);
    }

    [Fact(
        DisplayName = "What stays in the SQL as it is: null, true/false and the empty text"
    )]
    public void ShapeStaysInTheSql()
    {
        var (nulls, _, nullSql) = Run(Doc(Cond("Age", "IsNull")));
        var (empty, _, emptySql) = Run(Doc(Cond("Email", "IsEmpty")));

        Assert.Equal<int>([2], nulls);
        Assert.Equal<int>([4], empty);
        Assert.Contains("IS NULL", nullSql.ToUpperInvariant());
        Assert.Contains("''", emptySql);
    }

    #endregion Shapes of filter

    static int CountOf(string text, string find) =>
        (text.Length - text.Replace(find, "").Length) / find.Length;
}
