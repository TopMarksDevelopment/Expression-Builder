namespace TopMarksDevelopment.ExpressionBuilder.Api.Helpers;

using System.Reflection;

/// <summary>
/// Helper class for expression methods
/// </summary>
public struct HelperMethods
{
    /// <summary>
    /// The method info for the Contains method
    /// </summary>
    public static MethodInfo ContainsMethod =>
        typeof(string).GetMethod("Contains", [typeof(string)])!;

    /// <summary>
    /// The method info for the StartsWith method
    /// </summary>
    public static MethodInfo StartsWithMethod =>
        typeof(string).GetMethod("StartsWith", [typeof(string)])!;

    /// <summary>
    /// The method info for the EndsWith method
    /// </summary>
    public static MethodInfo EndsWithMethod =>
        typeof(string).GetMethod("EndsWith", [typeof(string)])!;

    /// <summary>
    /// The method info for the Concat method
    /// </summary>
    public static MethodInfo ConcatMethod =>
        typeof(string).GetMethod("Concat", [typeof(string), typeof(string)])!;

    /// <summary>
    /// The method info for the Trim method
    /// </summary>
    public static MethodInfo TrimMethod =>
        typeof(string).GetMethod("Trim", Type.EmptyTypes)!;

    /// <summary>
    /// The method info for the ToLower method
    /// </summary>
    public static MethodInfo ToLowerMethod =>
        typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;
}
