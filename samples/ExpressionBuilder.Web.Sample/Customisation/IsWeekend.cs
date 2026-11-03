namespace ExpressionBuilder.Web.Sample.Customisation;

using System.Linq.Expressions;
using TopMarksDevelopment.ExpressionBuilder;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// An operation of the app's own: a date that falls on a Saturday or Sunday. It is an ordinary
/// <see cref="IOperation"/>; Program.cs says how the filter builder should offer it.
/// </summary>
public class IsWeekend : IOperation
{
    public string Name => "IsWeekend";

    // The property is checked for null before this runs, so it is only given dates
    public OperationDefaults Defaults =>
        new() { NullHandler = OperationNullHandler.NotNullAnd };

    public Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> values,
        IFilterStatementOptions? options
    )
    {
        var day = Expression.Property(member, nameof(DateTime.DayOfWeek));

        return Expression.OrElse(
            Expression.Equal(day, Expression.Constant(DayOfWeek.Saturday)),
            Expression.Equal(day, Expression.Constant(DayOfWeek.Sunday))
        );
    }

    public void Validate(IFilterStatement statement) { }
}
