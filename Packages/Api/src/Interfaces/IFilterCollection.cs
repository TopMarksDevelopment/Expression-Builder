namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System.Collections;

public interface IFilterCollection : ICollection
{    
    abstract bool Any();
    
    abstract string ToString();
}