namespace ExpressionBuilder.Tests.Collections;

using System.Collections;

/// <summary>
/// The values of a statement are exposed as a non-generic <see cref="IFilterCollection"/>, which is
/// what serialisers and the <see cref="IFilterStatement"/> interface work with.
/// </summary>
public class FilterCollectionTests
{
    [Fact(DisplayName = "Setting the values of a statement copies them")]
    public void SettingValuesCopiesThem()
    {
        var source = new FilterStatement<int>("Id", new Equal(), [1, 2, 3]);
        IFilterStatement target = new FilterStatement<int>(
            "Id",
            new Equal(),
            [9]
        );

        target.Values = source.Values;

        Assert.Equal<int[]>([1, 2, 3], target.Values.Cast<int>().ToArray());
    }

    [Fact(DisplayName = "Values can be added to a non-generic collection")]
    public void AddRange()
    {
        IFilterStatement statement = new FilterStatement<int>(
            "Id",
            new Equal(),
            [1]
        );

        statement.Values.AddRange(new object[] { 2, 3 });

        Assert.Equal<int[]>([1, 2, 3], statement.Values.Cast<int>().ToArray());
    }

    [Fact(DisplayName = "It can be copied to an array")]
    public void CopyTo()
    {
        ICollection values = new FilterStatement<int>(
            "Id",
            new Equal(),
            [4, 5]
        ).Values;
        var array = new int[3];

        values.CopyTo(array, 1);

        Assert.Equal<int[]>([0, 4, 5], array);
    }

    [Fact(DisplayName = "Copying to an array of the wrong type is rejected")]
    public void CopyToWrongType()
    {
        ICollection values = new FilterStatement<int>(
            "Id",
            new Equal(),
            [4]
        ).Values;

        Assert.Throws<ArgumentException>(() => values.CopyTo(new string[1], 0));
    }

    [Fact(
        DisplayName = "It has something to lock on, so it can be enumerated safely"
    )]
    public void SyncRoot()
    {
        ICollection values = new FilterStatement<int>(
            "Id",
            new Equal(),
            [4]
        ).Values;

        Assert.False(values.IsSynchronized);
        Assert.NotNull(values.SyncRoot);
    }

    [Fact(DisplayName = "Its string form is a bracketed list")]
    public void ToStringIsAList()
    {
        var statement = new FilterStatement<string>(
            "Name",
            new Equal(),
            ["a", null, "c"]
        );

        Assert.Equal("[a, null, c]", statement.Values.ToString());
    }
}
