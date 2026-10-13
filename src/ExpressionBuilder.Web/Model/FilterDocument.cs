namespace TopMarksDevelopment.ExpressionBuilder.Web;

using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// The posted form of a filter: what the script writes and the server reads.
/// </summary>
/// <remarks>
/// <code>
/// { "version": 1, "root": { "type": "group", "connector": "And", "items": [
///     { "type": "condition", "field": "Name", "operation": "Contains", "values": ["bike"],
///       "match": "Any", "manipulators": [{ "name": "ToLower" }], "connector": "Or" },
///     { "type": "group", "collection": "Contacts", "connector": "And", "items": [ ... ] }
/// ] } }
/// </code>
/// It is deliberately not the library's own serialised filter: that names .NET types, which a
/// browser should never get to choose. Here everything is plain text that is checked against the
/// <see cref="FilterSchema"/>. As in the library, a <c>connector</c> says how an item joins the
/// <em>next</em> one.
/// </remarks>
sealed class FilterDocument
{
    public const int CurrentVersion = 1;

    public static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false,
            MaxDepth = 64,
        };

    public int Version { get; set; } = CurrentVersion;

    public FilterNode? Root { get; set; }

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);
}

/// <summary>
/// A condition or a group. One shape serves both so it can be read without guessing; which members
/// matter depends on <see cref="Type"/>.
/// </summary>
sealed class FilterNode
{
    public const string ConditionType = "condition";
    public const string GroupType = "group";

    /// <summary><c>condition</c> or <c>group</c>.</summary>
    public string? Type { get; set; }

    /// <summary>How this item joins the next: <c>And</c> or <c>Or</c>.</summary>
    public string? Connector { get; set; }

    /// <summary>Condition: the property, as a path such as <c>Birth.Country</c>.</summary>
    public string? Field { get; set; }

    /// <summary>Condition: the name of the operation.</summary>
    public string? Operation { get; set; }

    /// <summary>Condition: the values, as text.</summary>
    public List<string?>? Values { get; set; }

    /// <summary>Condition: <c>Any</c> or <c>All</c> of several values.</summary>
    public string? Match { get; set; }

    /// <summary>Condition: changes applied to the property and the values first.</summary>
    public List<ManipulatorNode>? Manipulators { get; set; }

    /// <summary>Group: the collection it filters the items of, if it filters one.</summary>
    public string? Collection { get; set; }

    /// <summary>Group: what is in it.</summary>
    public List<FilterNode>? Items { get; set; }
}

/// <summary>
/// A change applied to a text property (and the values it is compared with).
/// </summary>
sealed class ManipulatorNode
{
    public const string Trim = "Trim";
    public const string ToLower = "ToLower";
    public const string ToUpper = "ToUpper";
    public const string Replace = "Replace";

    /// <summary>One of <c>Trim</c>, <c>ToLower</c>, <c>ToUpper</c> or <c>Replace</c>.</summary>
    public string? Name { get; set; }

    /// <summary><c>Replace</c> only: the text to find.</summary>
    public string? Find { get; set; }

    /// <summary><c>Replace</c> only: the text to put in its place.</summary>
    public string? With { get; set; }
}
