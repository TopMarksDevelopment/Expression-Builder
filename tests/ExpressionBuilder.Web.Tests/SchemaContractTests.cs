namespace ExpressionBuilder.Web.Tests;

using System.Text.Json;
using ExpressionBuilder.Web.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder.Web;

/// <summary>
/// The server and the script agree on the shape of the schema through a fixture that both read: the
/// script's tests run against it, and this checks it is what the server really produces.
/// </summary>
/// <remarks>
/// If this fails after you changed the schema, run the tests once with the environment variable
/// <c>UPDATE_FIXTURES=1</c> to rewrite the fixture, check the script's tests still pass, and commit it.
/// </remarks>
[Collection("Operation catalog")]
public class SchemaContractTests
{
    static string FixturePath()
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
            "person-schema.json"
        );
    }

    [Fact(
        DisplayName = "The schema the script's tests use is what the server produces"
    )]
    public void FixtureMatchesTheServer()
    {
        var path = FixturePath();
        var current = Format(FilterSchema.For<Person>().ToJson());

        if (Environment.GetEnvironmentVariable("UPDATE_FIXTURES") == "1")
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, current);
        }

        Assert.True(
            File.Exists(path),
            $"Missing fixture: {path}. Run the tests with UPDATE_FIXTURES=1."
        );
        Assert.Equal(Normalise(current), Normalise(File.ReadAllText(path)));
    }

    static string Format(string json) =>
        JsonSerializer.Serialize(
            JsonDocument.Parse(json).RootElement,
            new JsonSerializerOptions { WriteIndented = true }
        ) + "\n";

    static string Normalise(string text) => text.Replace("\r\n", "\n");
}
