namespace TopMarksDevelopment.ExpressionBuilder.Operations.Tests.SmartSearchTest;

public class OperationTests
{
    [Theory(DisplayName = "Anything other than 2 values throws")]
    [InlineData(TestType.Null)]
    [InlineData(TestType.EmptyArray)]
    public void CheckValidateWithNoValues(TestType testType) =>
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new SmartSearch().Validate(
                    new FilterStatement<string>(
                        "",
                        new SmartSearch(),
                        testType switch
                        {
                            TestType.Null => null,
                            TestType.EmptyArray or _ => []
                        }
                    )
                )
        );

    public enum TestType
    {
        Null,
        EmptyArray
    }
}
