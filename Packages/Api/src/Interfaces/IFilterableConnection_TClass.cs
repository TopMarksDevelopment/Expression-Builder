namespace TopMarksDevelopment.ExpressionBuilder.Api;

public interface IFilterableConnection<TClass> : IFilterableConnection
    where TClass : class
{
    new IFilterable<TClass> And();

    new IFilterable<TClass> Or();

    new IFilterableConnection<TClass> CloseGroup();
    
    IFilterableConnection<TParentClass> CloseCollection<TParentClass>() where TParentClass : class;
    IFilterableConnection<TParentClass> CloseCollection<TParentClass>(
        Func<TClass, TParentClass> expression
    ) where TParentClass : class;
}