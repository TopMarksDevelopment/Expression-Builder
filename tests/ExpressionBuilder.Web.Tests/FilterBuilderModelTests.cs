namespace ExpressionBuilder.Web.Tests;

using ExpressionBuilder.Web.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;
using TopMarksDevelopment.ExpressionBuilder.Web;
using static ExpressionBuilder.Web.Tests.TestData;

/// <summary>
/// What the browser posts becomes a filter, and the filter finds the right people.
/// </summary>
[Collection("Operation catalog")]
public class FilterBuilderModelTests
{
    static readonly List<Person> People = TestData.People();

    static FilterBuildResult<Person> Build(
        string? json,
        FilterBuilderOptions? options = null
    ) => new FilterBuilderModel { Json = json }.Build<Person>(options);

    static List<int> Run(params object[] items)
    {
        var result = Build(Doc(items));

        Assert.True(result.Succeeded, string.Join("; ", result.Errors));

        return People.Where(result.Filter!).Ids();
    }

    #region Finding people

    [Fact(DisplayName = "Text: contains ignores case")]
    public void TextContains() =>
        Assert.Equal<int>([1], Run(Cond("Name", "Contains", ["TURING"])));

    [Fact(DisplayName = "Text: any of several terms")]
    public void ContainsAny() =>
        Assert.Equal<int>(
            [1, 2],
            Run(Cond("Name", "Contains", ["turing", "hopper"], match: "Any"))
        );

    [Fact(DisplayName = "Text: all of several terms")]
    public void ContainsAll()
    {
        Assert.Equal<int>(
            [1],
            Run(Cond("Name", "Contains", ["alan", "turing"], match: "All"))
        );
        Assert.Empty(
            Run(Cond("Name", "Contains", ["alan", "hopper"], match: "All"))
        );
    }

    [Fact(DisplayName = "Text: starts with any of several terms")]
    public void StartsWithAny() =>
        // Text can't start with two different things, so it is always "any of them"
        Assert.Equal<int>(
            [1, 2],
            Run(Cond("Name", "StartsWith", ["al", "gr"], match: "All"))
        );

    [Fact(
        DisplayName = "Text: does not start with, any or all of several terms"
    )]
    public void DoesNotStartWithAnyOrAll()
    {
        // Not starting with "al" and not starting with "gr"
        Assert.Equal<int>(
            [3, 4],
            Run(Cond("Name", "DoesNotStartWith", ["al", "gr"], match: "All"))
        );
        // Not starting with "al" or not starting with "gr": everyone
        Assert.Equal<int>(
            [1, 2, 3, 4],
            Run(Cond("Name", "DoesNotStartWith", ["al", "gr"], match: "Any"))
        );
    }

    [Fact(DisplayName = "Text: smart search")]
    public void SmartSearch() =>
        Assert.Equal<int>(
            [2],
            Run(Cond("Name", "SmartSearch", ["grace", "-turing"]))
        );

    [Fact(DisplayName = "Numbers: equal, and between")]
    public void Numbers()
    {
        Assert.Equal<int>([3], Run(Cond("Id", "Equal", ["3"])));
        Assert.Equal<int>([1, 4], Run(Cond("Age", "Between", ["20", "41"])));
        Assert.Equal<int>(
            [1, 2],
            Run(Cond("Salary", "GreaterThan", ["999.99"]))
        );
        Assert.Equal<int>([3], Run(Cond("Score", "LessThan", ["-0.5"])));
    }

    [Fact(DisplayName = "Numbers: one of a list")]
    public void NumbersIn() =>
        Assert.Equal<int>([1, 3], Run(Cond("Id", "In", ["1", "3", "99"])));

    [Fact(DisplayName = "Dates: a date, a date and time, and a date only")]
    public void Dates()
    {
        Assert.Equal<int>(
            [2, 3],
            Run(Cond("Born", "LessThan", ["1910-01-01"]))
        );
        Assert.Equal<int>(
            [1, 4],
            Run(Cond("LastSeen", "Equal", ["2024-01-10T09:30"]))
        );
        Assert.Equal<int>(
            [4],
            Run(Cond("Joined", "GreaterThan", ["2010-01-01"]))
        );
    }

    [Fact(DisplayName = "Times: a time of day and a duration")]
    public void Times()
    {
        Assert.Equal<int>([2, 4], Run(Cond("StartsAt", "LessThan", ["08:00"])));
        Assert.Equal<int>(
            [1, 2],
            Run(Cond("Duration", "GreaterThanOrEqual", ["01:30"]))
        );
    }

    [Fact(DisplayName = "Booleans: true, false and not set")]
    public void Booleans()
    {
        Assert.Equal<int>([1, 2], Run(Cond("Active", "Equal", ["true"])));
        Assert.Equal<int>([3, 4], Run(Cond("Active", "NotEqual", ["true"])));
        Assert.Equal<int>([1, 4], Run(Cond("Verified", "Equal", ["true"])));
        Assert.Equal<int>([3], Run(Cond("Verified", "IsNull")));
    }

    [Fact(DisplayName = "Enums: by member name")]
    public void Enums()
    {
        Assert.Equal<int>([1, 4], Run(Cond("Favourite", "Equal", ["Red"])));
        Assert.Equal<int>(
            [2, 3],
            Run(Cond("Favourite", "In", ["Blue", "Green"]))
        );
    }

    [Fact(DisplayName = "Guids")]
    public void Guids() =>
        Assert.Equal<int>(
            [1, 4],
            Run(Cond("Reference", "Equal", [GuidOne.ToString()]))
        );

    [Fact(
        DisplayName = "Related objects: a property of one, and whether one exists"
    )]
    public void RelatedObjects()
    {
        Assert.Equal<int>(
            [1],
            Run(Cond("Home.Postcode", "StartsWith", ["M1"]))
        );
        Assert.Equal<int>([2], Run(Cond("Employer", "IsNull")));
        Assert.Equal<int>([1, 3, 4], Run(Cond("Employer", "IsNotNull")));
        Assert.Equal<int>(
            [1],
            Run(Cond("Employer.Name", "Equal", ["Bletchley"]))
        );
    }

    [Fact(DisplayName = "Missing values: null, empty and blank")]
    public void MissingValues()
    {
        Assert.Equal<int>([2], Run(Cond("Email", "IsNull")));
        Assert.Equal<int>([1, 3, 4], Run(Cond("Email", "IsNotNull")));
        Assert.Equal<int>([4], Run(Cond("Email", "IsEmpty")));
        Assert.Equal<int>([2, 4], Run(Cond("Email", "IsNullOrWhiteSpace")));
        Assert.Equal<int>([1, 3], Run(Cond("Email", "IsNotNullOrWhiteSpace")));
    }

    #endregion Finding people

    #region Manipulators

    [Fact(DisplayName = "Adjusting text: ignore case")]
    public void ToLowerManipulator() =>
        Assert.Equal<int>(
            [2],
            Run(
                Cond(
                    "Name",
                    "Equal",
                    ["grace hopper"],
                    manipulators: [new { name = "ToLower" }]
                )
            )
        );

    [Fact(DisplayName = "Adjusting text: trim")]
    public void TrimManipulator()
    {
        Assert.Empty(Run(Cond("Name", "Equal", ["Ada Lovelace"])));
        Assert.Equal<int>(
            [3],
            Run(
                Cond(
                    "Name",
                    "Equal",
                    ["Ada Lovelace"],
                    manipulators: [new { name = "Trim" }]
                )
            )
        );
    }

    [Fact(DisplayName = "Adjusting text: replace")]
    public void ReplaceManipulator() =>
        Assert.Equal<int>(
            [1],
            Run(
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

    [Fact(DisplayName = "Adjusting text: several together")]
    public void SeveralManipulators() =>
        Assert.Equal<int>(
            [3],
            Run(
                Cond(
                    "Name",
                    "Equal",
                    ["ADA LOVELACE"],
                    manipulators:
                    [
                        new { name = "Trim" },
                        new { name = "ToUpper" },
                    ]
                )
            )
        );

    #endregion Manipulators

    #region Connectors and groups

    [Fact(DisplayName = "Connectors join an item to the next")]
    public void Connectors()
    {
        Assert.Equal<int>(
            [1, 2],
            Run(
                Cond("Name", "Contains", ["alan"], connector: "Or"),
                Cond("Name", "Contains", ["grace"])
            )
        );
        Assert.Empty(
            Run(
                Cond("Name", "Contains", ["alan"], connector: "And"),
                Cond("Name", "Contains", ["grace"])
            )
        );
    }

    [Fact(
        DisplayName = "A connector defaults to And, and is not case sensitive"
    )]
    public void ConnectorDefaults()
    {
        Assert.Empty(
            Run(
                Cond("Name", "Contains", ["alan"]),
                Cond("Name", "Contains", ["grace"])
            )
        );
        Assert.Equal<int>(
            [1, 2],
            Run(
                Cond("Name", "Contains", ["alan"], connector: "or"),
                Cond("Name", "Contains", ["grace"])
            )
        );
    }

    [Fact(DisplayName = "Groups keep their conditions together")]
    public void Groups() =>
        Assert.Equal<int>(
            [2],
            Run(
                Group(
                    [
                        Cond("Name", "Contains", ["alan"], connector: "Or"),
                        Cond("Name", "Contains", ["grace"]),
                    ],
                    connector: "And"
                ),
                Cond("Age", "GreaterThan", ["50"])
            )
        );

    [Fact(DisplayName = "Groups can be nested")]
    public void NestedGroups() =>
        Assert.Equal<int>(
            [1, 2],
            Run(
                Group([
                    Group([
                        Cond("Name", "Contains", ["alan"], connector: "Or"),
                        Cond("Name", "Contains", ["grace"]),
                    ]),
                ])
            )
        );

    [Fact(DisplayName = "A group can filter the items of a collection")]
    public void CollectionGroups()
    {
        Assert.Equal<int>(
            [1, 2],
            Run(
                Group(
                    [Cond("Type", "Equal", ["Email"])],
                    collection: "Contacts"
                )
            )
        );
        Assert.Equal<int>(
            [1],
            Run(
                Group(
                    [Cond("Value", "StartsWith", ["0123"])],
                    collection: "Contacts"
                )
            )
        );
    }

    [Fact(
        DisplayName = "Conditions on one item of a collection are tested together"
    )]
    public void CollectionConditionsApplyToTheSameItem()
    {
        // Person 1 has an email contact, and a telephone contact with a comment, but no email with one
        Assert.Empty(
            Run(
                Group(
                    [
                        Cond("Type", "Equal", ["Email"]),
                        Cond("Comments", "IsNotNull"),
                    ],
                    collection: "Contacts"
                )
            )
        );
    }

    [Fact(
        DisplayName = "A collection of plain values is filtered by its items"
    )]
    public void ScalarCollections()
    {
        Assert.Equal<int>(
            [1, 4],
            Run(Group([Cond("", "Equal", ["maths"])], collection: "Tags"))
        );
        Assert.Equal<int>(
            [2],
            Run(Group([Cond("", "Contains", ["cob"])], collection: "Tags"))
        );
    }

    [Fact(DisplayName = "A collection of a related object can be filtered")]
    public void CollectionsOfRelatedObjects() =>
        Assert.Equal<int>(
            [1],
            Run(
                Group(
                    [Cond("Staff", "GreaterThan", ["100"])],
                    collection: "Employer.Branches"
                )
            )
        );

    [Fact(
        DisplayName = "A collection group can hold a further collection group"
    )]
    public void NestedCollections() =>
        Assert.Equal<int>(
            [],
            Run(
                Group(
                    [
                        Group(
                            [Cond("Value", "Equal", ["x"])],
                            collection: "Owner.Contacts"
                        ),
                    ],
                    collection: "Contacts"
                )
            )
        );

    [Fact(
        DisplayName = "Nothing posted means no conditions, which matches everything"
    )]
    public void NothingMatchesEverything()
    {
        foreach (var json in new string?[] { null, "", "  ", Doc() })
        {
            var result = Build(json);

            Assert.True(result.Succeeded);
            Assert.Equal<int>([1, 2, 3, 4], People.Where(result.Filter!).Ids());
        }
    }

    #endregion Connectors and groups

    #region Every operation works on every kind it is offered for

    public static TheoryData<string, string> OfferedOperations()
    {
        var data = new TheoryData<string, string>();
        var schema = FilterSchema.For<Person>();

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
        DisplayName = "An operation that is offered for a property builds and runs"
    )]
    [MemberData(nameof(OfferedOperations))]
    public void OfferedOperationsWork(string path, string operationName)
    {
        var schema = FilterSchema.For<Person>();
        var field = schema
            .Types[schema.Root]
            .Fields.Single(f => f.Path == path);
        var operation = schema.Operations.Single(o => o.Name == operationName);

        var result = Build(
            Doc(Cond(path, operationName, ValuesFor(field, operation)))
        );

        Assert.True(result.Succeeded, string.Join("; ", result.Errors));
        Assert.Null(
            Record.Exception(() => People.Where(result.Filter!).ToList())
        );

        // And as a database would see it: only the expression, not a compiled delegate
        Assert.Null(
            Record.Exception(() =>
                People.AsQueryable().Where(result.Filter!).ToList()
            )
        );
    }

    #endregion Every operation works on every kind it is offered for

    #region Not trusting what is posted

    [Theory(
        DisplayName = "A posted filter that can't be used is refused, and says why"
    )]
    [InlineData("{", "could not be read")]
    [InlineData("[]", "could not be read")]
    [InlineData("\"text\"", "could not be read")]
    [InlineData(
        "{\"version\":2,\"root\":{\"type\":\"group\"}}",
        "not in a form"
    )]
    [InlineData("{\"version\":1}", "not in a form")]
    [InlineData(
        "{\"version\":1,\"root\":{\"type\":\"condition\"}}",
        "must start with a group"
    )]
    public void RefusesUnreadableFilters(string json, string expected)
    {
        var result = Build(json);

        Assert.False(result.Succeeded);
        Assert.Null(result.Filter);
        Assert.Contains(result.Errors, e => e.Message.Contains(expected));
    }

    public static TheoryData<string, string> BadConditions() =>
        new()
        {
            {
                Doc(Cond("Secret", "Equal", ["x"])),
                "\"Secret\" is not a property"
            },
            {
                Doc(Cond("name", "Contains", ["x"])),
                "\"name\" is not a property"
            },
            {
                Doc(Cond("Name.Length", "Equal", ["1"])),
                "\"Name.Length\" is not a property"
            },
            { Doc(Cond("GetType()", "Equal", ["1"])), "is not a property" },
            { Doc(Cond("", "Equal", ["1"])), "is not a property" },
            { Doc(Cond("Photo", "IsNull")), "is not a property" },
            {
                Doc(Cond("Home.Country.Name", "Equal", ["UK"])),
                "is not a property"
            },
            { Doc(Cond("Contacts", "IsNull")), "is not a property" },
            {
                Doc(Cond("Salary", "Contains", ["1"])),
                "cannot be used on Salary"
            },
            {
                Doc(Cond("Name", "GreaterThan", ["a"])),
                "cannot be used on Name"
            },
            { Doc(Cond("Id", "IsNull")), "cannot be used on Id" },
            { Doc(Cond("Name", "equal", ["a"])), "cannot be used" },
            { Doc(Cond("Name", "Bogus", ["a"])), "\"Bogus\" cannot be used" },
            { Doc(Cond("Name", "", ["a"])), "cannot be used" },
            { Doc(Cond("Name", "Equal", [])), "needs one value" },
            { Doc(Cond("Name", "Equal", ["a", "b"])), "needs one value" },
            { Doc(Cond("Age", "Between", ["1"])), "needs two values" },
            {
                Doc(Cond("Age", "Between", ["1", "2", "3"])),
                "needs two values"
            },
            { Doc(Cond("Name", "Contains", [])), "needs at least one value" },
            { Doc(Cond("Age", "Equal", ["abc"])), "not a valid number" },
            { Doc(Cond("Age", "Equal", ["1e5"])), "not a valid number" },
            { Doc(Cond("Age", "Equal", ["1.5"])), "not a valid number" },
            {
                Doc(Cond("Age", "Equal", ["99999999999"])),
                "not a valid number"
            },
            { Doc(Cond("Salary", "Equal", ["NaN"])), "not a valid number" },
            { Doc(Cond("Score", "Equal", ["Infinity"])), "not a valid number" },
            { Doc(Cond("Born", "Equal", ["yesterday"])), "not a valid date" },
            { Doc(Cond("Born", "Equal", ["01/02/2000"])), "not a valid date" },
            {
                Doc(Cond("Joined", "Equal", ["2000-13-01"])),
                "not a valid date"
            },
            { Doc(Cond("StartsAt", "Equal", ["25:00"])), "not a valid time" },
            { Doc(Cond("Duration", "Equal", ["soon"])), "not a valid time" },
            { Doc(Cond("Active", "Equal", ["yes"])), "not a valid boolean" },
            { Doc(Cond("Favourite", "Equal", ["Purple"])), "not a valid enum" },
            { Doc(Cond("Favourite", "Equal", ["1"])), "not a valid enum" },
            {
                Doc(Cond("Favourite", "Equal", ["Red, Blue"])),
                "not a valid enum"
            },
            { Doc(Cond("Reference", "Equal", ["nope"])), "not a valid guid" },
            { Doc(Cond("Name", "Equal", [""])), "A value is required" },
            {
                Doc(Cond("Name", "Contains", ["ok", ""])),
                "A value is required"
            },
            {
                Doc(Cond("Name", "Equal", [new string('x', 501)])),
                "at most 500 characters"
            },
            {
                Doc(Cond("Name", "Contains", ["a"], match: "Some")),
                "any, or all"
            },
            {
                Doc(Cond("Name", "Equal", ["a"], connector: "Xor")),
                "can only be joined with And or Or"
            },
            {
                Doc(
                    Cond(
                        "Id",
                        "Equal",
                        ["1"],
                        manipulators: [new { name = "Trim" }]
                    )
                ),
                "can't be used on"
            },
            {
                Doc(
                    Cond(
                        "Name",
                        "Equal",
                        ["a"],
                        manipulators: [new { name = "ToString" }]
                    )
                ),
                "not available"
            },
            {
                Doc(
                    Cond(
                        "Name",
                        "Equal",
                        ["a"],
                        manipulators: [new { name = "Replace" }]
                    )
                ),
                "is missing something"
            },
            {
                Doc(
                    Cond(
                        "Name",
                        "Equal",
                        ["a"],
                        manipulators:
                        [
                            new { name = "Replace", args = new[] { "", "x" } },
                        ]
                    )
                ),
                "needs: Find"
            },
            {
                Doc(
                    Cond(
                        "Name",
                        "Equal",
                        ["a"],
                        manipulators:
                        [
                            new { name = "Trim" },
                            new { name = "Trim" },
                            new { name = "Trim" },
                            new { name = "Trim" },
                            new { name = "Trim" },
                            new { name = "Trim" },
                            new { name = "Trim" },
                            new { name = "Trim" },
                            new { name = "Trim" },
                        ]
                    )
                ),
                "Too many"
            },
            { Doc(new { type = "other" }), "must be a condition or a group" },
            {
                Doc(new { type = (string?)null }),
                "must be a condition or a group"
            },
            {
                Doc(Group([Cond("Name", "Equal", ["a"])], collection: "Name")),
                "not a collection"
            },
            {
                Doc(Group([Cond("Name", "Equal", ["a"])], collection: "Nope")),
                "not a collection"
            },
            {
                Doc(Group([Cond("Name", "Equal", ["a"])], collection: "Home")),
                "not a collection"
            },
            { Doc(Group([])), "at least one condition" },
            {
                Doc(Group([], collection: "Contacts")),
                "at least one condition"
            },
            {
                Doc(
                    Group(
                        [Cond("Name", "Equal", ["a"])],
                        collection: "Contacts"
                    )
                ),
                "is not a property"
            },
        };

    [Theory(
        DisplayName = "A condition that doesn't add up is refused, and says why"
    )]
    [MemberData(nameof(BadConditions))]
    public void RefusesBadConditions(string json, string expected)
    {
        var result = Build(json);

        Assert.False(result.Succeeded);
        Assert.Null(result.Filter);
        Assert.Contains(result.Errors, e => e.Message.Contains(expected));
    }

    [Fact(DisplayName = "A null item in a list is refused")]
    public void RefusesNullItems()
    {
        var result = Build(
            "{\"version\":1,\"root\":{\"type\":\"group\",\"items\":[null]}}"
        );

        Assert.Contains(result.Errors, e => e.Message.Contains("missing"));
    }

    [Fact(DisplayName = "Errors say where they are")]
    public void ErrorsHaveAPath()
    {
        var result = Build(
            Doc(
                Cond("Name", "Equal", ["fine"]),
                Group([
                    Cond("Age", "Equal", ["5"]),
                    Cond("Age", "Equal", ["not a number"]),
                ])
            )
        );

        var error = Assert.Single(result.Errors);

        Assert.Equal("root.items[1].items[1].values[0]", error.Path);
    }

    [Fact(DisplayName = "Every problem is reported, not only the first")]
    public void ReportsEveryProblem()
    {
        var result = Build(
            Doc(
                Cond("Nope", "Equal", ["1"]),
                Cond("Age", "Equal", ["x"]),
                Cond("Name", "Bogus", ["x"])
            )
        );

        Assert.Equal(3, result.Errors.Count);
    }

    [Fact(DisplayName = "GetFilterOrThrow explains what is wrong")]
    public void GetFilterOrThrow()
    {
        var bad = Build(Doc(Cond("Nope", "Equal", ["1"])));
        var good = Build(Doc(Cond("Name", "Equal", ["a"])));

        var exception = Assert.Throws<InvalidOperationException>(
            bad.GetFilterOrThrow
        );

        Assert.Contains("Nope", exception.Message);
        Assert.NotNull(good.GetFilterOrThrow());
    }

    #endregion Not trusting what is posted

    #region Limits

    [Fact(DisplayName = "A filter has a limit on its size")]
    public void LimitsThePayload()
    {
        var result = Build(
            Doc(Cond("Name", "Equal", ["a"])),
            new() { MaxPayloadLength = 20 }
        );

        Assert.Contains(result.Errors, e => e.Message.Contains("too large"));
    }

    [Fact(DisplayName = "A filter has a limit on its conditions")]
    public void LimitsConditions()
    {
        var conditions = Enumerable
            .Range(0, 6)
            .Select(_ => Cond("Name", "Equal", ["a"]))
            .ToArray();

        var allowed = Build(Doc(conditions[..5]), new() { MaxConditions = 5 });
        var tooMany = Build(Doc(conditions), new() { MaxConditions = 5 });

        Assert.True(allowed.Succeeded);
        Assert.False(tooMany.Succeeded);
        Assert.Single(tooMany.Errors);
        Assert.Contains("at most 5 conditions", tooMany.Errors[0].Message);
    }

    [Fact(DisplayName = "A condition has a limit on its values")]
    public void LimitsValues()
    {
        var values = Enumerable.Range(1, 4).Select(i => i.ToString()).ToArray();

        var allowed = Build(
            Doc(Cond("Id", "In", values)),
            new() { MaxValuesPerCondition = 4 }
        );
        var tooMany = Build(
            Doc(Cond("Id", "In", [.. values, "5"])),
            new() { MaxValuesPerCondition = 4 }
        );

        Assert.True(allowed.Succeeded);
        Assert.Contains(
            tooMany.Errors,
            e => e.Message.Contains("at most 4 values")
        );
    }

    [Fact(DisplayName = "Groups can only be nested so far")]
    public void LimitsGroupDepth()
    {
        object Nest(int groups) =>
            groups == 0
                ? Cond("Name", "Equal", ["a"])
                : Group([Nest(groups - 1)]);

        // The top level group counts as the first
        Assert.True(Build(Doc(Nest(2)), new() { MaxGroupDepth = 3 }).Succeeded);

        var tooDeep = Build(Doc(Nest(3)), new() { MaxGroupDepth = 3 });

        Assert.Contains(
            tooDeep.Errors,
            e => e.Message.Contains("nested too deeply")
        );
    }

    [Fact(DisplayName = "Deeply nested JSON is refused rather than crashing")]
    public void HugeNestingDoesNotCrash()
    {
        var json =
            string.Concat(Enumerable.Repeat("{\"items\":[", 200))
            + string.Concat(Enumerable.Repeat("]}", 200));

        var result = Build(json, new() { MaxPayloadLength = 100_000 });

        Assert.False(result.Succeeded);
    }

    #endregion Limits

    #region Showing a saved filter

    [Fact(
        DisplayName = "A filter that was built in code can be shown, and gives the same results"
    )]
    public void RoundTrip()
    {
        var filter = new Filter<Person>();
        filter
            .OpenGroup()
            .Contains(x => x.Name, ["alan", "grace"])
            .Or()
            .Equal(x => x.Name.ToLower(), "ada lovelace")
            .CloseGroup()
            .And()
            .OpenCollection(x => x.Contacts)
            .Equal(x => x.Type, ContactType.Email)
            .CloseCollection<Person>()
            .And()
            .Between(x => x.Age, 20, 90)
            .And()
            .GreaterThan(x => x.Born, new DateTime(1900, 1, 1))
            .And()
            .IsNotNull(x => x.Home.Postcode);

        var model = FilterBuilderModel.FromFilter(filter);
        var rebuilt = model.Build<Person>();

        Assert.True(rebuilt.Succeeded, string.Join("; ", rebuilt.Errors));
        Assert.Equal(
            People.Where(filter).Ids(),
            People.Where(rebuilt.Filter!).Ids()
        );
        Assert.Equal(filter.ToString(), rebuilt.Filter!.ToString());
    }

    [Fact(
        DisplayName = "Showing a filter keeps its values, options and connectors"
    )]
    public void RoundTripKeepsDetails()
    {
        var filter = new Filter<Person>();
        filter
            .Contains(
                x => x.Name,
                ["a", "b"],
                new FilterStatementOptions { Match = Matches.All },
                Connector.Or
            )
            .Between(
                x => x.LastSeen,
                new DateTime(2020, 1, 2, 3, 4, 5),
                new DateTime(2025, 1, 1)
            )
            .And()
            .Equal(x => x.Favourite, Colour.Blue)
            .And()
            .Equal(x => x.Reference, GuidTwo)
            .And()
            .Equal(x => x.Active, true)
            .And()
            .LessThan(x => x.Joined, new DateOnly(2001, 2, 3))
            .And()
            .LessThan(x => x.StartsAt, new TimeOnly(10, 11, 12))
            .And()
            .LessThan(x => x.Duration, TimeSpan.FromMinutes(61))
            .And()
            .GreaterThan(x => x.Salary, 12.5m);

        var json = FilterBuilderModel.FromFilter(filter).Json!;
        var rebuilt = new FilterBuilderModel { Json = json }.Build<Person>();

        Assert.True(rebuilt.Succeeded, string.Join("; ", rebuilt.Errors));
        Assert.Equal(filter.ToString(), rebuilt.Filter!.ToString());
        Assert.Contains("\"match\":\"All\"", json);
        Assert.Contains("\"connector\":\"Or\"", json);
        Assert.Contains("2020-01-02T03:04:05", json);
    }

    [Fact(DisplayName = "A replace adjustment survives being shown")]
    public void RoundTripReplace()
    {
        var filter = new Filter<Person>();
        filter.Equal(
            "Name",
            ["AlanTuring"],
            new FilterStatementOptions
            {
                Manipulators = [new ReplaceManipulator(" ", "")],
            }
        );

        var rebuilt = FilterBuilderModel.FromFilter(filter).Build<Person>();

        Assert.Equal<int>([1], People.Where(rebuilt.Filter!).Ids());
    }

    [Fact(
        DisplayName = "A filter using something the builder doesn't offer can't be shown"
    )]
    public void RefusesToMisrepresentAFilter()
    {
        var filter = new Filter<Person>();
        filter.Equal(x => x.Secret, "enigma");

        Assert.Throws<NotSupportedException>(() =>
            FilterBuilderModel.FromFilter(filter)
        );
    }

    [Fact(
        DisplayName = "A manipulator the builder doesn't know can't be shown"
    )]
    public void RefusesUnknownManipulators()
    {
        var filter = new Filter<Person>();
        filter.Equal(x => x.Name.Substring(1), "lan Turing");

        Assert.Throws<NotSupportedException>(() =>
            FilterBuilderModel.FromFilter(filter)
        );
    }

    [Fact(DisplayName = "An empty filter is shown as no conditions")]
    public void EmptyFilter()
    {
        var model = FilterBuilderModel.FromFilter(new Filter<Person>());

        Assert.True(model.IsEmpty);
        Assert.True(model.Build<Person>().Succeeded);
    }

    [Fact(DisplayName = "A null filter is rejected")]
    public void NullFilter() =>
        Assert.Throws<ArgumentNullException>(() =>
            FilterBuilderModel.FromFilter<Person>(null!)
        );

    #endregion Showing a saved filter

    [Theory(DisplayName = "IsEmpty: whether any conditions were posted")]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData("   ", true)]
    [InlineData(
        "{\"version\":1,\"root\":{\"type\":\"group\",\"items\":[]}}",
        true
    )]
    [InlineData("{\"version\":1,\"root\":{\"type\":\"group\"}}", true)]
    [InlineData(
        "{\"version\":1,\"root\":{\"type\":\"group\",\"items\":[{\"type\":\"group\"}]}}",
        false
    )]
    [InlineData("not json", false)]
    public void IsEmpty(string? json, bool expected) =>
        Assert.Equal(expected, new FilterBuilderModel { Json = json }.IsEmpty);
}
