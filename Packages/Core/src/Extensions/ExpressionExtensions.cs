namespace TopMarksDevelopment.ExpressionBuilder.Extensions;

using System;
using System.Linq.Expressions;
using System.Reflection;

internal static class ExpressionExtensions
{
    internal static Expression GetExpression(
        this ParameterExpression param,
        string? propertyChain,
        Dictionary<string, uint> parameterLog,
        Func<Expression, Dictionary<string, uint>, Expression> applyExpressions,
        OperationNullHandler nullHandler
    ) =>
        ((Expression)param).GetExpression(
            propertyChain,
            parameterLog,
            applyExpressions,
            nullHandler
        );

    /// <summary>
    /// Gets a member expression for an specific property
    /// </summary>
    /// <param name="param"></param>
    /// <param name="propertyChain"></param>
    /// <returns></returns>
    internal static Expression GetExpression(
        this Expression member,
        string? propertyChain,
        Dictionary<string, uint> parameterLog,
        Func<Expression, Dictionary<string, uint>, Expression> applyExpressions,
        OperationNullHandler nullHandler
    )
    {
        if (string.IsNullOrWhiteSpace(propertyChain))
            return applyExpressions.Invoke(member, parameterLog);

        ParseChain(propertyChain, out var propertyName, out var restOfTerm);

        var subMember = member.GetMemberExpression(propertyName);

        if (
            !propertyChain.Contains('.')
            && !propertyChain.Contains("[]")
            && (
                nullHandler == OperationNullHandler.Skip
                || IsNotNullable(subMember)
            )
        )
            return applyExpressions.Invoke(subMember, parameterLog);

        if (propertyName.EndsWith("[]"))
            return member.HandleCollections(
                propertyChain,
                parameterLog,
                applyExpressions,
                nullHandler
            );

        if (IsNotNullable(subMember))
            return GetExpression(
                member.GetMemberExpression(propertyName),
                restOfTerm,
                parameterLog,
                applyExpressions,
                nullHandler
            );

        return ApplyNullHandler(
            GetExpression(
                member.GetMemberExpression(
                    propertyName
                        + (
                            restOfTerm.StartsWith("Value")
                            || Nullable.GetUnderlyingType(subMember.Type)
                                == null
                                ? ""
                                : ".Value"
                        )
                ),
                restOfTerm,
                parameterLog,
                applyExpressions,
                nullHandler
            )
        );

        BinaryExpression ApplyNullHandler(Expression right) =>
            nullHandler == OperationNullHandler.IsNullOr
                ? Expression.OrElse(Expression.Equal(subMember, Expression.Constant(null)), right)
                : Expression.AndAlso(Expression.NotEqual(subMember, Expression.Constant(null)), right);
    }

    static Expression HandleCollections(
        this Expression param,
        string? propertyChain,
        Dictionary<string, uint> parameterLog,
        Func<Expression, Dictionary<string, uint>, Expression> applyExpressions,
        OperationNullHandler nullHandler
    )
    {
        if (
            string.IsNullOrWhiteSpace(propertyChain)
            || !propertyChain.Contains("[]")
        )
            return applyExpressions.Invoke(param, parameterLog);

        ParseChain(propertyChain, out var propertyName, out var restOfTerm);

        var embededProperty =
            param.GetPropertyInfo(propertyName[..^2])
            ?? throw new NullReferenceException(
                $"Could not find property \"{propertyChain}\" in \"{param.Type}\""
            );

        var type = embededProperty.PropertyType.GetGenericArguments()[0];
        var listParam = Expression.Parameter(
            type,
            parameterLog.GetMeaningfulUnique(
                embededProperty.Name.ToLower()[..1]
            )
        );

        var anyInfo = typeof(Enumerable)
            .GetMethods(BindingFlags.Static | BindingFlags.Public)
            .First(m => m.Name == "Any" && m.GetParameters().Length == 2);

        anyInfo = anyInfo.MakeGenericMethod(type);

        return Expression.Call(
            anyInfo,
            param.GetMemberExpression(propertyName[..^2]),
            Expression.Lambda(
                listParam.GetExpression(
                    restOfTerm,
                    parameterLog,
                    applyExpressions,
                    nullHandler
                ),
                listParam
            )
        );
    }

    static bool IsNotNullable(Expression member) =>
        member.Type.IsClass
            ? !member.Type.CustomAttributes.Any(x =>
                x.AttributeType.FullName
                == "System.Runtime.CompilerServices.NullableAttribute"
            )
            : Nullable.GetUnderlyingType(member.Type) == null;

    static void ParseChain(
        string propertyChain,
        out string propertyName,
        out string restOfTerm
    )
    {
        var index = propertyChain.IndexOf('.');
        propertyName = index == -1 ? propertyChain : propertyChain[..index];
        restOfTerm = index == -1 ? "" : propertyChain[(index + 1)..];
    }

    static PropertyInfo? GetPropertyInfo(
        this Expression param,
        string propertyChain
    )
    {
        if (!propertyChain.Contains('.'))
            return param.Type.GetProperty(propertyChain);

        var index = propertyChain.IndexOf('.');
        var x = param.GetMemberExpression(propertyChain[..index]);

        return x.Type.GetProperty(propertyChain[(index + 1)..]);
    }

    static MemberExpression GetMemberExpression(
        this Expression param,
        string propertyChain
    )
    {
        if (!propertyChain.Contains('.'))
            return Expression.PropertyOrField(
                param,
                propertyChain.EndsWith("[]")
                    ? propertyChain[..^2]
                    : propertyChain
            );

        var index = propertyChain.IndexOf('.');
        var propName = propertyChain[..index];
        var subParam = Expression.PropertyOrField(
            param,
            propName.EndsWith("[]") ? propName[..^2] : propName
        );

        return GetMemberExpression(subParam, propertyChain[(index + 1)..]);
    }
}
