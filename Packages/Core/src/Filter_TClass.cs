namespace TopMarksDevelopment.ExpressionBuilder;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Serialization;
using ProtoBuf;
using ProtoBuf.Meta;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Extensions;
using TopMarksDevelopment.ExpressionBuilder.Serialization;

public class Filter<TClass> : IFilter<TClass>, IFilter
    where TClass : class
{
    readonly IFilterGroup _group;
    readonly IFilter? _oldFilter;
    IFilterGroup _current;

    [JsonConverter(typeof(FilterItemCollectionJsonConverter))]
    public ICollection<IFilterItem> Items
    {
        get => _group.Items;
        set { _group.Items = value; }
    }

    [JsonIgnore]
    public ICollection<IFilterItem> Current
    {
        get => _current.Items;
        set { _current.Items = value; }
    }

    void PrepareForSerialization()
    {
        if (!RuntimeTypeModel.Default.CanSerialize(typeof(IFilterItem)))
        {
            RuntimeTypeModel.Default.Add(typeof(Connector), true);
            RuntimeTypeModel.Default.Add(typeof(Matches), true);

            RuntimeTypeModel
                .Default.Add(typeof(IFilterGroup), false)
                .AddSubType(45, typeof(FilterGroup));

            RuntimeTypeModel
                .Default.Add(typeof(IFilterStatementOptions), false)
                .AddSubType(45, typeof(FilterStatementOptions));

            RuntimeTypeModel
                .Default.Add(typeof(IFilterItem), false)
                .AddSubType(45, typeof(IFilterStatement))
                .AddSubType(46, typeof(IFilterGroup));

            Serializer.PrepareSerializer<Filter<TClass>>();
        }

        var pTypes = TypeTracker.FilterStatementTypes;
        var filterMeta = RuntimeTypeModel.Default.Add(
            typeof(IFilterStatement),
            false
        );
        var filterSubTypes = filterMeta
            .GetSubtypes()
            .Select(x => x.DerivedType.Type);
        var baseType = typeof(FilterStatement<>);

        var indexTracker = 0;
        foreach (var pType in pTypes)
        {
            if (baseType != pType.GetGenericTypeDefinition())
                throw new TypeLoadException(
                    "Must be of type FilterStatement<>"
                );

            if (!filterSubTypes.Contains(pType))
                filterMeta.AddSubType(50 + indexTracker, pType);

            indexTracker++;
        }

        (_group as FilterGroup)!.PrepForSerialisation(filterMeta, ref pTypes);
    }

    public void SerializeTo(Stream stream)
    {
        PrepareForSerialization();

        Serializer.Serialize(stream, _group as FilterGroup);
    }

    public void SerializeTo(out byte[] bytes)
    {
        using var memStream = new MemoryStream();
        SerializeTo(memStream);
        bytes = memStream.ToArray();
    }

    public static Filter<TClass> DeserializeFrom(Stream stream)
    {
        var group = Serializer.Deserialize<IFilterGroup>(stream);

        return new Filter<TClass>(group, group, null);
    }

    public static Filter<TClass> DeserializeFrom(byte[] bytes)
    {
        using var memStream = new MemoryStream(bytes);
        var group = Serializer.Deserialize<IFilterGroup>(memStream);

        return new Filter<TClass>(group, group, null);
    }

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

    public IFilter<TClass> Add<TPropertyType>(
        IFilterStatement<TPropertyType> statement
    )
    {
        _current.Items.Add(statement);

        return this;
    }

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

    public void SetLastConnector(Connector connector)
    {
        _current.Items.Last().Connector = connector;
    }

    public IFilter<TClass> OpenGroup()
    {
        var newGroup = new FilterGroup(_current);

        _current.Items.Add(newGroup);
        _current = newGroup;

        return this;
    }

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

    public virtual IFilterConnection<T> CloseCollection<T>()
        where T : class
    {
        CloseGroup();

        return new FilterConnection<T>(
            new Filter<T>(_group, _current, _oldFilter),
            _current
        );
    }

    public virtual IFilterConnection<T> CloseCollection<T>(
        Func<TClass, T> expression
    )
        where T : class => CloseCollection<T>();

    public Expression<Func<TClass, bool>> ToExpression()
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

    public IFilter OpenCollection(string propertyExpression)
    {
        var x = OpenGroup();

        _current.ParentPropertyExpression = propertyExpression;

        if (!_current.ParentPropertyExpression.EndsWith("[]"))
            _current.ParentPropertyExpression += "[]";

        return x;
    }

    public IFilterConnection CloseCollection() => CloseGroup();

    Expression<Func<T, bool>> IFilter.ToExpression<T>() =>
        (ToExpression() as Expression<Func<T, bool>>)!;

    #region Operators

    /// <summary>
    /// Implicitly converts a <see cref="Filter{TClass}" /> into a <see cref="System.Linq.Expressions.Expression{Func{TClass, TResult}}" />.
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
        filter.ToExpression().Compile();

    #endregion Operators
}
