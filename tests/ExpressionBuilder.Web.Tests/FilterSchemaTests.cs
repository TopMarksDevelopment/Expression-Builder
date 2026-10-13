namespace ExpressionBuilder.Web.Tests;

using System.Text.Json;
using ExpressionBuilder.Web.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;
using TopMarksDevelopment.ExpressionBuilder.Web;

[Collection("Operation catalog")]
public class FilterSchemaTests
{
    static readonly FilterSchema Schema = FilterSchema.For<Person>();

    static TypeSchema Root => Schema.Types[Schema.Root];

    static FieldSchema Field(string path, TypeSchema? type = null) =>
        (type ?? Root).Fields.Single(f => f.Path == path);

    [Theory(DisplayName = "Properties are classified by the type they hold")]
    [InlineData("Name", FieldKind.String, "text")]
    [InlineData("Email", FieldKind.String, "text")]
    [InlineData("Id", FieldKind.Number, "number")]
    [InlineData("Age", FieldKind.Number, "number")]
    [InlineData("Salary", FieldKind.Number, "number")]
    [InlineData("Score", FieldKind.Number, "number")]
    [InlineData("Born", FieldKind.Date, "date")]
    [InlineData("LastSeen", FieldKind.DateTime, "datetime-local")]
    [InlineData("Joined", FieldKind.Date, "date")]
    [InlineData("StartsAt", FieldKind.Time, "time")]
    [InlineData("Duration", FieldKind.Time, "time")]
    [InlineData("Active", FieldKind.Boolean, "boolean")]
    [InlineData("Verified", FieldKind.Boolean, "boolean")]
    [InlineData("Favourite", FieldKind.Enum, "select")]
    [InlineData("Reference", FieldKind.Guid, "text")]
    [InlineData("Home", FieldKind.Object, "")]
    [InlineData("Employer", FieldKind.Object, "")]
    [InlineData("Contacts", FieldKind.Collection, "")]
    [InlineData("Tags", FieldKind.Collection, "")]
    public void ClassifiesProperties(string path, FieldKind kind, string input)
    {
        var field = Field(path);

        Assert.Equal(kind, field.Kind);
        Assert.Equal(input, field.Input);
    }

    [Theory(DisplayName = "Whether a value can be missing follows the type")]
    [InlineData("Name", true)]
    [InlineData("Email", true)]
    [InlineData("Id", false)]
    [InlineData("Age", true)]
    [InlineData("Active", false)]
    [InlineData("Verified", true)]
    [InlineData("LastSeen", true)]
    [InlineData("Joined", false)]
    [InlineData("Home", true)]
    public void KnowsWhatCanBeNull(string path, bool nullable) =>
        Assert.Equal(nullable, Field(path).Nullable);

    [Theory(DisplayName = "Numbers step by one, or by any amount")]
    [InlineData("Id", "1")]
    [InlineData("Age", "1")]
    [InlineData("Salary", "any")]
    [InlineData("Score", "any")]
    public void NumbersHaveASuitableStep(string path, string step) =>
        Assert.Equal(step, Field(path).Step);

    [Fact(DisplayName = "Only a DateTime marked as a date is a date")]
    public void DateTimesAreDatesWhenMarked()
    {
        Assert.Equal(FieldKind.Date, Field("Born").Kind);
        Assert.Equal(FieldKind.DateTime, Field("LastSeen").Kind);
    }

    [Fact(DisplayName = "Enums offer their members, using display names")]
    public void EnumsOfferMembers() =>
        Assert.Equal(
            [("Red", "Red"), ("Green", "Green"), ("Blue", "Deep blue")],
            Field("Favourite").Options!.Select(o => (o.Value, o.Label))
        );

    [Fact(
        DisplayName = "Properties of related objects are reached with a path"
    )]
    public void ReachesIntoRelatedObjects()
    {
        var postcode = Field("Home.Postcode");

        Assert.Equal(FieldKind.String, postcode.Kind);
        Assert.Equal("Home address › Postcode", postcode.Label);
        Assert.Contains("Home.City", Root.Fields.Select(f => f.Path));
    }

    [Fact(
        DisplayName = "How far to reach is limited, so cycles can't run away"
    )]
    public void LimitsHowFarItReaches()
    {
        // Person > Home > Country > Head > ... would never end
        Assert.DoesNotContain(
            Root.Fields,
            f => f.Path.Count(c => c == '.') >= 2
        );

        var deeper = FilterSchema.For<Person>(
            new FilterBuilderOptions { MaxNavigationDepth = 3 }
        );
        var paths = deeper
            .Types[deeper.Root]
            .Fields.Select(f => f.Path)
            .ToList();

        Assert.Contains("Home.Country.Name", paths);
        Assert.DoesNotContain(paths, p => p.Count(c => c == '.') >= 3);
    }

    [Fact(
        DisplayName = "A depth of one only offers the class's own properties"
    )]
    public void DepthOfOneIsFlat()
    {
        var flat = FilterSchema.For<Person>(
            new FilterBuilderOptions { MaxNavigationDepth = 1 }
        );

        Assert.DoesNotContain(
            flat.Types[flat.Root].Fields,
            f => f.Path.Contains('.')
        );
    }

    [Fact(
        DisplayName = "A collection of objects leads to the schema of its items"
    )]
    public void CollectionsHaveAnItemType()
    {
        var contacts = Field("Contacts");
        var item = Schema.Types[contacts.ItemType!];

        Assert.Empty(contacts.Operations);
        Assert.Equal(
            ["Type", "Value", "Comments", "Owner"],
            item.Fields.Where(f => !f.Path.Contains('.')).Select(f => f.Path)
        );
    }

    [Fact(
        DisplayName = "A collection of plain values is filtered by its items"
    )]
    public void ScalarCollectionsHaveASingleField()
    {
        var tags = Field("Tags");
        var item = Schema.Types[tags.ItemType!];

        var value = Assert.Single(item.Fields);

        Assert.Equal("", value.Path);
        Assert.Equal(FieldKind.String, value.Kind);
        Assert.Contains("Contains", value.Operations);
    }

    [Fact(
        DisplayName = "A class reached through collections is described once"
    )]
    public void SharedTypesAreDescribedOnce()
    {
        // Person > Contacts > Owner > Person ... is the same Person
        var contactType = Schema.Types[Field("Contacts").ItemType!];

        Assert.Equal(
            Schema.Root,
            Field("Owner", contactType).Kind == FieldKind.Object
                ? Schema.Root
                : "unexpected"
        );
        Assert.Equal(
            Schema.Types.Count,
            Schema.Types.Values.Select(t => t.Id).Distinct().Count()
        );
    }

    [Fact(DisplayName = "Properties that are ignored are not offered")]
    public void IgnoresMarkedProperties() =>
        Assert.DoesNotContain(Root.Fields, f => f.Path == "Secret");

    [Fact(DisplayName = "Properties can be excluded in the options too")]
    public void ExcludesPropertiesFromOptions()
    {
        var schema = FilterSchema.For<Person>(
            new FilterBuilderOptions().Exclude<Person>(
                p => p.Salary,
                p => p.Email
            )
        );
        var paths = schema
            .Types[schema.Root]
            .Fields.Select(f => f.Path)
            .ToList();

        Assert.DoesNotContain("Salary", paths);
        Assert.DoesNotContain("Email", paths);
        Assert.Contains("Name", paths);
    }

    [Fact(DisplayName = "An exclusion applies wherever the class appears")]
    public void ExclusionsApplyEverywhere()
    {
        var schema = FilterSchema.For<Person>(
            new FilterBuilderOptions().Exclude<Contact>(c => c.Comments)
        );
        var contact = schema.Types[
            schema
                .Types[schema.Root]
                .Fields.Single(f => f.Path == "Contacts")
                .ItemType!
        ];

        Assert.DoesNotContain(contact.Fields, f => f.Path == "Comments");
    }

    [Fact(
        DisplayName = "Excluding something that is not a property is rejected"
    )]
    public void ExcludeNeedsAProperty() =>
        Assert.Throws<ArgumentException>(() =>
            new FilterBuilderOptions().Exclude<Person>(p => p.Name.Length)
        );

    [Theory(DisplayName = "Things that can't be filtered are left out")]
    [InlineData("Photo")]
    [InlineData("Extras")]
    [InlineData("ReadOnly")]
    [InlineData("Item")]
    public void LeavesOutUnsupportedMembers(string name)
    {
        // ReadOnly only has a getter, so it can still be filtered on. The others can't.
        var offered = Root.Fields.Any(f => f.Path == name);

        Assert.Equal(name == "ReadOnly", offered);
    }

    [Fact(DisplayName = "Labels are made readable from the property name")]
    public void LabelsAreReadable()
    {
        Assert.Equal("Name", Field("Name").Label);
        Assert.Equal(
            "Nickname with underscores",
            Field("Nickname_With_Underscores").Label
        );
        Assert.Equal("Home address", Field("Home").Label);
    }

    [Theory(DisplayName = "Names are turned into words")]
    [InlineData("OtherSearchField", "Other search field")]
    [InlineData("StockLocations", "Stock locations")]
    [InlineData("Id", "Id")]
    [InlineData("URL", "URL")]
    [InlineData("HTTPStatus", "HTTP status")]
    [InlineData("Postcode", "Postcode")]
    [InlineData("X", "X")]
    [InlineData("", "")]
    public void Humanises(string name, string expected) =>
        Assert.Equal(expected, SchemaBuilder.Humanize(name));

    [Fact(DisplayName = "Operations suit the kind of property")]
    public void OperationsSuitTheKind()
    {
        Assert.Contains("Contains", Field("Name").Operations);
        Assert.DoesNotContain("GreaterThan", Field("Name").Operations);

        Assert.Contains("GreaterThan", Field("Salary").Operations);
        Assert.DoesNotContain("Contains", Field("Salary").Operations);

        Assert.Equal(["Equal", "NotEqual"], Field("Active").Operations);

        Assert.Equal(["IsNull", "IsNotNull"], Field("Employer").Operations);
    }

    [Fact(
        DisplayName = "Null checks are only offered where a value can be missing"
    )]
    public void NullChecksNeedNullableProperties()
    {
        Assert.Contains("IsNull", Field("Age").Operations);
        Assert.DoesNotContain("IsNull", Field("Id").Operations);
        Assert.DoesNotContain("IsNotNull", Field("Joined").Operations);
        Assert.Contains("IsNull", Field("LastSeen").Operations);
    }

    [Fact(DisplayName = "Only operations that are installed are offered")]
    public void OnlyInstalledOperations()
    {
        var names = Schema.Operations.Select(o => o.Name).ToList();

        Assert.Contains("Equal", names);
        Assert.All(
            names,
            name => Assert.True(OperationRegistry.TryGet(name, out _), name)
        );
    }

    [Fact(DisplayName = "Operation labels can be replaced")]
    public void OperationLabelsCanBeReplaced()
    {
        var options = new FilterBuilderOptions();
        options.OperationLabels["Equal"] = "Is exactly";

        var schema = FilterSchema.For<Person>(options);

        Assert.Equal(
            "Is exactly",
            schema.Operations.Single(o => o.Name == "Equal").Label
        );
    }

    [Fact(
        DisplayName = "Only some text operations let the user choose any or all terms"
    )]
    public void MatchIsOnlyOfferedForTextOperations()
    {
        var supports = Schema
            .Operations.Where(o => o.SupportsMatch)
            .Select(o => o.Name);

        Assert.Equal(
            [
                "Contains",
                "DoesNotContain",
                "DoesNotStartWith",
                "DoesNotEndWith",
            ],
            supports
        );
    }

    [Fact(
        DisplayName = "It is sent to the script as camelCase JSON, with kinds as words"
    )]
    public void SerialisesForTheScript()
    {
        using var document = JsonDocument.Parse(Schema.ToJson());
        var root = document.RootElement;

        Assert.Equal(Schema.Root, root.GetProperty("root").GetString());

        var name = root.GetProperty("types")
            .GetProperty(Schema.Root)
            .GetProperty("fields")
            .EnumerateArray()
            .Single(f => f.GetProperty("path").GetString() == "Name");

        Assert.Equal("string", name.GetProperty("kind").GetString());
        Assert.True(name.GetProperty("nullable").GetBoolean());
        Assert.Equal("text", name.GetProperty("input").GetString());
        Assert.False(name.TryGetProperty("step", out _));
        Assert.Equal(
            "Add condition",
            root.GetProperty("labels").GetProperty("addCondition").GetString()
        );
        Assert.Equal(
            "many",
            root.GetProperty("operations")
                .EnumerateArray()
                .Single(o => o.GetProperty("name").GetString() == "In")
                .GetProperty("arity")
                .GetString()
        );
    }

    [Fact(
        DisplayName = "The CLR type behind a field is not sent to the browser"
    )]
    public void ClrTypesStayOnTheServer() =>
        Assert.DoesNotContain(
            "valueType",
            Schema.ToJson(),
            StringComparison.OrdinalIgnoreCase
        );

    [Fact(DisplayName = "A null type is rejected")]
    public void NullTypeIsRejected() =>
        Assert.Throws<ArgumentNullException>(() => FilterSchema.For(null!));

    [Fact(
        DisplayName = "Custom operations are offered once they are registered and described"
    )]
    public void CustomOperations()
    {
        OperationRegistry.Register<ShoutsOperation>();

        Assert.DoesNotContain(
            FilterSchema.For<Person>().Operations,
            o => o.Name == "Shouts"
        );

        OperationCatalog.Register(
            new OperationDescriptor(
                "Shouts",
                "Is shouted",
                ValueArity.None,
                new HashSet<FieldKind> { FieldKind.String }
            )
        );

        try
        {
            var schema = FilterSchema.For<Person>();

            Assert.Contains(schema.Operations, o => o.Name == "Shouts");
            Assert.Contains(
                "Shouts",
                schema
                    .Types[schema.Root]
                    .Fields.Single(f => f.Path == "Name")
                    .Operations
            );
        }
        finally
        {
            // Other tests read the catalog, so leave it as it was found
            OperationCatalog.Unregister("Shouts");
        }

        Assert.DoesNotContain(
            FilterSchema.For<Person>().Operations,
            o => o.Name == "Shouts"
        );
    }

    public class ShoutsOperation
        : TopMarksDevelopment.ExpressionBuilder.Api.IOperation
    {
        public string Name => "Shouts";

        public TopMarksDevelopment.ExpressionBuilder.Api.OperationDefaults Defaults =>
            new();

        public System.Linq.Expressions.Expression Build<TPropertyType>(
            System.Linq.Expressions.Expression member,
            TopMarksDevelopment.ExpressionBuilder.Api.IFilterCollection<TPropertyType?> values,
            TopMarksDevelopment.ExpressionBuilder.Api.IFilterStatementOptions? options
        ) => System.Linq.Expressions.Expression.Constant(true);

        public void Validate(
            TopMarksDevelopment.ExpressionBuilder.Api.IFilterStatement statement
        ) { }
    }
}
