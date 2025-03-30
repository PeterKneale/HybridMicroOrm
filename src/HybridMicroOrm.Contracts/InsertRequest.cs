using Newtonsoft.Json;

namespace HybridMicroOrm.Contracts;

public record InsertRequest
{
    private InsertRequest(Guid id, string type, string data, bool isTenantData)
    {
        Id = id;
        Type = type;
        Data = data;
        IsTenantData = isTenantData;
    }

    public Guid Id { get; }
    public string Type { get; }
    public string Data { get; }
    public bool IsTenantData { get; }

    public static InsertRequest Create(Guid id, string type, object data, bool isTenantData = true)
    {
        var jsonData = JsonConvert.SerializeObject(data);
        return new InsertRequest(id, type, jsonData, isTenantData);
    }
}