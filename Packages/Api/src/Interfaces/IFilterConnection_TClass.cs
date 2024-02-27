namespace TopMarksDevelopment.ExpressionBuilder.Api;

using System;

public interface IFilterConnection<TClass> : IFilterConnection
    where TClass : class
{
    new IFilter<TClass> And();
    new IFilter<TClass> Or();
    new IFilterConnection<TClass> CloseGroup();
    IFilterConnection<TParentClass> CloseCollection<TParentClass>()
        where TParentClass : class;
    IFilterConnection<TParentClass> CloseCollection<TParentClass>(
        Func<TClass, TParentClass> expression
    )
        where TParentClass : class;
}
