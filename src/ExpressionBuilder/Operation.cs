namespace TopMarksDevelopment.ExpressionBuilder;

/// <summary>
/// Every built-in operation, ready to pass to <c>filter.Add(...)</c>.
/// </summary>
/// <example>
/// <code>
/// filter.Add("Id", Operation.Between, 2, 4);
/// </code>
/// </example>
public static class Operation
{
    /// <summary>A new <see cref="Operations.Between"/> operation.</summary>
    public static Between Between => new();

    /// <summary>A new <see cref="Operations.BetweenExclusive"/> operation.</summary>
    public static BetweenExclusive BetweenExclusive => new();

    /// <summary>A new <see cref="Operations.Contains"/> operation.</summary>
    public static Contains Contains => new();

    /// <summary>A new <see cref="Operations.DoesNotContain"/> operation.</summary>
    public static DoesNotContain DoesNotContain => new();

    /// <summary>A new <see cref="Operations.DoesNotEndWith"/> operation.</summary>
    public static DoesNotEndWith DoesNotEndWith => new();

    /// <summary>A new <see cref="Operations.DoesNotStartWith"/> operation.</summary>
    public static DoesNotStartWith DoesNotStartWith => new();

    /// <summary>A new <see cref="Operations.EndsWith"/> operation.</summary>
    public static EndsWith EndsWith => new();

    /// <summary>A new <see cref="Operations.Equal"/> operation.</summary>
    public static Equal Equal => new();

    /// <summary>A new <see cref="Operations.GreaterThan"/> operation.</summary>
    public static GreaterThan GreaterThan => new();

    /// <summary>A new <see cref="Operations.GreaterThanOrEqual"/> operation.</summary>
    public static GreaterThanOrEqual GreaterThanOrEqual => new();

    /// <summary>A new <see cref="Operations.In"/> operation.</summary>
    public static In In => new();

    /// <summary>A new <see cref="Operations.IsEmpty"/> operation.</summary>
    public static IsEmpty IsEmpty => new();

    /// <summary>A new <see cref="Operations.IsNotEmpty"/> operation.</summary>
    public static IsNotEmpty IsNotEmpty => new();

    /// <summary>A new <see cref="Operations.IsNotNull"/> operation.</summary>
    public static IsNotNull IsNotNull => new();

    /// <summary>A new <see cref="Operations.IsNotNullOrWhiteSpace"/> operation.</summary>
    public static IsNotNullOrWhiteSpace IsNotNullOrWhiteSpace => new();

    /// <summary>A new <see cref="Operations.IsNull"/> operation.</summary>
    public static IsNull IsNull => new();

    /// <summary>A new <see cref="Operations.IsNullOrWhiteSpace"/> operation.</summary>
    public static IsNullOrWhiteSpace IsNullOrWhiteSpace => new();

    /// <summary>A new <see cref="Operations.LessThan"/> operation.</summary>
    public static LessThan LessThan => new();

    /// <summary>A new <see cref="Operations.LessThanOrEqual"/> operation.</summary>
    public static LessThanOrEqual LessThanOrEqual => new();

    /// <summary>A new <see cref="Operations.NotBetween"/> operation.</summary>
    public static NotBetween NotBetween => new();

    /// <summary>A new <see cref="Operations.NotBetweenExclusive"/> operation.</summary>
    public static NotBetweenExclusive NotBetweenExclusive => new();

    /// <summary>A new <see cref="Operations.NotEqual"/> operation.</summary>
    public static NotEqual NotEqual => new();

    /// <summary>A new <see cref="Operations.NotIn"/> operation.</summary>
    public static NotIn NotIn => new();

    /// <summary>A new <see cref="Operations.SmartSearch"/> operation.</summary>
    public static SmartSearch SmartSearch => new();

    /// <summary>A new <see cref="Operations.StartsWith"/> operation.</summary>
    public static StartsWith StartsWith => new();
}
