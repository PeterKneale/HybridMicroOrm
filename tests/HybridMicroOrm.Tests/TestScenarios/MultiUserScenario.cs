using HybridMicroOrm.Contracts;

namespace HybridMicroOrm.Tests.TestScenarios;

public class MultiUserScenario(IntegrationTestFixture fixture, ITestOutputHelper output)
    : BaseTest(fixture, output)
{
    protected readonly Guid UserId1 = Guid.NewGuid();
    protected readonly Guid UserId2 = Guid.NewGuid();
    protected readonly Guid UserId3 = Guid.NewGuid();

    protected async Task ExecUser1(Func<IHybridMicroOrm, Task> action) => await Exec(action, null, UserId1);
    protected async Task ExecUser2(Func<IHybridMicroOrm, Task> action) => await Exec(action, null, UserId2);
    protected async Task ExecUser3(Func<IHybridMicroOrm, Task> action) => await Exec(action, null, UserId3);
    
    protected async Task<T> ExecUser1<T>(Func<IHybridMicroOrm, Task<T>> action) => await Exec(action, null, UserId1);
    protected async Task<T> ExecUser2<T>(Func<IHybridMicroOrm, Task<T>> action) => await Exec(action, null, UserId2);
    protected async Task<T> ExecUser3<T>(Func<IHybridMicroOrm, Task<T>> action) => await Exec(action, null, UserId3);
}