namespace TopMarksDevelopment.ExpressionBuilder.Extensions;

/// <summary>
/// Helpers for the log of parameter names used while building an expression.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Gets a parameter name that has not been used yet, based on a letter
    /// (<c>x</c>, then <c>x1</c>, <c>x2</c>, ...).
    /// </summary>
    /// <param name="dictionary">The log of names used so far, which is updated</param>
    /// <param name="baseLetter">The name to start from</param>
    /// <returns>A name that is unique within the expression</returns>
    public static string GetMeaningfulUnique(
        this Dictionary<string, uint> dictionary,
        string baseLetter
    )
    {
        if (dictionary.TryGetValue(baseLetter, out uint value))
        {
            value += 1;

            dictionary[baseLetter] = value;

            return $"{baseLetter}{value}";
        }
        else
        {
            dictionary.Add(baseLetter, 0);

            return baseLetter;
        }
    }
}
