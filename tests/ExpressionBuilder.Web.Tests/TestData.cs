namespace ExpressionBuilder.Web.Tests;

using System.Text.Json;
using ExpressionBuilder.Web.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder.Web;

/// <summary>
/// People to filter, and a short way of writing the filters the browser posts.
/// </summary>
static class TestData
{
    public static readonly Guid GuidOne = Guid.Parse(
        "11111111-1111-1111-1111-111111111111"
    );
    public static readonly Guid GuidTwo = Guid.Parse(
        "22222222-2222-2222-2222-222222222222"
    );

    public static List<Person> People() =>
        [
            new()
            {
                Id = 1,
                Name = "Alan Turing",
                Email = "alan@example.com",
                Age = 41,
                Salary = 1000m,
                Score = 1.5,
                Born = new DateTime(1912, 6, 23),
                LastSeen = new DateTime(2024, 1, 10, 9, 30, 0),
                Joined = new DateOnly(2000, 1, 1),
                StartsAt = new TimeOnly(9, 0),
                Duration = TimeSpan.FromMinutes(90),
                Active = true,
                Verified = true,
                Favourite = Colour.Red,
                Reference = GuidOne,
                Home = new()
                {
                    Postcode = "M1 1AA",
                    City = "Manchester",
                    Country = new() { Name = "UK" },
                },
                Employer = new()
                {
                    Name = "Bletchley",
                    Branches =
                    [
                        new() { Town = "Bletchley", Staff = 500 },
                        new() { Town = "London", Staff = 20 },
                    ],
                },
                Contacts =
                [
                    new()
                    {
                        Type = ContactType.Email,
                        Value = "alan@example.com",
                    },
                    new()
                    {
                        Type = ContactType.Telephone,
                        Value = "0123 456",
                        Comments = "Home",
                    },
                ],
                Tags = ["maths", "codes"],
                Secret = "enigma",
            },
            new()
            {
                Id = 2,
                Name = "Grace Hopper",
                Email = null,
                Age = 85,
                Salary = 2500.5m,
                Score = 9.25,
                Born = new DateTime(1906, 12, 9),
                LastSeen = null,
                Joined = new DateOnly(1950, 5, 5),
                StartsAt = new TimeOnly(7, 30),
                Duration = TimeSpan.FromHours(3),
                Active = true,
                Verified = false,
                Favourite = Colour.Blue,
                Reference = GuidTwo,
                Home = new()
                {
                    Postcode = "NY 100",
                    City = null,
                    Country = new() { Name = "USA" },
                },
                Employer = null,
                Contacts =
                [
                    new()
                    {
                        Type = ContactType.Email,
                        Value = "grace@navy.mil",
                    },
                ],
                Tags = ["cobol"],
                Secret = "compiler",
            },
            new()
            {
                Id = 3,
                Name = "  Ada Lovelace ",
                Email = "ada@example.org",
                Age = null,
                Salary = 300m,
                Score = -2,
                Born = new DateTime(1815, 12, 10),
                LastSeen = new DateTime(2023, 12, 25, 18, 0, 0),
                Joined = new DateOnly(1843, 7, 10),
                StartsAt = new TimeOnly(23, 59, 30),
                Duration = TimeSpan.Zero,
                Active = false,
                Verified = null,
                Favourite = Colour.Green,
                Reference = Guid.Empty,
                Home = new()
                {
                    Postcode = "W1 2BB",
                    City = "London",
                    Country = new() { Name = "UK" },
                },
                Employer = new() { Name = "Babbage & Co", Branches = [] },
                Contacts = [],
                Tags = [],
                Secret = "notes",
            },
            new()
            {
                Id = 4,
                Name = "",
                Email = "",
                Age = 20,
                Salary = 0m,
                Score = 0,
                Born = new DateTime(2004, 4, 4),
                LastSeen = new DateTime(2024, 1, 10, 9, 30, 0),
                Joined = new DateOnly(2020, 2, 29),
                StartsAt = new TimeOnly(0, 0),
                Duration = TimeSpan.FromMinutes(1),
                Active = false,
                Verified = true,
                Favourite = Colour.Red,
                Reference = GuidOne,
                Home = new()
                {
                    Postcode = "",
                    City = "",
                    Country = null,
                },
                Employer = new()
                {
                    Name = "Tiny",
                    Branches = [new() { Town = "Leeds", Staff = 3 }],
                },
                Contacts =
                [
                    new()
                    {
                        Type = ContactType.Telephone,
                        Value = "999",
                        Comments = null,
                    },
                ],
                Tags = ["new", "maths"],
                Secret = "",
            },
        ];

    static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System
            .Text
            .Json
            .Serialization
            .JsonIgnoreCondition
            .WhenWritingNull,
    };

    /// <summary>Values that suit a property and an operation, for trying every combination.</summary>
    public static string[] ValuesFor(
        FieldSchema field,
        OperationSchema operation
    )
    {
        var one = field.Kind switch
        {
            FieldKind.Number => "5",
            FieldKind.Date => "2000-01-01",
            FieldKind.DateTime => "2000-01-01T10:00",
            FieldKind.Time => "10:00",
            FieldKind.Boolean => "true",
            FieldKind.Enum => field.Options![0].Value,
            FieldKind.Guid => GuidOne.ToString(),
            _ => "a",
        };

        return operation.Arity switch
        {
            ValueArity.None => [],
            ValueArity.One => [one],
            ValueArity.Two => [one, one],
            _ => [one, one],
        };
    }

    /// <summary>A document with the given items at the top.</summary>
    public static string Doc(params object[] items) =>
        JsonSerializer.Serialize(
            new
            {
                version = 1,
                root = new
                {
                    type = "group",
                    connector = "And",
                    items,
                },
            },
            Json
        );

    /// <summary>A condition, as the browser posts it.</summary>
    public static object Cond(
        string field,
        string operation,
        string[]? values = null,
        string? connector = null,
        string? match = null,
        object[]? manipulators = null
    ) =>
        new
        {
            type = "condition",
            field,
            operation,
            values = values ?? [],
            connector,
            match,
            manipulators,
        };

    /// <summary>A group, as the browser posts it.</summary>
    public static object Group(
        object[] items,
        string? collection = null,
        string? connector = null
    ) =>
        new
        {
            type = "group",
            collection,
            connector,
            items,
        };

    public static List<int> Ids(this IEnumerable<Person> people) =>
        [.. people.Select(p => p.Id).Order()];
}
