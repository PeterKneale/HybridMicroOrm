using HybridMicroOrm.Contracts;

namespace HybridMicroOrm.Tests;

public class GetTests(IntegrationTestFixture fixture, ITestOutputHelper output) : MultiTenantScenario(fixture, output)
{
    [Fact]
    public async Task Test_tenant_record_can_be_read_with_tenant_context()
    {
        // arrange
        var customerId = Guid.NewGuid();
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId)));

        // act
        var record = await ExecTenant1User1(x => x.Get(customerId));

        // assert
        record.ShouldNotBeNull();
        record.Id.ShouldBe(customerId);
        record.TenantId.ShouldBe(Tenant1.TenantId);
        record.CreatedBy.ShouldBe(Tenant1.UserId1);
    }

    [Fact]
    public async Task Test_tenant_record_can_NOT_be_read_without_tenant_context()
    {
        // arrange
        var customerId = Guid.NewGuid();

        // act
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId)));

        // assert
        var record = await Exec(x => x.Get(customerId));
        record.ShouldBeNull();
    }

    [Fact]
    public async Task Test_tenant_record_can_NOT_be_read_with_different_tenant_context()
    {
        // arrange
        var customerId = Guid.NewGuid();

        // act
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId)));

        // assert
        var record = await ExecTenant2User1(x => x.Get(customerId));
        record.ShouldBeNull();
    }

    [Fact]
    public async Task Test_global_record_can_be_read_with_tenant_context()
    {
        // arrange
        var customerId = Guid.NewGuid();

        // act
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId, isTenantData: false)));

        // assert
        var record = await ExecTenant1User1(x => x.Get(customerId));
        record.ShouldNotBeNull();
        record.Id.ShouldBe(customerId);
        record.TenantId.ShouldBeNull();
        record.CreatedBy.ShouldBe(Tenant1.UserId1);
    }

    [Fact]
    public async Task Test_global_record_can_be_read_without_tenant_context()
    {
        // arrange
        var customerId = Guid.NewGuid();

        // act
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId, isTenantData: false)));

        // assert
        var record = await Exec(x => x.Get(customerId));
        record.ShouldNotBeNull();
        record.Id.ShouldBe(customerId);
        record.TenantId.ShouldBeNull();
        record.CreatedBy.ShouldBe(Tenant1.UserId1);
    }

    [Fact]
    public async Task Test_global_record_inserted_without_context_can_be_read_without_tenant_context()
    {
        // arrange
        var customerId = Guid.NewGuid();

        // act
        await Exec(x => x.Insert(CreateInsertRequest(customerId, isTenantData: false)));

        // assert
        var record = await Exec(x => x.Get(customerId));
        record.ShouldNotBeNull();
        record.Id.ShouldBe(customerId);
        record.TenantId.ShouldBeNull();
        record.CreatedBy.ShouldBeNull();
    }

    [Fact]
    public async Task Test_global_record_inserted_without_context_can_be_read_with_tenant_context()
    {
        // arrange
        var customerId = Guid.NewGuid();

        // act
        await Exec(x => x.Insert(CreateInsertRequest(customerId, isTenantData: false)));

        // assert
        var record = await ExecTenant1User1(x => x.Get(customerId));
        record.ShouldNotBeNull();
        record.Id.ShouldBe(customerId);
        record.TenantId.ShouldBeNull();
        record.CreatedBy.ShouldBeNull();
    }

    private InsertRequest CreateInsertRequest(Guid customerId, bool isTenantData = true) =>
        InsertRequest.Create(customerId, "customer", new User
        {
            Id = customerId, Name = "John Doe", Email = "user@example.com"
        }, isTenantData);
}