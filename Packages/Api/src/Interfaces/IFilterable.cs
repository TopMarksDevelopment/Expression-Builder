namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Collections;
using System.Linq.Expressions;

public interface IFilterable : IEnumerable
{
    IFilter Filter { get; }

    IFilterable Add(IFilterStatement statement);

    IFilterableConnection Add<TClass, TPropertyType>(
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    )
        where TClass : class;

    IFilterableConnection Add<TPropertyType>(
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    );

    IFilterable OpenGroup();

    IFilterableConnection CloseGroup();

    IFilterable OpenCollection(string propertyExpression);

    IFilterableConnection CloseCollection();

    Expression<Func<TClass, bool>> ToExpression<TClass>();
}
