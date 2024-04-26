namespace TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// A connection for fluent calls on <see cref="IFilterable"/>s'
/// </summary>
/// <typeparam name="TClass">The filters' class</typeparam>
public interface IFilterableConnection<TClass> : IFilterableConnection
    where TClass : class
{
    /// <summary>
    /// Apply <see cref="Connector.And"/> to the last item
    /// </summary>
    /// <returns>The same filter ready for the next statement</returns>
    new IFilterable<TClass> And();

    /// <summary>
    /// Apply <see cref="Connector.Or"/> to the last item
    /// </summary>
    /// <returns>The same filter ready for the next statement</returns>
    new IFilterable<TClass> Or();

    /// <summary>
    /// Closes the group of statements
    /// </summary>
    /// <returns>A connection for fluent calls</returns>
    /// <exception cref="InvalidOperationException">No group to close</exception>
    new IFilterableConnection<TClass> CloseGroup();

    /// <summary>
    /// Closes a subcollection to continue querying against the parent class
    /// (where the type is specified)
    /// </summary>
    /// <typeparam name="TParentClass">The type of the parent collection</typeparam>
    /// <remarks>This must be specified if you plan to do fluent calls</remarks>
    /// <returns>A connection for fluent calls</returns>
    /// <exception cref="InvalidOperationException">No group/collection to close</exception>
    IFilterableConnection<TParentClass> CloseCollection<TParentClass>()
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

    IFilterableConnection<TParentClass> CloseCollection<TParentClass>(
        Func<TClass, TParentClass> expression
    )
        where TParentClass : class;
}
