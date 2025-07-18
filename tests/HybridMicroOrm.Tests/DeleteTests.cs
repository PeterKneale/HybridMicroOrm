namespace HybridMicroOrm.Tests;

public class DeleteTests(IntegrationTestFixture fixture, ITestOutputHelper output) : BaseTest(fixture, output)
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _customerId = Guid.NewGuid();

    [Fact]
    public async Task Test_tenant_record_is_deleted()
    {
        await Exec(x => x.Insert(CreateInsertRequest()), _tenantId, _userId);

        // assert
        await Exec(x => x.Delete(_customerId), _tenantId, _userId);

        var record1 = await Exec(x => x.Get(new GetRequest(_customerId) { IncludeDeleted = false }), _tenantId, _userId);
        record1.ShouldBeNull();
        var record2 = await Exec(x => x.Get(new GetRequest(_customerId) { IncludeDeleted = true }), _tenantId, _userId);
        record2.ShouldBeNull();
    }

    [Fact]
    public async Task Test_tenant_record_is_soft_deleted()
    {
        await Exec(x => x.Insert(CreateInsertRequest()), _tenantId, _userId);

        // assert
        await Exec(x => x.SoftDelete(_customerId), _tenantId, _userId);

        var record1 = await Exec(x => x.Get(new GetRequest(_customerId) { IncludeDeleted = false }), _tenantId, _userId);
        record1.ShouldBeNull();
        var record2 = await Exec(x => x.Get(new GetRequest(_customerId) { IncludeDeleted = true }), _tenantId, _userId);
        record2.ShouldNotBeNull();
        record2.Id.ShouldBe(_customerId);
        record2.TenantId.ShouldBe(_tenantId);
        record2.CreatedBy.ShouldBe(_userId);
        record2.DeletedBy.ShouldBe(_userId);
    }

    [Fact]
    public async Task Test_global_record_is_soft_deleted()
    {
        await Exec(x => x.Insert(CreateInsertRequest(isTenantData: false)));

        // assert
        await Exec(x => x.SoftDelete(_customerId));

        var record1 = await Exec(x => x.Get(new GetRequest(_customerId) { IncludeDeleted = false }));
        record1.ShouldBeNull();
        var record2 = await Exec(x => x.Get(new GetRequest(_customerId) { IncludeDeleted = true }));
        record2.ShouldNotBeNull();
        record2.Id.ShouldBe(_customerId);
        record2.DeletedAt.ShouldNotBeNull();
        record2.DeletedBy.ShouldBeNull();
    }

    [Fact]
    public async Task Test_tenant_record_is_soft_deleted_if_tenant_context_is_available()
    {
        await Exec(x => x.Insert(CreateInsertRequest()), _tenantId, _userId);

        // assert
        await Exec(x => x.SoftDelete(_customerId), _tenantId, _userId);

        var record = await Exec(x => x.Get(new GetRequest(_customerId)), _tenantId, _userId);
        record.ShouldBeNull();
    }

    [Fact]
    public async Task Test_tenant_record_is_not_soft_deleted_if_no_tenant_context_is_available()
    {
        await Exec(x => x.Insert(CreateInsertRequest()), _tenantId, _userId);

        // assert
        await Exec(x => x.SoftDelete(_customerId));

        var record = await Exec(x => x.Get(new GetRequest(_customerId)), _tenantId, _userId);
        record.ShouldNotBeNull();
        record.Id.ShouldBe(_customerId);
        record.TenantId.ShouldBe(_tenantId);
        record.CreatedBy.ShouldBe(_userId);
        record.DeletedBy.ShouldBeNull();
    }

    private InsertRequest CreateInsertRequest(bool isTenantData = true) =>
        InsertRequest.Create(_customerId, "customer", new User
        {
            Id = _customerId, Name = "John Doe", Email = "user@example.com"
        }, JsonConverter, isTenantData);
}