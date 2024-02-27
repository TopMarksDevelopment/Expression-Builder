namespace TopMarksDevelopment.ExpressionBuilder.Api;

public interface IQueryFilterableConnection<TClass> : IQueryFilterableConnection
    where TClass : class
{
    new IQueryFilterable<TClass> And();

    new IQueryFilterable<TClass> Or();

    new IQueryFilterableConnection<TClass> CloseGroup();

    IQueryFilterable<TClass> End();

    IQueryFilterableConnection<T> CloseCollection<T>()
        where T : class;

    IQueryFilterableConnection<T> CloseCollection<T>(Func<TClass, T> expression)
        where T : class;
}
