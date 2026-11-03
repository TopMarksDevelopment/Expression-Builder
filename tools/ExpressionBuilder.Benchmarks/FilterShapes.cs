namespace ExpressionBuilder.Benchmarks;

using TopMarksDevelopment.ExpressionBuilder;

/// <summary>
/// Filters of a known size, built the way a saved or posted filter is (by property name), so every benchmark uses the
/// same ones. A "condition" is one comparison; a group or a collection holds three of them.
/// </summary>
static class FilterShapes
{
    /// <summary>A filter of about this many conditions: mostly flat, a bracketed group in every ten and a collection (with a collection inside it) in every twenty-five.</summary>
    /// <param name="enums">Whether to use this project's enums. A saved filter with one cannot be read back (its type name has no assembly), so the serialisation benchmarks leave them out</param>
    public static Filter<Large> Build(int conditions, bool enums = true)
    {
        var filter = new Filter<Large>();
        var added = 0;

        while (added < conditions)
        {
            if (added % 25 == 20 && added + 3 <= conditions)
            {
                Collection(filter, added);
                added += 3;
            }
            else if (added % 10 == 5 && added + 3 <= conditions)
            {
                filter.OpenGroup();
                Condition(filter, added, enums).Or();
                Condition(filter, added + 1, enums).Or();
                Condition(filter, added + 2, enums);
                filter.CloseGroup();
                added += 3;
            }
            else
            {
                Condition(filter, added, enums);
                added++;
            }
        }

        return filter;
    }

    static void Collection(Filter<Large> filter, int i)
    {
        filter.OpenCollection("Orders");
        filter.Add("Total", Operation.GreaterThan, A(100m + i));
        filter.Add("Placed", Operation.GreaterThan, A(Date(i)));
        filter.OpenCollection("Lines");
        filter.Add("Quantity", Operation.GreaterThanOrEqual, A(10 + i));
        filter.CloseCollection();
        filter.CloseCollection();
    }

    /// <summary>One flat condition. They differ in type (text, numbers, dates, enums, ids), in the number of values, and in what they reach (a navigation).</summary>
    static TopMarksDevelopment.ExpressionBuilder.Api.IFilterConnection<Large> Condition(
        Filter<Large> filter,
        int i,
        bool enums
    ) =>
        (i % 14) switch
        {
            0 => filter.Add("Name", Operation.Contains, A($"name{i}")),
            1 => filter.Add(
                "Age",
                Operation.GreaterThanOrEqual,
                A(18 + i % 40)
            ),
            2 => filter.Add("Active", Operation.Equal, A(true)),
            3 => filter.Add(
                "Created",
                Operation.Between,
                A(Date(i), Date(i + 30))
            ),
            4 => enums
                ? filter.Add("Tier", Operation.Equal, A(Tier.Gold))
                : filter.Add("Level", Operation.Equal, A((short)3)),
            5 => filter.Add("Salary", Operation.LessThan, A(50_000m + i)),
            6 => filter.Add("Email", Operation.EndsWith, A("@example.com")),
            7 => enums
                ? filter.Add(
                    "Region",
                    Operation.In,
                    A(Region.North, Region.East)
                )
                : filter.Add("Points", Operation.In, A(1L, 2L)),
            8 => filter.Add("Home.City", Operation.Equal, A("London")),
            9 => filter.Add("Rank", Operation.GreaterThan, A(i % 100)),
            10 => filter.Add("Score", Operation.GreaterThan, A(1.5 + i)),
            11 => filter.Add("Code", Operation.StartsWith, A("AB", "CD")),
            12 => filter.Add("Employer.Name", Operation.Equal, A("Acme")),
            _ => filter.Add(
                "Reference",
                Operation.Equal,
                A(Guid.Parse("11111111-1111-1111-1111-111111111111"))
            ),
        };

    static T[] A<T>(params T[] values) => values;

    static DateTime Date(int i) => new DateTime(2024, 1, 1).AddDays(i);

    /// <summary>
    /// The three-condition filter written by hand, the way the library writes it: text operations trim and lower-case
    /// both sides unless told not to, and there is a null check on the way.
    /// </summary>
    public static System.Linq.Expressions.Expression<
        Func<Large, bool>
    > ThreeByHand() =>
        x =>
            x.Name != null
            && x.Name.Trim().ToLower().Contains("name0")
            && x.Age >= 19
            && x.Active == true;

    /// <summary>The lambda most people would write: no trimming, no case-folding. It means something slightly different.</summary>
    public static System.Linq.Expressions.Expression<
        Func<Large, bool>
    > ThreeSimple() =>
        x =>
            x.Name != null
            && x.Name.Contains("name0")
            && x.Age >= 19
            && x.Active == true;

    /// <summary>The same three, through the string form (the lambda form is the same filter).</summary>
    public static Filter<Large> ThreeByName() => Build(3);

    /// <summary>People to filter, the same for every run.</summary>
    public static List<Large> Items(int count)
    {
        var random = new Random(42);
        var items = new List<Large>(count);

        for (var i = 0; i < count; i++)
            items.Add(
                new Large
                {
                    Id = i,
                    Name = random.Next(3) == 0 ? $" Name0-{i} " : $"other{i}",
                    Age = random.Next(10, 70),
                    Active = random.Next(2) == 0,
                }
            );

        return items;
    }
}
