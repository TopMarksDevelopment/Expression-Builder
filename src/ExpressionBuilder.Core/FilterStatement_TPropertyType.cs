namespace TopMarksDevelopment.ExpressionBuilder;

using System.Linq.Expressions;
using System.Text.Json.Serialization;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Collections;
using TopMarksDevelopment.ExpressionBuilder.Extensions;
using TopMarksDevelopment.ExpressionBuilder.Serialization;

public class FilterStatement<TPropertyType>
    : IFilterStatement<TPropertyType>,
        IProtoFilterItem
{
    public string typeRef => "S";

    public string propertyType { get; set; }

    public string PropertyId { get; set; }

    [JsonConverter(typeof(IOperationJsonConverter))]
    public IOperation Operation { get; set; }

    [JsonConverter(typeof(FilterCollectionJsonConverterFactory))]
    public IFilterCollection<TPropertyType?> Values { get; set; } =
        new FilterCollection<TPropertyType?>();

    [JsonConverter(typeof(FilterStatementOptionsJsonConverter))]
    public IFilterStatementOptions? Options { get; set; }

    public Connector Connector { get; set; } = Connector.And;

    IFilterCollection IFilterStatement.Values
    {
        get => Values;
        set => Values = new FilterCollection<TPropertyType>(value);
    }

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

    public FilterStatement()
    {
        var t = typeof(TPropertyType);
        propertyType = t.FullName ?? t.AssemblyQualifiedName ?? t.Name;

        PropertyId = string.Empty;
        Operation = null!;
    }

    public FilterStatement(
        string propertyId,
        IOperation operation,
        IFilterCollection<TPropertyType?> values,
        IFilterStatementOptions? options = null,
        Connector connector = Connector.And
    )
    {
        var t = typeof(TPropertyType);
        propertyType = t.FullName ?? t.AssemblyQualifiedName ?? t.Name;

        PropertyId = propertyId;
        Operation = operation;
        Values = values;
        Connector = connector;
        Options = options;

        Operation.Validate(this);
    }

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
                        mcExp.Arguments.Select(x => (ConstantExpression)x)
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

    public IFilterItem Unpack() => this;
}
