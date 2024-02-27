namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Linq.Expressions;

public interface IQueryFilterable : IQueryable
{
    IFilter Filter { get; }

    IQueryFilterable Add(IFilterStatement statement);

    IQueryFilterableConnection Add<TClass, TPropertyType>(
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    ) where TClass : class;

    IQueryFilterableConnection Add<TPropertyType>(
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    );

    IQueryFilterable SetConnector(Connector connector);

    IQueryFilterable OpenGroup();

    IQueryFilterableConnection CloseGroup();

    IQueryFilterable OpenCollection(string propertyExpression);

    IQueryFilterableConnection CloseCollection();
}
