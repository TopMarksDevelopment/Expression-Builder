namespace ExpressionBuilder.Tests;

internal class OperationTestData : TheoryData<TestBuilder<Product>>
{
    internal static DateTime? createdDate = new(2023, 10, 1, 10, 0, 10);
    internal static DateTime? otherDate = createdDate.Value.AddMinutes(10);
    static readonly int[] nonNullSource = [1, 3];

    internal static string ApplyReplacements(string input) =>
        input
            .Replace("new [] {", "[")
            .Replace("}.", "].")
            .Replace("OperationTestData.nonNullSource", "[1, 3]")
            .Replace(
                "[OperationTestData.createdDate",
                "[" + createdDate.ToString()
            )
            .Replace(
                "OperationTestData.otherDate]",
                otherDate.ToString() + "]"
            );

    public OperationTestData() =>
        AddRange(NullPropertyHasNullCheck, NonNullableHasNoNullCheck);

    static TestBuilder<Product> NullPropertyHasNullCheck =>
        new(
            "A null property should have null checks",
            x => new[] { createdDate, otherDate }.Contains(x.CreatedAt),
            x =>
                x.Equal<Product, DateTime?>(
                    x => x.CreatedAt,
                    [createdDate, otherDate]
                ),
            x => x.Equal(x => x.CreatedAt, [createdDate, otherDate]),
            x => x.Equal(x => x.CreatedAt, [createdDate, otherDate]),
            x => x.Equal(x => x.CreatedAt, [createdDate, otherDate])
        );

    static TestBuilder<Product> NonNullableHasNoNullCheck =>
        new(
            "No Null Check On Non-Nullables",
            x => (nonNullSource.Contains(x.Id)),
            x => x.In<Product, int>(x => x.Id, nonNullSource),
            x => x.In(x => x.Id, nonNullSource),
            x => x.In(x => x.Id, nonNullSource),
            x => x.In(x => x.Id, nonNullSource)
        );

    // This is no longer available - need to revisit (as per blow)
    // https://github.com/TopMarksDevelopment/Expression-Builder/discussions/14
    //
    // static TestBuilder<Product> SkipNullCheck =>
    //     new(
    //         "A null check can be skipped",
    //         x => new[] { createdDate, otherDate }.Contains(x.CreatedAt),
    //         x =>
    //             x.Equal<Product, DateTime?>(
    //                 x => x.CreatedAt,
    //                 [createdDate, otherDate],
    //                 new FilterStatementOptions { SkipNullChecks = true }
    //             ),
    //         x =>
    //             x.Equal(
    //                 x => x.CreatedAt,
    //                 [createdDate, otherDate],
    //                 new FilterStatementOptions { SkipNullChecks = true }
    //             ),
    //         x =>
    //             x.Equal(
    //                 x => x.CreatedAt,
    //                 [createdDate, otherDate],
    //                 new FilterStatementOptions { SkipNullChecks = true }
    //             ),
    //         x =>
    //             x.Equal(
    //                 x => x.CreatedAt,
    //                 [createdDate, otherDate],
    //                 new FilterStatementOptions { SkipNullChecks = true }
    //             )
    //     );
}
