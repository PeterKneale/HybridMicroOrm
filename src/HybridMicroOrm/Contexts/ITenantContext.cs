namespace HybridMicroOrm.Contexts;

public interface ITenantContext
{
    Guid? TenantId { get; }
}