using HybridMicroOrm.Tests.Contexts;

namespace HybridMicroOrm.Tests.Fixtures;

[Collection(nameof(IntegrationTestCollection))]
public class BaseTest : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture;
    protected readonly ITestOutputHelper Output;
    protected IJsonConverter JsonConverter => _fixture.Services.GetRequiredService<IJsonConverter>();

    protected BaseTest(IntegrationTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _fixture.OutputHelper = output;
        Output = output;
    }

    public async Task InitializeAsync()
    {
        await using var scope = _fixture.Services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<IHybridMicroOrmManager>();
        await manager.Drop();
        await manager.Init();
    }

    protected async Task Exec(Func<IHybridMicroOrm, Task> action, Guid? tenantId = null, Guid? userId = null)
    {
        using var scope = GetScope(tenantId, userId);
        var db = scope.ServiceProvider.GetRequiredService<IHybridMicroOrm>();
        await action(db);
    }

    protected async Task<T> Exec<T>(Func<IHybridMicroOrm, Task<T>> action, Guid? tenantId = null, Guid? userId = null)
    {
        using var scope = GetScope(tenantId, userId);
        var db = scope.ServiceProvider.GetRequiredService<IHybridMicroOrm>();
        return await action(db);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected IServiceScope GetScope(Guid? tenantId = null, Guid? userId = null)
    {
        var scope = _fixture.Services.CreateScope();
        (scope.ServiceProvider.GetRequiredService<ITenantContext>() as TestTenantContext)!.TenantId = tenantId;
        (scope.ServiceProvider.GetRequiredService<IUserContext>() as TestUserContext)!.UserId = userId;
        return scope;
    }
}