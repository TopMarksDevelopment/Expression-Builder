namespace ExpressionBuilder.Tests;

[Collection("Ensure old fixes are not undone")]
public class FixTests
{
    [Fact(DisplayName = "Fixed: Equal shouldn't null check the last item")]
    public void NotEqualShouldntNullCheck()
    {
        Expression<Func<Product, bool>> actual = x =>
            x.OtherSearchField != "word";

        var filter = new Filter<Product>();

        filter.NotEqual(x => x.OtherSearchField, "word");

        Assert.Equal(filter.ToMatchString(), actual.ToMatchString());
    }
}
