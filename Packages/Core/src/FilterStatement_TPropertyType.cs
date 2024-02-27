namespace TopMarksDevelopment.ExpressionBuilder;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Serialization;
using ProtoBuf;
using ProtoBuf.Meta;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Collections;
using TopMarksDevelopment.ExpressionBuilder.Extensions;
using TopMarksDevelopment.ExpressionBuilder.Serialization;

[ProtoContract(UseProtoMembersOnly = true)]
public class FilterStatement<TPropertyType>
    : IFilterStatement<TPropertyType>,
        ISerializer
{
    public string typeRef => "S";

    [ProtoMember(2)]
    public string propertyType { get; set; }

    [ProtoMember(3)]
    public string PropertyId { get; set; }

    [ProtoMember(4)]
    string OperationName
    {
        get => Operation.Name;
        set
        {
            var i = TypeTracker.Operations;
            Operation = SerializerBase.FindOperationByName(value, ref i);
            TypeTracker.Operations = i;
        }
    }

    [JsonConverter(typeof(IOperationJsonConverter))]
    public IOperation Operation { get; set; }

    [JsonConverter(typeof(FilterCollectionJsonConverterFactory))]
    [ProtoMember(5)]
    public IFilterCollection<TPropertyType?> Values { get; set; } =
        new FilterCollection<TPropertyType?>();

    [ProtoMember(6)]
    List<ManipulatorInfo> ManipulatorInfo
    {
        get =>
            Manipulators
                .Select(x =>
                    x is ExpressionMethodManipulator eMM
                        ? new ManipulatorInfo(eMM)
                        : new ManipulatorInfo(x.Name, x.Arguments)
                )
                .ToList();
        set
        {
            List<IEntityManipulator> manipulators = [];

            var i = TypeTracker.ManipulatorTypes;
            foreach (var item in value)
                manipulators.Add(SerializerBase.FindManipulator(item, ref i));

            Manipulators = manipulators;
        }
    }

    [JsonConverter(typeof(IEntityManipulatorJsonConverter))]
    public IEnumerable<IEntityManipulator> Manipulators { get; set; } = [];

    [ProtoMember(1)]
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
            {
                var inlineMember = m;

                foreach (var manipulator in Manipulators)
                    inlineMember = manipulator.ManipulateExpression(m);

                return Operation.Build(inlineMember, Values, Manipulators);
            },
            Operation.SkipNullMemberChecks
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

        if (options?.Manipulators != null)
            Manipulators = options.Manipulators;

        if (options != null)
        {
            if (options.SkipNullChecks.HasValue)
                Operation.SkipNullMemberChecks = options.SkipNullChecks.Value;

            if (options.Match.HasValue)
                Operation.Match = options.Match.Value;
        }

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
        var outputString = $"{PropertyId} {Operation.Name}";

        foreach (var manip in Manipulators)
            outputString +=
                $".{manip.Name}({string.Join(",", manip.Arguments)})";

        if (Values.Count > 1)
            outputString += $" {Operation.Match:g}";

        if (Values.Any())
            outputString += $" [ {string.Join(", ", Values)} ]";

        return $"{outputString} {Connector:g} ";
    }

    public void PrepForSerialisation(
        MetaType ifilterBase,
        ref ICollection<Type> fTypes
    )
    {
        var thisType = GetType();

        if (!fTypes.Contains(thisType))
        {
            ifilterBase.AddSubType(50 + fTypes.Count, thisType);
            fTypes.Add(thisType);
        }
    }
}
