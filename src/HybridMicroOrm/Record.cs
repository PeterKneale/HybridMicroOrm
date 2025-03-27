using Newtonsoft.Json;

namespace HybridMicroOrm;

public class Record
{
    public Guid Id { get; init; }
    public string Type { get; set; } = "";
    public Guid? TenantId { get; init; }
    public string Data { get; set; } = "";
    public DateTime CreatedAt { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    public T Get<T>()
    {
        return JsonConvert.DeserializeObject<T>(Data)!;
    }
}