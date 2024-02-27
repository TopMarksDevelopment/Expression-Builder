namespace TopMarksDevelopment.ExpressionBuilder.Api.Helpers;

using System;
using System.Reflection;

public struct HelperMethods
{
    public static MethodInfo ContainsMethod 
        => typeof(string).GetMethod("Contains", [typeof(string)])!;
        
    public static MethodInfo StartsWithMethod
        => typeof(string).GetMethod("StartsWith", [typeof(string)])!;

    public static MethodInfo EndsWithMethod
        => typeof(string).GetMethod("EndsWith", [typeof(string)])!;

    public static MethodInfo ConcatMethod
        => typeof(string).GetMethod("Concat", [typeof(string), typeof(string)])!;

    public static MethodInfo TrimMethod
        => typeof(string).GetMethod("Trim", Type.EmptyTypes)!;

    public static MethodInfo ToLowerMethod
        => typeof(string).GetMethod("ToLower", Type.EmptyTypes)!;

}