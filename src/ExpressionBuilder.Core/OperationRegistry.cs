namespace TopMarksDevelopment.ExpressionBuilder;

using System.Reflection;
using TopMarksDevelopment.ExpressionBuilder.Api;
using TopMarksDevelopment.ExpressionBuilder.Registry;

/// <summary>
/// Finds <see cref="IOperation"/>s by name. This is how a saved filter (JSON or <c>byte[]</c>)
/// gets back its operations.
/// </summary>
/// <remarks>
/// <para>
/// The built-in operations (and any other operation in an Expression Builder package you reference)
/// are found automatically, even if your code has not used them yet.
/// </para>
/// <para>
/// An operation in your own assembly is found too if that assembly is already loaded. If it is not
/// (for example, an operations library that nothing has touched yet), register it with
/// <see cref="Register{TOperation}"/> or <see cref="RegisterAssembly(Assembly)"/> when your
/// application starts.
/// </para>
/// </remarks>
public static class OperationRegistry
{
    static readonly NamedTypeRegistry<IOperation> _registry = new(o => o.Name);

    /// <summary>
    /// Registers an operation so it can be found by its <see cref="IOperation.Name"/>. An operation
    /// already registered with the same name is replaced.
    /// </summary>
    /// <typeparam name="TOperation">The operation, which needs a public parameterless constructor</typeparam>
    public static void Register<TOperation>()
        where TOperation : IOperation, new() =>
        _registry.Register(typeof(TOperation));

    /// <summary>
    /// Registers every operation in an assembly. Operations that are already registered keep
    /// their registration.
    /// </summary>
    /// <param name="assembly">The assembly to search</param>
    public static void RegisterAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        _registry.RegisterAssembly(assembly);
    }

    /// <summary>
    /// Gets a new instance of the operation with this name.
    /// </summary>
    /// <param name="name">The operation's <see cref="IOperation.Name"/></param>
    /// <param name="operation">The operation, if there is one</param>
    /// <returns>True if the operation exists</returns>
    public static bool TryGet(string name, out IOperation operation)
    {
        ArgumentNullException.ThrowIfNull(name);

        return _registry.TryGet(name, out operation);
    }

    /// <summary>
    /// Gets a new instance of the operation with this name.
    /// </summary>
    /// <param name="name">The operation's <see cref="IOperation.Name"/></param>
    /// <returns>The operation</returns>
    /// <exception cref="TypeLoadException">There is no operation with this name</exception>
    public static IOperation Get(string name) =>
        TryGet(name, out var operation)
            ? operation
            : throw new TypeLoadException(
                $"Could not find the operation \"{name}\". If it is a custom operation, "
                    + $"register it with {nameof(OperationRegistry)}.{nameof(Register)}."
            );

    /// <summary>
    /// Gets a new instance of every operation that can be found, ordered by name.
    /// </summary>
    public static IReadOnlyList<IOperation> GetAll() => _registry.GetAll();
}
