namespace TopMarksDevelopment.ExpressionBuilder;

using System.Linq.Expressions;
using System.Text;
using System.Text.Json.Serialization;
using ProtoBuf;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Extensions;
using TopMarksDevelopment.ExpressionBuilder.Serialization;

[ProtoContract(UseProtoMembersOnly = true)]
public class FilterGroup : IFilterGroup, IProtoFilterItem
{
    [ProtoMember(3)]
    List<IProtoFilterItem> Statements
    {
        get =>
            Items
                .Select(i =>
                    i is FilterGroup
                        ? (IProtoFilterItem)i
                        : (i as IProtoFilterItem)!.Pack()
                )
                .ToList();
        set =>
            Items = new List<IFilterItem>(
                value.Select(i =>
                    i is FilterGroup ? (IFilterItem)i : i.Unpack()
                )
            );
    }

    public string typeRef => "G";

    [JsonIgnore]
    public IFilterGroup? LastGroup { get; set; }

    [ProtoMember(2)]
    public string? ParentPropertyExpression { get; set; }

    [JsonConverter(typeof(FilterItemCollectionJsonConverter))]
    public ICollection<IFilterItem> Items { get; set; } = [];

    [ProtoMember(1)]
    public Connector Connector { get; set; } = Connector.And;

    public FilterGroup() { }

    public FilterGroup(string parentExpression)
    {
        ParentPropertyExpression = parentExpression;
    }

    internal FilterGroup(IFilterGroup? lastGroup)
    {
        LastGroup = lastGroup;
    }

    internal FilterGroup(IFilterGroup lastGroup, string parentExpression)
    {
        ParentPropertyExpression = parentExpression;

        LastGroup = lastGroup;
    }

    public IFilterGroup Close() =>
        LastGroup ?? throw new InvalidOperationException("No group to close");

    public Expression Build(
        Expression member,
        Dictionary<string, uint> parameterLog
    ) =>
        member.GetExpression(
            ParentPropertyExpression,
            parameterLog,
            (p, l) => BuildItems(p, parameterLog) ?? Expression.Constant(true),
            OperationNullHandler.Skip
        );

    Expression? BuildItems(
        Expression member,
        Dictionary<string, uint> parameterLog
    )
    {
        Expression? expr = null;
        var connector = Connector.And;

        foreach (var item in Items)
        {
            var itemExpr = item.Build(member, parameterLog);

            expr =
                expr == null
                    ? itemExpr
                    : expr.JoinExpression(itemExpr, connector);

            connector = item.Connector;
        }

        return expr;
    }

    public override string ToString()
    {
        var sB = new StringBuilder(
            string.IsNullOrWhiteSpace(ParentPropertyExpression)
                ? "("
                : $"{ParentPropertyExpression}.Any( "
        );

        foreach (var item in Items)
            sB.Append(item.ToString());

        var sBString = sB.ToString();

        if (sBString.Length > 4)
            sBString = sBString[..^4].TrimEnd();

        return $"{sBString} ) {Connector:g} ";
    }

    public IFilterItem Unpack() => this;

    public IProtoFilterItem Pack() => this;
}
