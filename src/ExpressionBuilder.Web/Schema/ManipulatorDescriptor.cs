namespace TopMarksDevelopment.ExpressionBuilder.Web;

using System.Text.RegularExpressions;
using TopMarksDevelopment.ExpressionBuilder.Api;

/// <summary>
/// One piece of text a manipulator needs from the user, such as the text to find.
/// </summary>
/// <param name="Label">What the user sees</param>
/// <param name="Required">Whether it can be left empty</param>
public sealed record ManipulatorParameter(string Label, bool Required = true);

/// <summary>
/// Describes a manipulator to the filter builder, so a user can choose it: what to call it, what it
/// needs from them, and how to make it.
/// </summary>
/// <remarks>
/// A browser only ever names a manipulator and gives it text. It is the descriptor, registered by your app,
/// that decides what that name makes (<see cref="Create"/>), so nothing a browser sends can reach any other
/// manipulator or method. Custom manipulators are listed apart from the built-in ones.
/// </remarks>
public sealed partial class ManipulatorDescriptor
{
    /// <summary>
    /// Describes a manipulator.
    /// </summary>
    /// <param name="name">
    /// A short name made of letters, digits and underscores, which is what is posted. It does not have to be
    /// the manipulator's own <see cref="IEntityManipulator.Name"/>.
    /// </param>
    /// <param name="label">What the user sees</param>
    /// <param name="create">
    /// Makes the manipulator from the text the user gave, one item per parameter. Throw an
    /// <see cref="ArgumentException"/> with a message fit for the user if the text is no good.
    /// </param>
    /// <param name="parameters">What the manipulator needs from the user. None if omitted.</param>
    /// <param name="kinds">The kinds of property it works on. Text only if omitted.</param>
    /// <param name="group">
    /// The heading it is listed under. <see cref="FilterBuilderLabels.CustomGroup"/> if omitted. The
    /// built-in manipulators ignore it.
    /// </param>
    /// <param name="read">
    /// Given a manipulator of a saved filter, returns the text this descriptor would have made it from, or
    /// <c>null</c> if it is not one of this descriptor's. Only needed to show saved filters in the builder
    /// (<see cref="FilterBuilderModel.FromFilter{TClass}"/>).
    /// </param>
    public ManipulatorDescriptor(
        string name,
        string label,
        Func<IReadOnlyList<string>, IEntityManipulator> create,
        IReadOnlyList<ManipulatorParameter>? parameters = null,
        IReadOnlySet<FieldKind>? kinds = null,
        string? group = null,
        Func<IEntityManipulator, IReadOnlyList<string>?>? read = null
    )
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(create);

        if (!ValidName().IsMatch(name))
            throw new ArgumentException(
                "A manipulator's name can only have letters, digits and underscores, and starts with a letter.",
                nameof(name)
            );

        Name = name;
        Label = label;
        Create = create;
        Parameters = parameters ?? [];
        Kinds = kinds ?? new HashSet<FieldKind> { FieldKind.String };
        Group = group;
        Read = read;
    }

    /// <summary>
    /// The name that is posted.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// What the user sees.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// What it needs from the user.
    /// </summary>
    public IReadOnlyList<ManipulatorParameter> Parameters { get; }

    /// <summary>
    /// The kinds of property it works on.
    /// </summary>
    public IReadOnlySet<FieldKind> Kinds { get; }

    /// <summary>
    /// The heading a custom manipulator is listed under.
    /// </summary>
    public string? Group { get; }

    /// <summary>
    /// Whether this is one of the manipulators that ships with the package, rather than one added by the
    /// app. Built-in manipulators cannot be replaced or removed, only left out of a builder.
    /// </summary>
    public bool IsBuiltIn { get; internal init; }

    internal Func<IReadOnlyList<string>, IEntityManipulator> Create { get; }

    internal Func<IEntityManipulator, IReadOnlyList<string>?>? Read { get; }

    /// <summary>
    /// Describes a manipulator class, which needs a public parameterless constructor and takes its
    /// <see cref="IEntityManipulator.Arguments"/> as the text the user gave. Its
    /// <see cref="IEntityManipulator.Name"/> is used as the name.
    /// </summary>
    /// <typeparam name="TManipulator">The manipulator</typeparam>
    /// <param name="label">What the user sees</param>
    /// <param name="parameters">What it needs from the user. None if omitted.</param>
    /// <param name="kinds">The kinds of property it works on. Text only if omitted.</param>
    /// <param name="group">The heading it is listed under</param>
    public static ManipulatorDescriptor For<TManipulator>(
        string label,
        IReadOnlyList<ManipulatorParameter>? parameters = null,
        IReadOnlySet<FieldKind>? kinds = null,
        string? group = null
    )
        where TManipulator : IEntityManipulator, new() =>
        new(
            new TManipulator().Name,
            label,
            text =>
            {
                IEntityManipulator manipulator = new TManipulator();

                manipulator.Arguments = [.. text];

                return manipulator;
            },
            parameters,
            kinds,
            group,
            manipulator =>
                manipulator is TManipulator
                && manipulator.Arguments.All(a => a is string)
                    ? [.. manipulator.Arguments.Cast<string>()]
                    : null
        );

    [GeneratedRegex("^[A-Za-z][A-Za-z0-9_]{0,63}$")]
    private static partial Regex ValidName();
}
