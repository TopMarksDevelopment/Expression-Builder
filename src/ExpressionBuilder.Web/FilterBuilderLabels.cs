namespace TopMarksDevelopment.ExpressionBuilder.Web;

/// <summary>
/// The text of the filter builder, so it can be translated or reworded.
/// </summary>
/// <remarks>
/// These are used by the Razor view and by the script. Set them once, when you register the filter
/// builder, and both pick them up.
/// </remarks>
public sealed class FilterBuilderLabels
{
    /// <summary>Heading of the top level group.</summary>
    public string Conditions { get; set; } = "Conditions";

    /// <summary>Label of a group of conditions in brackets.</summary>
    public string Group { get; set; } = "Group";

    /// <summary>
    /// Heading of a group that filters a collection. <c>{0}</c> is the name of the collection.
    /// </summary>
    public string CollectionGroup { get; set; } = "Any {0} where";

    /// <summary>Shown in an empty group.</summary>
    public string EmptyGroup { get; set; } = "Add a condition to this group.";

    /// <summary>Button that adds a condition.</summary>
    public string AddCondition { get; set; } = "Add condition";

    /// <summary>Button that adds a group.</summary>
    public string AddGroup { get; set; } = "Add group";

    /// <summary>Heading of the collections in the list of properties.</summary>
    public string Collections { get; set; } = "Collections (any item where…)";

    /// <summary>Button that removes a condition.</summary>
    public string RemoveCondition { get; set; } = "Remove condition";

    /// <summary>Button that removes a group.</summary>
    public string RemoveGroup { get; set; } = "Remove group";

    /// <summary>Joins two conditions where both must match.</summary>
    public string And { get; set; } = "And";

    /// <summary>Joins two conditions where either can match.</summary>
    public string Or { get; set; } = "Or";

    /// <summary>Label of the connector between conditions.</summary>
    public string Connector { get; set; } = "How this connects to the next";

    /// <summary>Label of the property choice.</summary>
    public string Field { get; set; } = "Property";

    /// <summary>Prompt of the property choice.</summary>
    public string FieldPlaceholder { get; set; } = "Choose a property…";

    /// <summary>Label of the condition choice.</summary>
    public string Operation { get; set; } = "Condition";

    /// <summary>Prompt of the condition choice.</summary>
    public string OperationPlaceholder { get; set; } = "Choose a condition…";

    /// <summary>Label of a value.</summary>
    public string Value { get; set; } = "Value";

    /// <summary>Label of the first value of a range.</summary>
    public string ValueFrom { get; set; } = "From";

    /// <summary>Label of the second value of a range.</summary>
    public string ValueTo { get; set; } = "To";

    /// <summary>Button that adds another value.</summary>
    public string AddValue { get; set; } = "Add value";

    /// <summary>Button that removes a value.</summary>
    public string RemoveValue { get; set; } = "Remove value";

    /// <summary>Button that shows the advanced options of a condition.</summary>
    public string Options { get; set; } = "Options";

    /// <summary>Label of the match choice.</summary>
    public string Match { get; set; } = "When there are several values, match";

    /// <summary>Match any of the values.</summary>
    public string MatchAny { get; set; } = "any of them";

    /// <summary>Match all of the values.</summary>
    public string MatchAll { get; set; } = "all of them";

    /// <summary>Heading of the manipulators.</summary>
    public string Manipulators { get; set; } =
        "Adjust the text before comparing";

    /// <summary>Remove spaces from both ends.</summary>
    public string Trim { get; set; } = "Trim spaces";

    /// <summary>Make lower case.</summary>
    public string ToLower { get; set; } = "Make lower case";

    /// <summary>Make upper case.</summary>
    public string ToUpper { get; set; } = "Make upper case";

    /// <summary>Replace text.</summary>
    public string Replace { get; set; } = "Replace text";

    /// <summary>Text to find.</summary>
    public string ReplaceFind { get; set; } = "Find";

    /// <summary>Text to replace it with.</summary>
    public string ReplaceWith { get; set; } = "Replace with";

    /// <summary>The <c>true</c> choice.</summary>
    public string Yes { get; set; } = "Yes";

    /// <summary>The <c>false</c> choice.</summary>
    public string No { get; set; } = "No";

    /// <summary>Shown when there are no conditions.</summary>
    public string SummaryEmpty { get; set; } =
        "No conditions, so everything matches.";

    /// <summary>Shown when some conditions are incomplete.</summary>
    public string Incomplete { get; set; } = "Some conditions are incomplete.";

    /// <summary>Shown beside a field that needs a value.</summary>
    public string Required { get; set; } = "Required";

    /// <summary>Shown while the page's script has not started.</summary>
    public string NeedsScript { get; set; } =
        "The filter builder needs JavaScript to be turned on.";
}
