namespace ExpressionBuilder.Web.E2E;

/// <summary>One sample app and one set of browsers for every test.</summary>
[CollectionDefinition(Name)]
public class SampleAppCollection
    : ICollectionFixture<SampleApp>,
        ICollectionFixture<PublishedApps>
{
    public const string Name = "Sample app";

    /// <summary>The browsers a test runs in, as theory data.</summary>
    public static TheoryData<string> Browsers
    {
        get
        {
            var data = new TheoryData<string>();

            foreach (var engine in SampleApp.Engines)
                data.Add(engine);

            return data;
        }
    }
}
