namespace ExpressionBuilder.Web.Tests.Models;

using System.ComponentModel.DataAnnotations;
using TopMarksDevelopment.ExpressionBuilder.Web;

/// <summary>
/// A class with one of everything the filter builder deals with.
/// </summary>
public class Person
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public string? Email { get; set; }

    public int? Age { get; set; }

    public decimal Salary { get; set; }

    public double Score { get; set; }

    [DataType(DataType.Date)]
    public DateTime Born { get; set; }

    public DateTime? LastSeen { get; set; }

    public DateOnly Joined { get; set; }

    public TimeOnly StartsAt { get; set; }

    public TimeSpan Duration { get; set; }

    public bool Active { get; set; }

    public bool? Verified { get; set; }

    public Colour Favourite { get; set; }

    public Guid Reference { get; set; }

    [Display(Name = "Home address")]
    public Address Home { get; set; } = new();

    public Company? Employer { get; set; }

    public List<Contact> Contacts { get; set; } = [];

    public List<string> Tags { get; set; } = [];

    [FilterBuilderIgnore]
    public string Secret { get; set; } = "";

    public string Nickname_With_Underscores { get; set; } = "";

    public byte[] Photo { get; set; } = [];

    public Dictionary<string, string> Extras { get; set; } = [];

    public string ReadOnly => Name;

    public string this[int index] => Name;
}

public enum Colour
{
    Red,
    Green,

    [Display(Name = "Deep blue")]
    Blue,
}

public class Address
{
    public string Postcode { get; set; } = "";

    public string? City { get; set; }

    public Country? Country { get; set; }
}

public class Country
{
    public string Name { get; set; } = "";

    public Person? Head { get; set; }
}

public class Company
{
    public string Name { get; set; } = "";

    public List<Branch> Branches { get; set; } = [];
}

public class Branch
{
    public string Town { get; set; } = "";

    public int Staff { get; set; }
}

public class Contact
{
    public ContactType Type { get; set; }

    public string Value { get; set; } = "";

    public string? Comments { get; set; }

    public Person? Owner { get; set; }
}

public enum ContactType
{
    Telephone,
    Email,
}
