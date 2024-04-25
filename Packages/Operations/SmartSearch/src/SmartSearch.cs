namespace TopMarksDevelopment.ExpressionBuilder.Operations;

using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using TopMarksDevelopment.ExpressionBuilder.Api;
using H = Api.Helpers.HelperMethods;

public partial struct SmartSearch : IOperation
{
    public SmartSearch() { }

    public readonly string Name => "SmartSearch";

    public readonly OperationDefaults Defaults =>
        new()
        {
            Match = Matches.All,
            NullHandler = OperationNullHandler.NotNullAnd
        };

    public readonly Expression Build<TPropertyType>(
        Expression member,
        IFilterCollection<TPropertyType?> values,
        IFilterStatementOptions? options
    )
    {
        if (!values.Any())
            throw new ArgumentOutOfRangeException(
                nameof(values),
                "There must be only 1 value"
            );

        var memberExpr = member.ToTrimLowerStringExpression();
        Expression? exactMatchExpr = null;

        Expression expr = null!;
        foreach (var term in values)
        {
            var matcher = true;
            var workingExpression = memberExpr;
            var workingTerm = options.ApplyManipulators(term)?.ToString() ?? "";

            if (workingTerm.StartsWith('-'))
            {
                matcher = false;
                workingTerm = workingTerm[1..];
            }

            if (workingTerm.StartsWith('"') && workingTerm.EndsWith('"'))
            {
                workingTerm = workingTerm[1..^1];

                if (!workingTerm.StartsWith(' ') && !workingTerm.EndsWith(' '))
                    workingTerm = " " + workingTerm + " ";

                exactMatchExpr ??= Expression.Add(
                    Expression.Add(
                        Expression.Constant(" "),
                        workingExpression,
                        H.ConcatMethod
                    ),
                    Expression.Constant(" "),
                    H.ConcatMethod
                );

                workingExpression = exactMatchExpr;
            }

            expr = expr.JoinExpression(
                Expression.Equal(
                    Expression.Call(
                        workingExpression,
                        H.ContainsMethod,
                        Expression.Constant(workingTerm)
                    ),
                    Expression.Constant(matcher)
                ),
                options?.Match == Matches.Any ? Connector.Or : Connector.And
            );
        }

        return expr;
    }

    public readonly void Validate(IFilterStatement statement)
    {
        if (!statement.Values.Any())
            throw new ArgumentOutOfRangeException(
                nameof(statement.Values),
                "There must be atleast 1 value"
            );
    }

    public static string[] SplitTerm(string input) =>
        SplitTermRegex().Split(input);

    [GeneratedRegex(" (?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)")]
    private static partial Regex SplitTermRegex();
}
