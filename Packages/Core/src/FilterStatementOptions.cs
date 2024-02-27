using TopMarksDevelopment.ExpressionBuilder.Api;

namespace TopMarksDevelopment.ExpressionBuilder;

public class FilterStatementOptions : IFilterStatementOptions
{
    public IEnumerable<IEntityManipulator>? Manipulators { get; set; }

    public Matches? Match { get; set; }

    public bool? SkipNullChecks { get; set; }
}
