namespace HybridMicroOrm.Tests;

public class ExistsTests(IntegrationTestFixture fixture, ITestOutputHelper output) : MultiTenantScenario(fixture, output)
{
    [Fact]
    public async Task Test_exists_async_returns_true_when_tenant_record_exists_with_correct_tenant_context()
    {
        // arrange
        var customerId = Guid.NewGuid();
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId)));

        // act
        var exists = await ExecTenant1User1(x => x.ExistsAsync(customerId));

        // assert
        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task Test_exists_returns_true_when_tenant_record_exists_with_correct_tenant_context()
    {
        // arrange
        var customerId = Guid.NewGuid();
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId)));

        // act
        var exists = await ExecTenant1User1(x => Task.FromResult(x.Exists(customerId)));

        // assert
        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task Test_exists_async_returns_false_when_tenant_record_does_not_exist()
    {
        // arrange
        var nonExistentId = Guid.NewGuid();

        // act
        var exists = await ExecTenant1User1(x => x.ExistsAsync(nonExistentId));

        // assert
        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task Test_exists_returns_false_when_tenant_record_does_not_exist()
    {
        // arrange
        var nonExistentId = Guid.NewGuid();

        // act
        var exists = await ExecTenant1User1(x => Task.FromResult(x.Exists(nonExistentId)));

        // assert
        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task Test_exists_async_returns_false_when_tenant_record_exists_but_wrong_tenant_context()
    {
        // arrange
        var customerId = Guid.NewGuid();
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId)));

        // act
        var exists = await ExecTenant2User1(x => x.ExistsAsync(customerId));

        // assert
        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task Test_exists_returns_false_when_tenant_record_exists_but_wrong_tenant_context()
    {
        // arrange
        var customerId = Guid.NewGuid();
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId)));

        // act
        var exists = await ExecTenant2User1(x => Task.FromResult(x.Exists(customerId)));

        // assert
        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task Test_exists_async_returns_false_when_tenant_record_exists_but_no_tenant_context()
    {
        // arrange
        var customerId = Guid.NewGuid();
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId)));

        // act
        var exists = await Exec(x => x.ExistsAsync(customerId));

        // assert
        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task Test_exists_returns_false_when_tenant_record_exists_but_no_tenant_context()
    {
        // arrange
        var customerId = Guid.NewGuid();
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId)));

        // act
        var exists = await Exec(x => Task.FromResult(x.Exists(customerId)));

        // assert
        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task Test_exists_async_returns_true_when_global_record_exists_with_tenant_context()
    {
        // arrange
        var customerId = Guid.NewGuid();
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId, isTenantData: false)));

        // act
        var exists = await ExecTenant1User1(x => x.ExistsAsync(customerId));

        // assert
        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task Test_exists_returns_true_when_global_record_exists_with_tenant_context()
    {
        // arrange
        var customerId = Guid.NewGuid();
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId, isTenantData: false)));

        // act
        var exists = await ExecTenant1User1(x => Task.FromResult(x.Exists(customerId)));

        // assert
        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task Test_exists_async_returns_true_when_global_record_exists_without_tenant_context()
    {
        // arrange
        var customerId = Guid.NewGuid();
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId, isTenantData: false)));

        // act
        var exists = await Exec(x => x.ExistsAsync(customerId));

        // assert
        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task Test_exists_returns_true_when_global_record_exists_without_tenant_context()
    {
        // arrange
        var customerId = Guid.NewGuid();
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId, isTenantData: false)));

        // act
        var exists = await Exec(x => Task.FromResult(x.Exists(customerId)));

        // assert
        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task Test_exists_async_returns_false_when_record_is_soft_deleted()
    {
        // arrange
        var customerId = Guid.NewGuid();
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId)));
        await ExecTenant1User1(x => x.SoftDelete(customerId));

        // act
        var exists = await ExecTenant1User1(x => x.ExistsAsync(customerId));

        // assert
        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task Test_exists_returns_false_when_record_is_soft_deleted()
    {
        // arrange
        var customerId = Guid.NewGuid();
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId)));
        await ExecTenant1User1(x => x.SoftDelete(customerId));

        // act
        var exists = await ExecTenant1User1(x => Task.FromResult(x.Exists(customerId)));

        // assert
        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task Test_exists_async_returns_false_when_record_is_hard_deleted()
    {
        // arrange
        var customerId = Guid.NewGuid();
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId)));
        await ExecTenant1User1(x => x.Delete(customerId));

        // act
        var exists = await ExecTenant1User1(x => x.ExistsAsync(customerId));

        // assert
        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task Test_exists_returns_false_when_record_is_hard_deleted()
    {
        // arrange
        var customerId = Guid.NewGuid();
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId)));
        await ExecTenant1User1(x => x.Delete(customerId));

        // act
        var exists = await ExecTenant1User1(x => Task.FromResult(x.Exists(customerId)));

        // assert
        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task Test_exists_async_accepts_cancellation_token()
    {
        // arrange
        var customerId = Guid.NewGuid();
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId)));
        var cancellationToken = CancellationToken.None;

        // act & assert - should not throw
        var exists = await ExecTenant1User1(x => x.ExistsAsync(customerId, cancellationToken));
        exists.ShouldBeTrue();
    }

    private InsertRequest CreateInsertRequest(Guid customerId, bool isTenantData = true) =>
        InsertRequest.Create(customerId, "customer", new User
        {
            Id = customerId, Name = "John Doe", Email = "user@example.com"
        }, isTenantData);
}