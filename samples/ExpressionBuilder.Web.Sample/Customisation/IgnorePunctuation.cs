namespace ExpressionBuilder.Web.Sample.Customisation;

using System.Linq.Expressions;
using System.Text.RegularExpressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// A manipulator of the app's own: compares text without its spaces and punctuation, so "T-shirt" finds
/// "T shirt". It is an ordinary <see cref="IEntityManipulator"/>; Program.cs says how the filter builder
/// should offer it. It runs in memory: a database provider would need a translation for the method.
/// </summary>
public partial struct IgnorePunctuation : IEntityManipulator
{
    static readonly System.Reflection.MethodInfo Method =
        typeof(IgnorePunctuation).GetMethod(nameof(Strip))!;

    public IgnorePunctuation() => Arguments = [];

    public readonly string Name => "IgnorePunctuation";

    public object?[] Arguments { get; set; }

    public readonly Type[] ExpectedTypes => [];

    public readonly Expression ManipulateMember(Expression member) =>
        Expression.Call(Method, member);

    public readonly object? ManipulateValue<TPropertyType>(
        TPropertyType? value
    ) => Strip(value?.ToString());

    public readonly void Validate() { }

    public static string? Strip(string? text) =>
        text == null ? null : NotLettersOrDigits().Replace(text, "");

    [GeneratedRegex(@"[^\p{L}\p{N}]")]
    private static partial Regex NotLettersOrDigits();
}
