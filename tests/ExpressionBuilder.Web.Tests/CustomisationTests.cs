namespace ExpressionBuilder.Web.Tests;

using System.Linq.Expressions;
using ExpressionBuilder.Web.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Web;
using static ExpressionBuilder.Web.Tests.TestData;

/// <summary>
/// Operations and manipulators of your own: offered only when the app has described them, kept apart from
/// the built-in ones, and never reachable by a name the app did not register.
/// </summary>
[Collection("Operation catalog")]
public class CustomisationTests
{
    static readonly List<Person> People = TestData.People();

    static readonly HashSet<FieldKind> Text = [FieldKind.String];

    static FilterBuildResult<Person> Build(
        string json,
        FilterBuilderOptions? options = null
    ) => new FilterBuilderModel { Json = json }.Build<Person>(options);

    #region Operations

    [Fact(DisplayName = "A built-in operation can't be replaced or forgotten")]
    public void BuiltInOperationsAreFixed()
    {
        Assert.Throws<InvalidOperationException>(() =>
            OperationCatalog.Register(
                new OperationDescriptor("Equal", "Same", ValueArity.One, Text)
            )
        );
        Assert.False(OperationCatalog.Unregister("Equal"));
        Assert.True(OperationCatalog.TryGet("Equal", out var equal));
        Assert.True(equal.IsBuiltIn);
        Assert.Throws<InvalidOperationException>(() =>
            new FilterBuilderOptions().AddOperation(
                new OperationDescriptor("Equal", "Same", ValueArity.One, Text)
            )
        );
    }

    [Fact(DisplayName = "Built-in operations have no group; custom ones do")]
    public void OperationGroups()
    {
        OperationCatalog.Register<ShortNameOperation>(Described());
        OperationCatalog.Register(
            new OperationDescriptor(
                "Shouts",
                "Is shouted",
                ValueArity.None,
                Text,
                Group: "Fun"
            )
        );
        OperationRegistry.Register<ShoutsOperation>();

        try
        {
            var operations = FilterSchema.For<Person>().Operations;

            Assert.All(
                operations.Where(o =>
                    OperationCatalog.TryGet(o.Name, out var d) && d.IsBuiltIn
                ),
                o => Assert.Null(o.Group)
            );
            Assert.Equal(
                "Custom",
                operations.Single(o => o.Name == "IsShortName").Group
            );
            Assert.Equal(
                "Fun",
                operations.Single(o => o.Name == "Shouts").Group
            );
        }
        finally
        {
            OperationCatalog.Unregister("IsShortName");
            OperationCatalog.Unregister("Shouts");
        }
    }

    [Fact(DisplayName = "The heading for custom operations can be translated")]
    public void CustomGroupLabel()
    {
        OperationCatalog.Register<ShortNameOperation>(Described());

        try
        {
            var options = new FilterBuilderOptions
            {
                Labels = new FilterBuilderLabels { CustomGroup = "Eich un" },
            };

            Assert.Equal(
                "Eich un",
                FilterSchema
                    .For<Person>(options)
                    .Operations.Single(o => o.Name == "IsShortName")
                    .Group
            );
        }
        finally
        {
            OperationCatalog.Unregister("IsShortName");
        }
    }

    [Fact(DisplayName = "A custom operation is posted, built and run")]
    public void CustomOperationEndToEnd()
    {
        OperationCatalog.Register<ShortNameOperation>(Described());

        try
        {
            var result = Build(Doc(Cond("Name", "IsShortName")));

            Assert.True(result.Succeeded, string.Join("; ", result.Errors));
            Assert.Equal(
                People.Where(p => p.Name.Length < 12).Ids(),
                People.Where(result.Filter!).Ids()
            );
        }
        finally
        {
            OperationCatalog.Unregister("IsShortName");
        }
    }

    [Fact(
        DisplayName = "A custom operation that was never described can't be used"
    )]
    public void UndescribedOperationRefused()
    {
        OperationRegistry.Register<ShortNameOperation>();

        var result = Build(Doc(Cond("Name", "IsShortName")));

        Assert.False(result.Succeeded);
        Assert.Contains("cannot be used", result.Errors[0].Message);
    }

    [Fact(DisplayName = "A description must match the operation it is for")]
    public void NamesMustMatch() =>
        Assert.Throws<ArgumentException>(() =>
            OperationCatalog.Register<ShortNameOperation>(
                new OperationDescriptor(
                    "SomethingElse",
                    "Is short",
                    ValueArity.None,
                    Text
                )
            )
        );

    [Fact(
        DisplayName = "One builder can leave an operation out, and the others still have it"
    )]
    public void ExcludeOperation()
    {
        var options = new FilterBuilderOptions().ExcludeOperations(
            "Contains",
            "SmartSearch"
        );

        var schema = FilterSchema.For<Person>(options);

        Assert.DoesNotContain(schema.Operations, o => o.Name == "Contains");
        Assert.DoesNotContain(
            schema
                .Types[schema.Root]
                .Fields.Single(f => f.Path == "Name")
                .Operations,
            n => n == "Contains"
        );
        Assert.Contains(
            FilterSchema.For<Person>().Operations,
            o => o.Name == "Contains"
        );

        var posted = Doc(Cond("Name", "Contains", ["a"]));

        Assert.False(Build(posted, options).Succeeded);
        Assert.True(Build(posted).Succeeded);
    }

    [Fact(
        DisplayName = "One builder can add an operation the others don't have"
    )]
    public void AddOperationToOneBuilder()
    {
        var options =
            new FilterBuilderOptions().AddOperation<ShortNameOperation>(
                Described()
            );
        var posted = Doc(Cond("Name", "IsShortName"));

        Assert.Contains(
            FilterSchema.For<Person>(options).Operations,
            o => o.Name == "IsShortName"
        );
        Assert.DoesNotContain(
            FilterSchema.For<Person>().Operations,
            o => o.Name == "IsShortName"
        );
        Assert.True(Build(posted, options).Succeeded);
        Assert.False(Build(posted).Succeeded);
    }

    [Fact(
        DisplayName = "An operation added to one builder is not offered unless it can be found"
    )]
    public void AddedOperationMustBeRegistered()
    {
        var options = new FilterBuilderOptions().AddOperation(
            new OperationDescriptor(
                "NobodyHasThisOne",
                "Nothing",
                ValueArity.None,
                Text
            )
        );

        Assert.DoesNotContain(
            FilterSchema.For<Person>(options).Operations,
            o => o.Name == "NobodyHasThisOne"
        );
    }

    #endregion Operations

    #region Manipulators

    [Fact(
        DisplayName = "A built-in manipulator can't be replaced or forgotten"
    )]
    public void BuiltInManipulatorsAreFixed()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ManipulatorCatalog.Register(
                new ManipulatorDescriptor(
                    "Trim",
                    "Trim",
                    _ => new LeftManipulator()
                )
            )
        );
        Assert.False(ManipulatorCatalog.Unregister("Trim"));
        Assert.Throws<InvalidOperationException>(() =>
            new FilterBuilderOptions().AddManipulator(
                new ManipulatorDescriptor(
                    "Replace",
                    "Replace",
                    _ => new LeftManipulator()
                )
            )
        );
    }

    [Fact(
        DisplayName = "A manipulator's name is kept to letters, digits and underscores"
    )]
    public void ManipulatorNamesAreRestricted()
    {
        foreach (var name in new[] { "", "1Up", "a b", "a\"b", "a-b", "<x>" })
            Assert.Throws<ArgumentException>(() =>
                new ManipulatorDescriptor(name, "X", _ => new LeftManipulator())
            );
    }

    [Fact(
        DisplayName = "The schema lists the built-in manipulators, then the custom ones under their own heading"
    )]
    public void ManipulatorSchema()
    {
        ManipulatorCatalog.Register<LeftManipulator>(
            "First letters",
            [new ManipulatorParameter("How many")]
        );

        try
        {
            var manipulators = FilterSchema.For<Person>().Manipulators;

            Assert.Equal(
                ["Trim", "ToLower", "ToUpper", "Replace", "Left"],
                manipulators.Select(m => m.Name)
            );
            Assert.All(manipulators.Take(4), m => Assert.Null(m.Group));

            var custom = manipulators.Last();

            Assert.Equal("Custom", custom.Group);
            Assert.Equal("First letters", custom.Label);
            Assert.Equal(["How many"], custom.Parameters.Select(p => p.Label));
            Assert.Equal([FieldKind.String], custom.Kinds);

            var replace = manipulators.Single(m => m.Name == "Replace");

            Assert.Equal(
                ["Find", "Replace with"],
                replace.Parameters.Select(p => p.Label)
            );
            Assert.True(replace.Parameters[0].Required);
            Assert.False(replace.Parameters[1].Required);
        }
        finally
        {
            ManipulatorCatalog.Unregister("Left");
        }
    }

    [Fact(
        DisplayName = "The built-in manipulators take their text from the labels"
    )]
    public void BuiltInManipulatorsAreTranslated()
    {
        var options = new FilterBuilderOptions
        {
            Labels = new FilterBuilderLabels
            {
                Trim = "Tocio",
                ReplaceFind = "Dod o hyd",
            },
        };

        options.ManipulatorLabels["ToLower"] = "Llythrennau bach";

        var manipulators = FilterSchema.For<Person>(options).Manipulators;

        Assert.Equal("Tocio", manipulators.Single(m => m.Name == "Trim").Label);
        Assert.Equal(
            "Llythrennau bach",
            manipulators.Single(m => m.Name == "ToLower").Label
        );
        Assert.Equal(
            "Dod o hyd",
            manipulators.Single(m => m.Name == "Replace").Parameters[0].Label
        );
    }

    [Fact(DisplayName = "A custom manipulator is posted, built and applied")]
    public void CustomManipulatorEndToEnd()
    {
        ManipulatorCatalog.Register<LeftManipulator>(
            "First letters",
            [new ManipulatorParameter("How many")]
        );

        try
        {
            var result = Build(
                Doc(
                    Cond(
                        "Name",
                        "Equal",
                        ["Al"],
                        manipulators:
                        [
                            new { name = "Left", args = new[] { "2" } },
                        ]
                    )
                )
            );

            Assert.True(result.Succeeded, string.Join("; ", result.Errors));
            Assert.Equal<int>([1], People.Where(result.Filter!).Ids());
        }
        finally
        {
            ManipulatorCatalog.Unregister("Left");
        }
    }

    [Fact(
        DisplayName = "A custom manipulator says what was wrong with the text it was given"
    )]
    public void CustomManipulatorExplainsItself()
    {
        ManipulatorCatalog.Register<LeftManipulator>(
            "First letters",
            [new ManipulatorParameter("How many")]
        );

        try
        {
            var result = Build(
                Doc(
                    Cond(
                        "Name",
                        "Equal",
                        ["x"],
                        manipulators:
                        [
                            new { name = "Left", args = new[] { "lots" } },
                        ]
                    )
                )
            );

            Assert.False(result.Succeeded);
            Assert.Equal(
                "Give a whole number of letters.",
                result.Errors[0].Message
            );
        }
        finally
        {
            ManipulatorCatalog.Unregister("Left");
        }
    }

    [Fact(
        DisplayName = "A manipulator that was never described can't be used, whatever it is called"
    )]
    public void UndescribedManipulatorRefused()
    {
        ManipulatorRegistry.Register<LeftManipulator>();

        foreach (
            var name in new[]
            {
                "Left",
                "LeftManipulator",
                "System.IO.File",
                "Substring",
                "TopMarksDevelopment.ExpressionBuilder.ReplaceManipulator",
            }
        )
        {
            var result = Build(
                Doc(
                    Cond(
                        "Name",
                        "Equal",
                        ["x"],
                        manipulators: [new { name, args = new[] { "1" } }]
                    )
                )
            );

            Assert.False(result.Succeeded, name);
            Assert.Contains("not available", result.Errors[0].Message);
        }
    }

    [Fact(
        DisplayName = "A manipulator is refused for a kind of property it doesn't suit"
    )]
    public void ManipulatorKinds()
    {
        ManipulatorCatalog.Register<LeftManipulator>(
            "First letters",
            [new ManipulatorParameter("How many")]
        );

        try
        {
            var result = Build(
                Doc(
                    Cond(
                        "Id",
                        "Equal",
                        ["1"],
                        manipulators:
                        [
                            new { name = "Left", args = new[] { "1" } },
                        ]
                    )
                )
            );

            Assert.False(result.Succeeded);
            Assert.Contains("can't be used on", result.Errors[0].Message);
        }
        finally
        {
            ManipulatorCatalog.Unregister("Left");
        }
    }

    [Fact(
        DisplayName = "One builder can leave a manipulator out, and the others still have it"
    )]
    public void ExcludeManipulator()
    {
        var options = new FilterBuilderOptions().ExcludeManipulators(
            "Replace",
            "ToUpper"
        );

        Assert.DoesNotContain(
            FilterSchema.For<Person>(options).Manipulators,
            m => m.Name == "Replace"
        );
        Assert.Contains(
            FilterSchema.For<Person>().Manipulators,
            m => m.Name == "Replace"
        );

        var posted = Doc(
            Cond(
                "Name",
                "Equal",
                ["a"],
                manipulators: [new { name = "ToUpper" }]
            )
        );

        Assert.False(Build(posted, options).Succeeded);
        Assert.True(Build(posted).Succeeded);
    }

    [Fact(
        DisplayName = "One builder can add a manipulator the others don't have"
    )]
    public void AddManipulatorToOneBuilder()
    {
        var options = new FilterBuilderOptions().AddManipulator(
            ManipulatorDescriptor.For<LeftManipulator>(
                "First letters",
                [new ManipulatorParameter("How many")],
                group: "Text tools"
            )
        );
        var posted = Doc(
            Cond(
                "Name",
                "Equal",
                ["Al"],
                manipulators: [new { name = "Left", args = new[] { "2" } }]
            )
        );

        Assert.Equal(
            "Text tools",
            FilterSchema
                .For<Person>(options)
                .Manipulators.Single(m => m.Name == "Left")
                .Group
        );
        Assert.DoesNotContain(
            FilterSchema.For<Person>().Manipulators,
            m => m.Name == "Left"
        );
        Assert.True(Build(posted, options).Succeeded);
        Assert.False(Build(posted).Succeeded);
    }

    [Fact(DisplayName = "The number of adjustments is limited by the options")]
    public void ManipulatorLimit()
    {
        var posted = Doc(
            Cond(
                "Name",
                "Equal",
                ["a"],
                manipulators: [new { name = "Trim" }, new { name = "ToLower" }]
            )
        );

        Assert.True(Build(posted).Succeeded);
        Assert.False(
            Build(
                posted,
                new FilterBuilderOptions { MaxManipulatorsPerCondition = 1 }
            ).Succeeded
        );
    }

    [Fact(
        DisplayName = "A custom manipulator survives being shown in the builder"
    )]
    public void CustomManipulatorRoundTrip()
    {
        ManipulatorCatalog.Register<LeftManipulator>(
            "First letters",
            [new ManipulatorParameter("How many")]
        );

        try
        {
            var filter = new Filter<Person>();
            filter.Equal(
                "Name",
                ["Al"],
                new FilterStatementOptions
                {
                    Manipulators = [new LeftManipulator { Arguments = ["2"] }],
                }
            );

            var json = FilterBuilderModel.FromFilter(filter).Json!;

            Assert.Contains("\"name\":\"Left\"", json);
            Assert.Contains("\"args\":[\"2\"]", json);

            var rebuilt = new FilterBuilderModel
            {
                Json = json,
            }.Build<Person>();

            Assert.True(rebuilt.Succeeded, string.Join("; ", rebuilt.Errors));
            Assert.Equal<int>([1], People.Where(rebuilt.Filter!).Ids());
        }
        finally
        {
            ManipulatorCatalog.Unregister("Left");
        }
    }

    [Fact(
        DisplayName = "A filter using a manipulator that was not described can't be shown"
    )]
    public void UndescribedManipulatorCannotBeShown()
    {
        var filter = new Filter<Person>();
        filter.Equal(
            "Name",
            ["Al"],
            new FilterStatementOptions
            {
                Manipulators = [new LeftManipulator { Arguments = ["2"] }],
            }
        );

        Assert.Throws<NotSupportedException>(() =>
            FilterBuilderModel.FromFilter(filter)
        );
    }

    #endregion Manipulators

    static OperationDescriptor Described() =>
        new("IsShortName", "Has a short name", ValueArity.None, Text);

    /// <summary>A custom operation: a name shorter than 12 characters.</summary>
    public class ShortNameOperation : IOperation
    {
        public string Name => "IsShortName";

        public OperationDefaults Defaults => new();

        public Expression Build<TPropertyType>(
            Expression member,
            IFilterCollection<TPropertyType?> values,
            IFilterStatementOptions? options
        ) =>
            Expression.LessThan(
                Expression.Property(member, nameof(string.Length)),
                Expression.Constant(12)
            );

        public void Validate(IFilterStatement statement) { }
    }

    public class ShoutsOperation : IOperation
    {
        public string Name => "Shouts";

        public OperationDefaults Defaults => new();

        public Expression Build<TPropertyType>(
            Expression member,
            IFilterCollection<TPropertyType?> values,
            IFilterStatementOptions? options
        ) => Expression.Constant(true);

        public void Validate(IFilterStatement statement) { }
    }

    /// <summary>
    /// A custom manipulator that keeps the first letters of the text. Its one argument is how many.
    /// </summary>
    public struct LeftManipulator : IEntityManipulator
    {
        static readonly System.Reflection.MethodInfo Take =
            typeof(LeftManipulator).GetMethod(
                nameof(TakeFirst),
                [typeof(string), typeof(int)]
            )!;

        public readonly string Name => "Left";

        public readonly Type[] ExpectedTypes => [typeof(string)];

        public object?[] Arguments { get; set; }

        public LeftManipulator() => Arguments = [];

        public readonly Expression ManipulateMember(Expression member) =>
            Expression.Call(
                Take,
                member,
                Expression.Constant(CountOf(Arguments))
            );

        public readonly object? ManipulateValue<TPropertyType>(
            TPropertyType? value
        ) => TakeFirst(value?.ToString(), CountOf(Arguments));

        public readonly void Validate() => _ = CountOf(Arguments);

        public static string? TakeFirst(string? text, int count) =>
            text == null ? null : text[..Math.Min(count, text.Length)];

        static int CountOf(object?[] arguments) =>
            arguments is [string text]
            && int.TryParse(text, out var count)
            && count >= 0
                ? count
                : throw new ArgumentException(
                    "Give a whole number of letters."
                );
    }
}
