namespace HybridMicroOrm.Tests.TestScenarios;

public class MultiTenantScenario(IntegrationTestFixture fixture, ITestOutputHelper output)
    : BaseTest(fixture, output)
{
    protected readonly TenantContext Tenant1 = new();
    protected readonly TenantContext Tenant2 = new();
    protected readonly TenantContext Tenant3 = new();

    protected class TenantContext
    {
        public Guid TenantId = Guid.NewGuid();
        public Guid UserId1 = Guid.NewGuid();
        public Guid UserId2 = Guid.NewGuid();
        public Guid UserId3 = Guid.NewGuid();
    }

    protected async Task ExecTenant1User1(Func<IHybridMicroOrm, Task> action) => await Exec(action, Tenant1.TenantId, Tenant1.UserId1);
    protected async Task ExecTenant1User2(Func<IHybridMicroOrm, Task> action) => await Exec(action, Tenant1.TenantId, Tenant1.UserId2);
    protected async Task ExecTenant1User3(Func<IHybridMicroOrm, Task> action) => await Exec(action, Tenant1.TenantId, Tenant1.UserId3);
    protected async Task ExecTenant2User1(Func<IHybridMicroOrm, Task> action) => await Exec(action, Tenant2.TenantId, Tenant2.UserId1);
    protected async Task ExecTenant2User2(Func<IHybridMicroOrm, Task> action) => await Exec(action, Tenant2.TenantId, Tenant2.UserId2);
    protected async Task ExecTenant2User3(Func<IHybridMicroOrm, Task> action) => await Exec(action, Tenant2.TenantId, Tenant2.UserId3);
    protected async Task ExecTenant3User1(Func<IHybridMicroOrm, Task> action) => await Exec(action, Tenant3.TenantId, Tenant3.UserId1);
    protected async Task ExecTenant3User2(Func<IHybridMicroOrm, Task> action) => await Exec(action, Tenant3.TenantId, Tenant3.UserId2);
    protected async Task ExecTenant3User3(Func<IHybridMicroOrm, Task> action) => await Exec(action, Tenant3.TenantId, Tenant3.UserId3);

    protected async Task<T> ExecTenant1User1<T>(Func<IHybridMicroOrm, Task<T>> action) => await Exec(action, Tenant1.TenantId, Tenant1.UserId1);
    protected async Task<T> ExecTenant1User2<T>(Func<IHybridMicroOrm, Task<T>> action) => await Exec(action, Tenant1.TenantId, Tenant1.UserId2);
    protected async Task<T> ExecTenant1User3<T>(Func<IHybridMicroOrm, Task<T>> action) => await Exec(action, Tenant1.TenantId, Tenant1.UserId3);
    protected async Task<T> ExecTenant2User1<T>(Func<IHybridMicroOrm, Task<T>> action) => await Exec(action, Tenant2.TenantId, Tenant2.UserId1);
    protected async Task<T> ExecTenant2User2<T>(Func<IHybridMicroOrm, Task<T>> action) => await Exec(action, Tenant2.TenantId, Tenant2.UserId2);
    protected async Task<T> ExecTenant2User3<T>(Func<IHybridMicroOrm, Task<T>> action) => await Exec(action, Tenant2.TenantId, Tenant2.UserId3);
    protected async Task<T> ExecTenant3User1<T>(Func<IHybridMicroOrm, Task<T>> action) => await Exec(action, Tenant3.TenantId, Tenant3.UserId1);
    protected async Task<T> ExecTenant3User2<T>(Func<IHybridMicroOrm, Task<T>> action) => await Exec(action, Tenant3.TenantId, Tenant3.UserId2);
    protected async Task<T> ExecTenant3User3<T>(Func<IHybridMicroOrm, Task<T>> action) => await Exec(action, Tenant3.TenantId, Tenant3.UserId3);
}