namespace ExpressionBuilder.Tests;

public class AllTests : TheoryData
{
    internal static string ApplyReplacements(string input) =>
        input
            .Replace("AllTests._values.", "[1, 2].")
            .Replace("AllTests._manipValues.", "[1, 4].")
            // This is [1, 4] because the [1, 2] is manipulated by `.Replace("2", "4")`
            .Replace("AllTests._inputManipValues.", "[1, 4].");

    public AllTests() =>
        AddRows(
            [
                [MultiMatcher],
                [ManipulationCheck],
                [ManipulatedTypeCheck]
            ]
        );

    static TestBuilder<Product> MultiMatcher =>
        new(
            "Matches multiple values",
            x => _values.Contains(x.Id),
            x => x.In("Id", _values),
            x => x.In(x => x.Id, _values),
            x => x.In(x => x.Id, _values),
            x => x.In(x => x.Id, _values)
        );

    static TestBuilder<Product> ManipulationCheck =>
        new(
            "Normal manipulation works",
            x => x.Name != null && _inputManipValues.Contains(x.Name.Replace("2", "4")),
            x => x.In("Name", _manipValues, _manipsOptions),
            x => x.In(x => x.Name, _manipValues, _manipsOptions),
            x => x.In(x => x.Name, _manipValues, _manipsOptions),
            x => x.In(x => x.Name, _manipValues, _manipsOptions)
        );

    static TestBuilder<Product> ManipulatedTypeCheck =>
        new(
            "Manipulation of type works",
            x => _manipValues.Contains(x.Id.ToString().Replace("2", "4")),
            x => x.In("Id", _values, _manipTypeOptions),
            x => x.In(x => x.Id, _values, _manipTypeOptions),
            x => x.In(x => x.Id, _values, _manipTypeOptions),
            x => x.In(x => x.Id, _values, _manipTypeOptions)
        );

    static readonly int[] _values = [1, 2];
    static readonly string[] _inputManipValues = ["1", "2"];
    static readonly string[] _manipValues = ["1", "4"];

    static readonly FilterStatementOptions _manipsOptions =
        new()
        {
            Manipulators = ExpressionMethodManipulator.GetAll<string>(x =>
                x.Replace("2", "4")
            )
        };

    static readonly FilterStatementOptions _manipTypeOptions =
        new()
        {
            Manipulators = ExpressionMethodManipulator.GetAll<int>(x =>
                x.ToString().Replace("2", "4")
            )
        };
}
