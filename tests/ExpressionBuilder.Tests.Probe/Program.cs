using System.Text.Json;
using TopMarksDevelopment.ExpressionBuilder;

// Reads a saved filter (JSON) from standard input, applies it to a fixed list of people and prints
// the ids that match.
//
// This deliberately never mentions an operation type. Every operation the filter needs is found by
// name, as it would be in an app that loads filters from a database before it has built any.
var json = Console.In.ReadToEnd();

var filter =
    JsonSerializer.Deserialize<Filter<Person>>(json)
    ?? throw new InvalidOperationException("No filter was read");

Person[] people =
[
    new(1, "Alan"),
    new(2, "Barbara"),
    new(3, "Bob"),
    new(4, "Brian"),
    new(5, null),
    new(6, "Bella"),
];

Console.WriteLine(string.Join(",", people.Where(filter).Select(p => p.Id)));

return 0;

record Person(int Id, string? Name);
