namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.SmartSearchTest;

public class SplitTermTests
{
    [Fact(DisplayName = "Negative term check")]
    public void NegativeCheck()
    {
        var term = "-IgnoreTerm3";

        Assert.Equal<string[]>(["-IgnoreTerm3"], SmartSearch.SplitTerm(term));
    }

    [Fact(DisplayName = "Quote term check")]
    public void QuotedTextCheck()
    {
        var term = "Term1 \"Exact term\"";

        Assert.Equal<string[]>(
            ["Term1", "\"Exact term\""],
            SmartSearch.SplitTerm(term)
        );
    }

    [Fact(DisplayName = "Negative quote term check")]
    public void ExactNegativeCheck()
    {
        var term = "-\"Ignore exact term\"";

        Assert.Equal<string[]>(
            ["-\"Ignore exact term\""],
            SmartSearch.SplitTerm(term)
        );
    }

    [Fact(DisplayName = "Example in README works")]
    public void ExampleCheck()
    {
        var term =
            "Term1 Term2 -IgnoreTerm3 \"Exact term\" -\"Ignore exact term\"";

        Assert.Equal<string[]>(
            [
                "Term1",
                "Term2",
                "-IgnoreTerm3",
                "\"Exact term\"",
                "-\"Ignore exact term\"",
            ],
            SmartSearch.SplitTerm(term)
        );
    }
}
