using System.Linq.Expressions;

namespace TopMarksDevelopment.ExpressionBuilder.Api;

public interface IEntityManipulator
{
    /// <summary>
    /// The name of the method or the unique id of the manipulator that's been created
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The arguments we're passing to the method
    /// </summary>
    object?[] Arguments { get; set; }

    /// <summary>
    /// What types are expected to be passed into the method
    /// </summary>
    Type[] ExpectedTypes { get; }

    /// <summary>
    /// The method to manipulate the member
    /// </summary>
    /// <param name="member">The member we're manipulating</param>
    /// <returns>The newly manipulated member</returns>
    Expression ManipulateMember(Expression member);

    /// <summary>
    /// The method to manipulate the one of the passed values
    /// </summary>
    /// <typeparam name="TPropertyType">The type of the parameter we're passing</typeparam>
    /// <param name="value">The value we're going to be manipulating</param>
    /// <returns>The newly manipulated value</returns>
    object? ManipulateValue<TPropertyType>(TPropertyType? value);

    /// <summary>
    /// Checks to ensure the method can be called successfully later on
    /// </summary>
    void Validate();
}
