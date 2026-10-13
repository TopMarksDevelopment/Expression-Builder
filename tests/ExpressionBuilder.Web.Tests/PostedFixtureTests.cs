namespace ExpressionBuilder.Web.Tests;

using ExpressionBuilder.Web.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder.Web;

/// <summary>
/// The other half of the script's contract tests: the documents the script posts when a user fills in the
/// interface (<c>client/test/fixtures/posted</c>) must be accepted and must find the right people.
/// </summary>
[Collection("Operation catalog")]
public class PostedFixtureTests
{
    static string FixtureFolder()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (
            directory != null
            && !File.Exists(
                Path.Combine(directory.FullName, "ExpressionBuilder.sln")
            )
        )
            directory = directory.Parent;

        Assert.NotNull(directory);

        return Path.Combine(
            directory.FullName,
            "src",
            "ExpressionBuilder.Web",
            "client",
            "test",
            "fixtures",
            "posted"
        );
    }

    /// <summary>The fixture, and who it should find in <see cref="TestData.People"/>.</summary>
    public static TheoryData<string, int[]> Scenarios =>
        new()
        {
            // Name contains "turing" and "alan", ignoring case and spaces
            { "text-all-terms", [1] },
            // ((Age 20-45 and born before 1915) or favourite red/blue) and active and not G2
            { "kinds-and-connectors", [1] },
            // Has an email contact, lives in M1 and has an email address
            { "collection-related-and-group", [1] },
            // Is tagged "maths" and has been verified (either way)
            { "plain-values-and-null", [1, 4] },
        };

    [Theory(
        DisplayName = "What the script posts is accepted and finds the right people"
    )]
    [MemberData(nameof(Scenarios))]
    public void ScriptOutputIsAcceptedByTheServer(string name, int[] expected)
    {
        var json = File.ReadAllText(
            Path.Combine(FixtureFolder(), name + ".json")
        );

        var result = new FilterBuilderModel { Json = json }.Build<Person>();

        Assert.True(result.Succeeded, string.Join("; ", result.Errors));
        Assert.Equal<int>(
            expected,
            TestData.People().Where(result.Filter!).Ids()
        );
    }

    [Fact(
        DisplayName = "Every fixture the script writes has an expectation here"
    )]
    public void NoFixtureIsLeftUntested()
    {
        var known = Scenarios.Select(s => (string)s[0]).Order().ToList();
        var written = Directory
            .GetFiles(FixtureFolder(), "*.json")
            .Select(f => Path.GetFileNameWithoutExtension(f))
            .Order()
            .ToList();

        Assert.Equal<string>(known, written);
    }
}
