namespace TopMarksDevelopment.ExpressionBuilder;

using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Serialization;
using ProtoBuf;
using ProtoBuf.Meta;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Extensions;
using TopMarksDevelopment.ExpressionBuilder.Serialization;

/// <summary>
/// A filter for <typeparamref name="TClass"/>, built from statements (and groups of them)
/// and converted to a LINQ expression to use with <c>Where()</c>.
/// </summary>
public class Filter<TClass> : IFilter<TClass>, IFilter
    where TClass : class
{
    readonly IFilterGroup _group;
    readonly IFilter? _oldFilter;
    IFilterGroup _current;

    /// <inheritdoc />
    [JsonConverter(typeof(FilterItemCollectionJsonConverter))]
    public ICollection<IFilterItem> Items
    {
        get => _group.Items;
        set { _group.Items = value; }
    }

    /// <inheritdoc />
    [JsonIgnore]
    public ICollection<IFilterItem> Current
    {
        get => _current.Items;
        set { _current.Items = value; }
    }

    void PrepareForSerialization()
    {
        if (!RuntimeTypeModel.Default.CanSerialize(typeof(IProtoFilterItem)))
        {
            RuntimeTypeModel.Default.Add(typeof(Connector), true);
            RuntimeTypeModel.Default.Add(typeof(Matches), true);

            Serializer.PrepareSerializer<FilterGroup>();
        }
    }

    /// <summary>
    /// Saves the filter in its compact binary (protobuf) form
    /// </summary>
    /// <param name="stream">The stream to write to</param>
    public void SerializeTo(Stream stream)
    {
        PrepareForSerialization();

        Serializer.Serialize(stream, _group as FilterGroup);
    }

    /// <summary>
    /// Saves the filter in its compact binary (protobuf) form
    /// </summary>
    /// <param name="bytes">The saved filter</param>
    public void SerializeTo(out byte[] bytes)
    {
        using var memStream = new MemoryStream();
        SerializeTo(memStream);
        bytes = memStream.ToArray();
    }

    /// <summary>
    /// Loads a filter that was saved with <c>SerializeTo</c>
    /// </summary>
    /// <param name="stream">The stream to read from</param>
    /// <returns>The saved filter</returns>
    public static Filter<TClass> DeserializeFrom(Stream stream)
    {
        var group = Serializer.Deserialize<FilterGroup>(stream);

        return new Filter<TClass>(group, group, null);
    }

    /// <summary>
    /// Loads a filter that was saved with <c>SerializeTo</c>
    /// </summary>
    /// <param name="bytes">The saved filter</param>
    /// <returns>The saved filter</returns>
    public static Filter<TClass> DeserializeFrom(byte[] bytes)
    {
        using var memStream = new MemoryStream(bytes);
        var group = Serializer.Deserialize<FilterGroup>(memStream);

        return new Filter<TClass>(group, group, null);
    }

    /// <summary>
    /// Creates an empty filter
    /// </summary>
    public Filter()
    {
        _group = new FilterGroup();
        _current = _group;
    }

    Filter(IFilterGroup group, IFilterGroup current, IFilter? oldFilter)
    {
        _group = group;
        _current = current;
        _oldFilter = oldFilter;
    }

    #region Add methods

    /// <inheritdoc />
    public IFilter<TClass> Add<TPropertyType>(
        IFilterStatement<TPropertyType> statement
    )
    {
        _current.Items.Add(statement);

        return this;
    }

    /// <inheritdoc />
    public IFilterConnection<TClass> Add<TPropertyType>(
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    )
    {
        Add(
            FilterStatement<TPropertyType>.Create(
                propertyExpression,
                operation,
                values,
                options,
                Connector.And
            )
        );

        return new FilterConnection<TClass>(this, _current.Items.Last());
    }

    /// <inheritdoc />
    public IFilterConnection<TClass> Add<TPropertyType>(
        string propertyExpression,
        IOperation operation,
        TPropertyType?[] values,
        IFilterStatementOptions? options = null
    )
    {
        Add(
            new FilterStatement<TPropertyType>(
                propertyExpression,
                operation,
                values,
                options,
                Connector.And
            )
        );

        return new FilterConnection<TClass>(this, _current.Items.Last());
    }

    #endregion Add methods

    /// <inheritdoc />
    public void SetLastConnector(Connector connector)
    {
        _current.Items.Last().Connector = connector;
    }

    /// <inheritdoc />
    public IFilter<TClass> OpenGroup()
    {
        var newGroup = new FilterGroup(_current);

        _current.Items.Add(newGroup);
        _current = newGroup;

        return this;
    }

    /// <inheritdoc />
    public IFilterConnection<TClass> CloseGroup()
    {
        var connection = new FilterConnection<TClass>(this, _current);

        try
        {
            if (_oldFilter?.Current == Current)
                _oldFilter.CloseGroup();
        }
        catch { }

        _current = _current.Close();

        return connection;
    }

    /// <inheritdoc />
    public virtual IFilter<T> OpenCollection<T>(
        Expression<Func<TClass, IEnumerable<T>>> propertyExpression
    )
        where T : class
    {
        var splitExpr = propertyExpression.ToString().Split(" => ");

        if (splitExpr.Length != 2)
            throw new ArgumentException(
                "The expression is not a valid property expression",
                nameof(propertyExpression)
            );

        var prefix = splitExpr.Last()[(splitExpr.First().Length + 1)..] + "[]";

        var newGroup = new FilterGroup(_current, prefix);

        _current.Items.Add(newGroup);
        _current = newGroup;

        return new Filter<T>(_group, _current, this);
    }

    /// <inheritdoc />
    public virtual IFilterConnection<T> CloseCollection<T>()
        where T : class
    {
        CloseGroup();

        return new FilterConnection<T>(
            new Filter<T>(_group, _current, _oldFilter),
            _current
        );
    }

    /// <inheritdoc />
    public virtual IFilterConnection<T> CloseCollection<T>(
        Func<TClass, T> expression
    )
        where T : class => CloseCollection<T>();

    /// <remarks>
    /// The values the filter compares with are read from variables rather than written in as constants, so a database
    /// provider sends them as parameters: one query plan serves every value. Use
    /// <see cref="FilterExpressionExtensions.InlineValues{TClass}"/> to get them as constants.
    /// </remarks>
    /// <inheritdoc />
    public Expression<Func<TClass, bool>> ToExpression() =>
        ValueParameters.Parameterise(BuildExpression());

    /// <summary>
    /// Makes a filter into a delegate, for objects in memory.
    /// </summary>
    /// <remarks>
    /// Compiled from constants, not variables: nothing here sends the values to a database, and the compiler is
    /// noticeably slower with the variables (see tools/ExpressionBuilder.Benchmarks).
    /// </remarks>
    internal static Func<TClass, bool> Compile(IFilter<TClass> filter) =>
        filter is Filter<TClass> own
            ? own.BuildExpression().Compile()
            : filter.ToExpression().InlineValues().Compile();

    Expression<Func<TClass, bool>> BuildExpression()
    {
        var parameterLog = new Dictionary<string, uint>() { };

        var param = Expression.Parameter(
            typeof(TClass),
            parameterLog.GetMeaningfulUnique("x")
        );

        var delegateType = typeof(Func<,>).MakeGenericType(
            typeof(TClass),
            typeof(bool)
        );

        return (Expression<Func<TClass, bool>>)
            Expression.Lambda(
                delegateType,
                param.GetExpression(
                    null,
                    parameterLog,
                    (o, l) => _group.Build(o, parameterLog),
                    OperationNullHandler.Skip
                ),
                param
            );
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var sB = new StringBuilder();

        foreach (var item in _group.Items)
            sB.Append(item.ToString());

        var sBString = sB.ToString();

        if (sBString.Length > 4)
            sBString = sBString[..^4].TrimEnd();

        return sBString;
    }

    IFilter IFilter.Add(IFilterStatement statement)
    {
        _current.Items.Add(statement);

        return this;
    }

#nullable disable

    // TODO: Adding nullability causes an error, removing
    // `?`'s removes error but causes nullability warning,
    // so added disable statement
    IFilterConnection IFilter.Add<TClass1, TPropertyType>(
        Expression<Func<TClass1, TPropertyType>> propertyExpression,
        IOperation operation,
        TPropertyType[] values,
        IFilterStatementOptions options
    )
        where TClass1 : class
    {
        Add(
            FilterStatement<TPropertyType>.Create(
                propertyExpression,
                operation,
                values,
                options
            )
        );

        return new FilterConnection<TClass>(this, _current.Items.Last());
    }

    IFilterConnection IFilter.Add<TPropertyType>(
        string propertyExpression,
        IOperation operation,
        TPropertyType[] values,
        IFilterStatementOptions options
    ) => Add(propertyExpression, operation, values, options);

#nullable restore

    IFilter IFilter.OpenGroup() => OpenGroup();

    IFilterConnection IFilter.CloseGroup() => CloseGroup();

    /// <inheritdoc />
    public IFilter OpenCollection(string propertyExpression)
    {
        var x = OpenGroup();

        _current.ParentPropertyExpression = propertyExpression;

        if (!_current.ParentPropertyExpression.EndsWith("[]"))
            _current.ParentPropertyExpression += "[]";

        return x;
    }

    /// <inheritdoc />
    public IFilterConnection CloseCollection() => CloseGroup();

    Expression<Func<T, bool>> IFilter.ToExpression<T>() =>
        (ToExpression() as Expression<Func<T, bool>>)!;

    #region Operators

    /// <summary>
    /// Implicitly converts a <see cref="Filter{TClass}" /> into an <c>Expression&lt;Func&lt;TClass, bool&gt;&gt;</c>.
    /// </summary>
    /// <param name="filter"></param>
    public static implicit operator Expression<Func<TClass, bool>>(
        Filter<TClass> filter
    ) => filter.ToExpression();

    /// <summary>
    /// Implicitly converts a <see cref="Filter{TClass}" /> into a <see cref="Func{TClass, TResult}" />.
    /// </summary>
    /// <param name="filter"></param>
    public static implicit operator Func<TClass, bool>(Filter<TClass> filter) =>
        Compile(filter);

    #endregion Operators
}
