namespace ExpressionBuilder.Tests;

using System.Collections.Generic;
using ExpressionBuilder.Tests.Models;
using TopMarksDevelopment.ExpressionBuilder;

public class AllTests
{
    public static IEnumerable<object[]> GetAllMatchers() =>
        new List<object[]> { new[] { SmartSearchOneCharacter } };

    static TestBuilder<Product> SmartSearchOneCharacter =>
        new(
            "Matches \"Word1\" exactly (not \"Word123\")",
            x => 
                x.OtherSearchField != null &&
                ((((" " + x.OtherSearchField.Trim().ToLower() + " ").Contains(" Word1 ")) == true)),
            x => x
                .SmartSearch<Product, string?>(
                    x => x.OtherSearchField, 
                    new[]{ "\"Word1\""}
                ),
            x => x
                .SmartSearch(
                    x => x.OtherSearchField, 
                    new[]{ "\"Word1\""}
                ),
            x => x
                .SmartSearch(
                    x => x.OtherSearchField, 
                    new[]{ "\"Word1\""}
                ),
            x => x
                .SmartSearch(
                    x => x.OtherSearchField, 
                    new[]{ "\"Word1\""}
                )
        );
}
