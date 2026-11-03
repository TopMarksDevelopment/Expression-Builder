namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Linq.Expressions;

/// <summary>
/// The requirement to be added to a filter or group
/// </summary>
public interface IFilterItem
{
    /// <summary>
    /// A unique reference to this specific type
    /// </summary>
    string typeRef { get; }

    /// <summary>
    /// How we are going to connect to the next statement
    /// </summary>
    Connector Connector { get; set; }

    /// <summary>
    /// Converts your item in to a LINQ expression
    /// </summary>
    /// <param name="param">The input parameter reference. So the x part of <c>x =></c> linq statments</param>
    /// <param name="parameterLog">A log of parameters used so far</param>
    /// <returns></returns>
    Expression Build(Expression param, Dictionary<string, uint> parameterLog);

    /// <summary>
    /// Convertion into a string form
    /// </summary>
    /// <returns>A string represenation</returns>
    string ToString();
}
