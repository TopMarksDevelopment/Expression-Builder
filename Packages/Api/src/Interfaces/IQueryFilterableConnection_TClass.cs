namespace TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// A connection for fluent calls on <see cref="IQueryFilterable"/>s'
/// </summary>
/// <typeparam name="TClass">The filters' class</typeparam>
public interface IQueryFilterableConnection<TClass> : IQueryFilterableConnection
    where TClass : class
{
    /// <summary>
    /// Apply <see cref="Connector.And"/> to the last item
    /// </summary>
    /// <returns>The same filter ready for the next statement</returns>
    new IQueryFilterable<TClass> And();

    /// <summary>
    /// Apply <see cref="Connector.Or"/> to the last item
    /// </summary>
    /// <returns>The same filter ready for the next statement</returns>
    new IQueryFilterable<TClass> Or();

    /// <summary>
    /// Closes the group of statements
    /// </summary>
    /// <returns>A connection for fluent calls</returns>
    /// <exception cref="InvalidOperationException">No group to close</exception>
    new IQueryFilterableConnection<TClass> CloseGroup();

    /// <summary>
    /// Use this at the end of your chain, thus gaining access to the <see cref="IQueryFilterable"/> again
    /// </summary>
    /// <returns>The <see cref="IQueryFilterable"/> without needing an <see cref="And"/> or <see cref="Or"/> call</returns>
    IQueryFilterable<TClass> End();

    /// <summary>
    /// Closes a subcollection to continue querying against the parent class
    /// (where the type is specified)
    /// </summary>
    /// <typeparam name="TParentClass">The type of the parent collection</typeparam>
    /// <remarks>This must be specified if you plan to do fluent calls</remarks>
    /// <returns>A connection for fluent calls</returns>
    /// <exception cref="InvalidOperationException">No group/collection to close</exception>
    IQueryFilterableConnection<TParentClass> CloseCollection<TParentClass>()
        where TParentClass : class;

    /// <summary>
    /// Closes a subcollection to continue querying against the parent class
    /// (where the type comes from a navigation property)
    /// </summary>
    /// <typeparam name="TParentClass">The type of the parent collection</typeparam>
    /// <param name="expression">The expression which points to the parent property (if navigation exists)</param>
    /// <remarks>This must be specified if you plan to do fluent calls</remarks>
    /// <returns>A connection for fluent calls</returns>
    /// <exception cref="InvalidOperationException">No group/collection to close</exception>

    IQueryFilterableConnection<TParentClass> CloseCollection<TParentClass>(
        Func<TClass, TParentClass> expression
    )
        where TParentClass : class;
}
