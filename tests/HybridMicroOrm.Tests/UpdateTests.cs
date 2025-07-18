using HybridMicroOrm.Contracts;

namespace HybridMicroOrm.Tests;

public class UpdateTests(IntegrationTestFixture fixture, ITestOutputHelper output) : MultiTenantScenario(fixture, output)
{
    private readonly Guid _customerId = Guid.NewGuid();

    [Fact]
    public async Task Test_tenant_record_can_be_updated_with_tenant_context()
    {
        // arrange
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest()));

        // act
        await ExecTenant1User1(x => x.Update(CreateUpdateRequest()));

        // assert
        var record = await ExecTenant1User1(x => x.Get(_customerId));
        record.ShouldNotBeNull();
        record.Id.ShouldBe(_customerId);
        record.TenantId.ShouldBe(Tenant1.TenantId);
        record.CreatedBy.ShouldBe(Tenant1.UserId1);
        record.UpdatedBy.ShouldBe(Tenant1.UserId1);
        record.UpdatedAt.ShouldNotBeNull();
    }
    
    [Fact]
    public async Task Test_tenant_record_is_not_updated_without_tenant_context()
    {
        // arrange
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest()));

        // act
        await Exec(x => x.Update(CreateUpdateRequest()));

        // assert
        var record = await ExecTenant1User1(x => x.Get(_customerId));
        record.ShouldNotBeNull();
        record.Id.ShouldBe(_customerId);
        record.TenantId.ShouldBe(Tenant1.TenantId);
        record.CreatedBy.ShouldBe(Tenant1.UserId1);
        record.UpdatedBy.ShouldBeNull();
        record.UpdatedAt.ShouldBeNull();
    }

    private InsertRequest CreateInsertRequest() =>
        InsertRequest.Create(_customerId, "customer", new User
        {
            Id = _customerId, Name = "John Doe", Email = "user@example.com"
        }, JsonConverter);

    private UpdateRequest CreateUpdateRequest() =>
        UpdateRequest.Create(_customerId, "customer", new User
        {
            Id = _customerId, Name = "John Doe2", Email = "user2@example.com"
        }, JsonConverter);
}