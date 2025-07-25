namespace HybridMicroOrm.Contracts;

public class Record<T>
{
    public Guid Id { get; init; }
    public Guid? TenantId { get; init; }
    
    public string Type { get; set; } = "";
    public T Data { get; set; } = default!;
    
    public DateTime CreatedAt { get; init; }
    public Guid? CreatedBy { get; init; }
    
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}