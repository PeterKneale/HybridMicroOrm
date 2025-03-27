using Newtonsoft.Json;

namespace HybridMicroOrm;

public class UpdateRequest
{
    private UpdateRequest(Guid id, string type, string data)
    {
        Id = id;
        Type = type;
        Data = data;
    }

    public Guid Id { get; }
    public string Type { get; }
    public string Data { get; }

    public static UpdateRequest Create(Guid id, string type, object data)
    {
        return new UpdateRequest(id, type, JsonConvert.SerializeObject(data));
    }
}