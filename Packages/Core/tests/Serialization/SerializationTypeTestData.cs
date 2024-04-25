namespace ExpressionBuilder.Tests;

public class SerializationTypeTestData
    : TheoryData<(int FieldNumber, Type Type)>
{
    public SerializationTypeTestData() =>
        AddRange(
            (50, typeof(FilterStatement<string>)),
            // (51, typeof(FilterStatement<string?>)),
            (52, typeof(FilterStatement<int>)),
            (53, typeof(FilterStatement<int?>)),
            (54, typeof(FilterStatement<short>)),
            (55, typeof(FilterStatement<short?>)),
            (56, typeof(FilterStatement<long>)),
            (57, typeof(FilterStatement<long?>)),
            (58, typeof(FilterStatement<uint>)),
            (59, typeof(FilterStatement<uint?>)),
            (60, typeof(FilterStatement<ushort>)),
            (61, typeof(FilterStatement<ushort?>)),
            (62, typeof(FilterStatement<ulong>)),
            (63, typeof(FilterStatement<ulong?>)),
            (64, typeof(FilterStatement<byte>)),
            (65, typeof(FilterStatement<byte?>)),
            (66, typeof(FilterStatement<bool>)),
            (67, typeof(FilterStatement<bool?>)),
            (68, typeof(FilterStatement<DateTime>)),
            (69, typeof(FilterStatement<DateTime?>)),
            (70, typeof(FilterStatement<DateTimeOffset>)),
            (71, typeof(FilterStatement<DateTimeOffset?>)),
            (72, typeof(FilterStatement<DateOnly>)),
            (73, typeof(FilterStatement<DateOnly?>)),
            (74, typeof(FilterStatement<TimeOnly>)),
            (75, typeof(FilterStatement<TimeOnly?>)),
            (76, typeof(FilterStatement<float>)),
            (77, typeof(FilterStatement<float?>)),
            (78, typeof(FilterStatement<double>)),
            (79, typeof(FilterStatement<double?>))
        );
}
