using System.Linq.Expressions;

namespace TopMarksDevelopment.ExpressionBuilder.Api;

public interface IEntityManipulator
{
    string Name { get; }
    object?[] Arguments { get; set; }

    Expression ManipulateExpression(Expression member);
    
    void Validate();
}
