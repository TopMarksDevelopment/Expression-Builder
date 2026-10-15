namespace TopMarksDevelopment.ExpressionBuilder;

using System.Linq.Expressions;
using System.Text.Json.Serialization;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Collections;
using TopMarksDevelopment.ExpressionBuilder.Extensions;
using TopMarksDevelopment.ExpressionBuilder.Serialization;

/// <summary>
/// A single condition: a property, the operation to perform on it and the value(s) to use.
/// </summary>
public class FilterStatement<TPropertyType>
    : IFilterStatement<TPropertyType>,
        IProtoFilterItem
{
    /// <inheritdoc />
    public string typeRef => "S";

    /// <summary>
    /// The full name of <typeparamref name="TPropertyType"/>, so a saved statement can be rebuilt
    /// </summary>
    public string propertyType { get; set; }

    /// <inheritdoc />
    public string PropertyId { get; set; }

    /// <inheritdoc />
    [JsonConverter(typeof(IOperationJsonConverter))]
    public IOperation Operation { get; set; }

    /// <inheritdoc />
    [JsonConverter(typeof(FilterCollectionJsonConverterFactory))]
    public IFilterCollection<TPropertyType?> Values { get; set; } =
        new FilterCollection<TPropertyType?>();

    /// <inheritdoc />
    [JsonConverter(typeof(FilterStatementOptionsJsonConverter))]
    public IFilterStatementOptions? Options { get; set; }

    /// <inheritdoc />
    public Connector Connector { get; set; } = Connector.And;

    IFilterCollection IFilterStatement.Values
    {
        get => Values;
        set => Values = new FilterCollection<TPropertyType>(value);
    }

    /// <inheritdoc />
    public Expression Build(
        Expression member,
        Dictionary<string, uint> parameterLog
    )
    {
        Operation.Validate(this);

        return member.GetExpression(
            PropertyId,
            parameterLog,
            (m, o) =>
                Operation.Build(Options.ApplyManipulators(m), Values, Options),
            Operation.Defaults.NullHandler
        );
    }

    #region Ctr.

    /// <summary>
    /// Creates an empty statement, for serialisers
    /// </summary>
    public FilterStatement()
    {
        var t = typeof(TPropertyType);
        propertyType = TypeNames.For(t);

        PropertyId = string.Empty;
        Operation = null!;
    }

    /// <summary>
    /// Creates a statement
    /// </summary>
    /// <param name="propertyId">The property, such as <c>Birth.Country</c></param>
    /// <param name="operation">The operation to perform</param>
    /// <param name="values">The values to use</param>
    /// <param name="options">Options for the statement</param>
    /// <param name="connector">How the statement connects to the next</param>
    public FilterStatement(
        string propertyId,
        IOperation operation,
        IFilterCollection<TPropertyType?> values,
        IFilterStatementOptions? options = null,
        Connector connector = Connector.And
    )
    {
        var t = typeof(TPropertyType);
        propertyType = TypeNames.For(t);

        PropertyId = propertyId;
        Operation = operation;
        Values = values;
        Connector = connector;
        Options = options;

        Operation.Validate(this);
    }

    /// <summary>
    /// Creates a statement
    /// </summary>
    /// <param name="propertyId">The property, such as <c>Birth.Country</c></param>
    /// <param name="operation">The operation to perform</param>
    /// <param name="values">The values to use</param>
    /// <param name="options">Options for the statement</param>
    /// <param name="connector">How the statement connects to the next</param>
    public FilterStatement(
        string propertyId,
        IOperation operation,
        TPropertyType?[]? values,
        IFilterStatementOptions? options = null,
        Connector connector = Connector.And
    )
        : this(
            propertyId,
            operation,
            new FilterCollection<TPropertyType?>(values ?? []),
            options,
            connector
        ) { }

    /// <summary>
    /// Creates a statement that has no values
    /// </summary>
    /// <param name="propertyId">The property, such as <c>Birth.Country</c></param>
    /// <param name="operation">The operation to perform</param>
    /// <param name="connector">How the statement connects to the next</param>
    public FilterStatement(
        string propertyId,
        IOperation operation,
        Connector connector
    )
        : this(
            propertyId,
            operation,
            new FilterCollection<TPropertyType?>(),
            null,
            connector
        ) { }

    #endregion Ctr.

    /// <summary>
    /// Creates a statement from a property expression (<c>x => x.Name</c>). A method call on the
    /// property (<c>x => x.Name.Trim()</c>) becomes a manipulator.
    /// </summary>
    /// <param name="propertyExpression">The property</param>
    /// <param name="operation">The operation to perform</param>
    /// <param name="values">The values to use</param>
    /// <param name="options">Options for the statement</param>
    /// <param name="connector">How the statement connects to the next</param>
    /// <returns>The statement</returns>
    public static IFilterStatement<TPropertyType> Create<TClass>(
        Expression<Func<TClass, TPropertyType?>> propertyExpression,
        IOperation operation,
        TPropertyType?[]? values = null,
        IFilterStatementOptions? options = null,
        Connector connector = Connector.And
    )
        where TClass : class
    {
        var propertyExpressionString = HandleExpression(
            propertyExpression.Body
        );

        if (propertyExpressionString == null)
        {
            var tsplitExpr = propertyExpression.ToString().Split(" => ");

            if (tsplitExpr.Length != 2)
                throw new ArgumentException(
                    "The expression is not a valid property expression",
                    nameof(propertyExpression)
                );

            propertyExpressionString = tsplitExpr.Last()[
                (tsplitExpr.Last().IndexOf(tsplitExpr.First() + ".") + 2)..
            ];

            var index = propertyExpressionString.IndexOf(", ");
            if (index >= 0)
                propertyExpressionString = propertyExpressionString[..index];
        }

        return new FilterStatement<TPropertyType>(
            propertyExpressionString,
            operation,
            values ?? [],
            options,
            connector
        );

        string? HandleExpression(Expression expr)
        {
            if (
                expr is UnaryExpression uExp
                && uExp.Operand is MemberExpression mExp
            )
                return HandleMemberExpression(mExp);
            else if (expr is MemberExpression memExp)
                return HandleMemberExpression(memExp);
            else if (propertyExpression.Body is MethodCallExpression mcExp)
            {
                options ??= new FilterStatementOptions();
                options.Manipulators ??= [];

                options.Manipulators = options.Manipulators.Prepend(
                    new ExpressionMethodManipulator(
                        mcExp.Method,
                        mcExp.Arguments.Select(x => x.ToConstant()).ToArray()
                    )
                );

                ArgumentNullException.ThrowIfNull(mcExp.Object);

                return HandleExpression(mcExp.Object);
            }

            return null;
        }
    }

    static string HandleMemberExpression(MemberExpression mExp)
    {
        var expr = mExp;
        var retStr = "";

        while (expr != null)
        {
            retStr = expr.Member.Name + "." + retStr;

            expr = expr.Expression is MemberExpression subExpr ? subExpr : null;
        }

        return retStr[..^1];
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var outputString = PropertyId;

        if (Options?.Manipulators != null)
            foreach (var manip in Options.Manipulators)
                outputString +=
                    $".{manip.Name}({string.Join(",", manip.Arguments)})";

        outputString += " " + Operation.Name;

        if (Values.Count > 1 || Options?.Match != null)
            outputString += $" {Options?.Match ?? Operation.Defaults.Match:g}";

        if (Values.Any())
            outputString += $" [ {string.Join(", ", Values)} ]";

        return $"{outputString} {Connector:g} ";
    }

    /// <inheritdoc />
    public IProtoFilterItem Pack() =>
        new ProtoFilterStatement()
        {
            Connector = Connector,
            propertyType = propertyType,
            PropertyId = PropertyId,
            Operation = Operation,
            ByteValues = Values.Select(ValueToByteConverters.Pack).ToList(),
            Options = Options as FilterStatementOptions,
        };

    /// <inheritdoc />
    public IFilterItem Unpack() => this;
}
