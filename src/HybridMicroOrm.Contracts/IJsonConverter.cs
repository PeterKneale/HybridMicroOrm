namespace HybridMicroOrm.Contracts;

/// <summary>
/// Provides JSON serialization and deserialization capabilities for the HybridMicroOrm.
/// Implement this interface with your preferred JSON serializer (e.g., Newtonsoft.Json, System.Text.Json).
/// </summary>
public interface IJsonConverter
{
    /// <summary>
    /// Serializes an object to a JSON string.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    string Serialize(object value);

    /// <summary>
    /// Deserializes a JSON string to an object of type T.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An object of type T.</returns>
    T Deserialize<T>(string json);
}