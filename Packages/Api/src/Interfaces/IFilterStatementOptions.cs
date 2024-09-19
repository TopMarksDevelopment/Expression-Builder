namespace TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// The options you can pass to a <see cref="IFilterStatement"/> to adjust how it works
/// </summary>
public interface IFilterStatementOptions
{
    /// <summary>
    /// A set of methods we can use to manipulate the property and values
    /// </summary>
    IEnumerable<IEntityManipulator>? Manipulators { get; set; }

    /// <summary>
    /// <b>Overwrite the <see cref="IOperation"/> default</b><br/>
    /// If there are multiple values, are we matching
    /// <see cref="Matches.Any"/> or <see cref="Matches.All"/> of them
    /// </summary>
    Matches? Match { get; set; }
}
