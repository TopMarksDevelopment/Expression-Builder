namespace TopMarksDevelopment.ExpressionBuilder.Extensions;

using System.Collections.Generic;

public static class DictionaryExtensions
{
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