namespace ExpressionBuilder.Tests;

public class ValueToByteTestData : TheoryData<dynamic?>
{
    public ValueToByteTestData() =>
        AddRange(
            [
                "test value",
                -99,
                (short)-295,
                999999999999999999L,
                110U,
                (ushort)295,
                999999999999999999UL,
                (byte)255,
                true,
                new DateTime(2024, 12, 31, 23, 59, 59),
                new DateTimeOffset(
                    new DateTime(2024, 12, 31, 23, 59, 59),
                    TimeSpan.FromHours(-5)
                ),
                new DateOnly(2024, 12, 31),
                new TimeOnly(23, 59, 59),
                3.14f,
                -343.134,
                Guid.NewGuid(),
                -3.14159265358979323846m,
                new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                TestEnum.Value1,
                null,
            ]
        );

    public enum TestEnum
    {
        Value1,
    }
}
