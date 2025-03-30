using HybridMicroOrm.Contracts;

namespace HybridMicroOrm.Tests.Contexts;

public class TestTenantContext : ITenantContext
{
    public Guid? TenantId { get; set; }
}