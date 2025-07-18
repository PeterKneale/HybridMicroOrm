using HybridMicroOrm.Contracts;
using Newtonsoft.Json;

namespace HybridMicroOrm.Tests.Fixtures;

/// <summary>
/// Implementation of IJsonConverter using Newtonsoft.Json for testing purposes.
/// This demonstrates how to implement the IJsonConverter interface with a specific JSON serializer.
/// </summary>
public class NewtonsoftJsonConverter : IJsonConverter
{
    public string Serialize(object value)
    {
        return JsonConvert.SerializeObject(value);
    }

    public T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json)!;
    }
}