namespace TopMarksDevelopment.ExpressionBuilder.Web;

/// <summary>
/// What a filterable property holds, which decides the operations and the input it is offered.
/// </summary>
public enum FieldKind
{
    /// <summary>Text.</summary>
    String,

    /// <summary>A whole or decimal number.</summary>
    Number,

    /// <summary>A date (<see cref="DateOnly"/>, or a <see cref="DateTime"/> marked as a date).</summary>
    Date,

    /// <summary>A date and time.</summary>
    DateTime,

    /// <summary>A time of day or a duration.</summary>
    Time,

    /// <summary>True or false.</summary>
    Boolean,

    /// <summary>One of the members of an enum.</summary>
    Enum,

    /// <summary>A <see cref="Guid"/>.</summary>
    Guid,

    /// <summary>Another object (a one-to-one relationship), which can only be checked for existing.</summary>
    Object,

    /// <summary>A collection, whose items are filtered in a group of their own.</summary>
    Collection,
}

/// <summary>
/// How many values an operation takes.
/// </summary>
public enum ValueArity
{
    /// <summary>None, such as <c>IsNull</c>.</summary>
    None,

    /// <summary>Exactly one.</summary>
    One,

    /// <summary>Exactly two, such as the ends of <c>Between</c>.</summary>
    Two,

    /// <summary>One or more, such as <c>In</c>.</summary>
    Many,
}
