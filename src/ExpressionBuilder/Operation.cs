namespace TopMarksDevelopment.ExpressionBuilder;

public struct Operation
{
    public static Between Between => new();

    public static BetweenExclusive BetweenExclusive => new();

    public static Contains Contains => new();

    public static DoesNotContain DoesNotContain => new();

    public static DoesNotEndWith DoesNotEndWith => new();

    public static DoesNotStartWith DoesNotStartWith => new();

    public static EndsWith EndsWith => new();

    public static Equal Equal => new();

    public static GreaterThan GreaterThan => new();

    public static GreaterThanOrEqual GreaterThanOrEqual => new();

    public static In In => new();

    public static IsEmpty IsEmpty => new();

    public static IsNotEmpty IsNotEmpty => new();

    public static IsNotNull IsNotNull => new();

    public static IsNotNullOrWhiteSpace IsNotNullOrWhiteSpace => new();

    public static IsNull IsNull => new();

    public static IsNullOrWhiteSpace IsNullOrWhiteSpace => new();

    public static LessThan LessThan => new();

    public static LessThanOrEqual LessThanOrEqual => new();

    public static NotBetween NotBetween => new();

    public static NotBetweenExclusive NotBetweenExclusive => new();

    public static NotEqual NotEqual => new();

    public static NotIn NotIn => new();

    public static SmartSearch SmartSearch => new();

    public static StartsWith StartsWith => new();
}
