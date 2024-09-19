namespace TopMarksDevelopment.ExpressionBuilder.Serialization;

/// <summary>
/// Defines methods for packing and unpacking data to and from byte arrays.
/// </summary>
public interface IProtoConverter
{
    /// <summary>
    /// Packs the data into a byte array.
    /// </summary>
    /// <returns>A byte array containing the packed data.</returns>
    byte[] Pack();

    /// <summary>
    /// Unpacks the data from a byte array.
    /// </summary>
    /// <param name="bytes">The byte array containing the data to unpack.</param>
    /// <returns>An object representing the unpacked data.</returns>
    object UnPack(byte[] bytes);
}
