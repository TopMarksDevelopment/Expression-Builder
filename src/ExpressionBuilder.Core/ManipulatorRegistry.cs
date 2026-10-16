namespace TopMarksDevelopment.ExpressionBuilder;

using System.Reflection;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Registry;

/// <summary>
/// Finds <see cref="IEntityManipulator"/>s by name, so saved filters get their manipulators back.
/// </summary>
/// <remarks>
/// Works like <see cref="OperationRegistry"/>. Method manipulators (such as
/// <c>x => x.Name.Trim()</c>) are not registered: they are rebuilt from the method they call.
/// </remarks>
public static class ManipulatorRegistry
{
    static readonly NamedTypeRegistry<IEntityManipulator> _registry = new(
        m => m.Name,
        exclude: t => t == typeof(ExpressionMethodManipulator)
    );

    /// <summary>
    /// Registers a manipulator so it can be found by its <see cref="IEntityManipulator.Name"/>.
    /// </summary>
    /// <typeparam name="TManipulator">The manipulator, which needs a public parameterless constructor</typeparam>
    public static void Register<TManipulator>()
        where TManipulator : IEntityManipulator, new() =>
        _registry.Register(typeof(TManipulator));

    /// <summary>
    /// Registers every manipulator in an assembly.
    /// </summary>
    /// <param name="assembly">The assembly to search</param>
    public static void RegisterAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        _registry.RegisterAssembly(assembly);
    }

    /// <summary>
    /// Gets a new instance of the manipulator with this name.
    /// </summary>
    /// <param name="name">The manipulator's <see cref="IEntityManipulator.Name"/></param>
    /// <param name="manipulator">The manipulator, if there is one</param>
    /// <returns>True if the manipulator exists</returns>
    public static bool TryGet(string name, out IEntityManipulator manipulator)
    {
        ArgumentNullException.ThrowIfNull(name);

        return _registry.TryGet(name, out manipulator);
    }
}
