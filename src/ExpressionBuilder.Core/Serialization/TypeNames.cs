namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

/// <summary>
/// The name a saved filter gives the type of a statement's values, so that the type can be found again.
/// </summary>
internal static class TypeNames
{
    /// <summary>
    /// Gets a name that <see cref="Type.GetType(string)"/> can find from any assembly.
    /// </summary>
    /// <remarks>
    /// <c>Type.GetType</c> only looks in the assembly that calls it and in the core library, so the plain name of a type
    /// of the application (an enum, say) is not found when the filter is read back. Those names say where the type is
    /// (<c>App.Tier, App</c>). The core library's types keep their plain name, which is what has always been saved, and is
    /// shorter. The assembly's version is left out, so a saved filter survives the application being rebuilt.
    /// </remarks>
    internal static string For(Type type) =>
        type.FullName is { } name && type.Assembly != typeof(object).Assembly
            ? $"{name}, {type.Assembly.GetName().Name}"
            : type.FullName ?? type.AssemblyQualifiedName ?? type.Name;
}
