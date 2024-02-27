namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Collections.Generic;
using System.Linq.Expressions;

public interface IFilterItem
{
    string typeRef { get; }

    Connector Connector { get; set; }

    Expression Build(Expression param, Dictionary<string, uint> parameterLog);

    string ToString();
}
