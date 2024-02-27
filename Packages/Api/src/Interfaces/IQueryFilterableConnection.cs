using System.Linq.Expressions;

namespace TopMarksDevelopment.ExpressionBuilder.Api;

public interface IQueryFilterableConnection
{
    IQueryFilterable And();

    IQueryFilterable Or();

    IQueryFilterableConnection CloseGroup();

    IQueryFilterableConnection CloseCollection();
}
