namespace HybridMicroOrm.Contracts;

public interface ITenantContext
{
    Guid? TenantId { get; }
}