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
        var record = await ExecTenant1User1(x => x.Get<User>(customerId));

        // assert
        record.ShouldNotBeNull();
        record.Id.ShouldBe(customerId);
        record.TenantId.ShouldBe(Tenant1.TenantId);
        record.CreatedBy.ShouldBe(Tenant1.UserId1);
        record.Data.ShouldNotBeNull();
        record.Data.Id.ShouldBe(customerId);
        record.Data.Name.ShouldBe("John Doe");
        record.Data.Email.ShouldBe("user@example.com");
    }

    [Fact]
    public async Task Test_tenant_record_can_NOT_be_read_without_tenant_context()
    {
        // arrange
        var customerId = Guid.NewGuid();

        // act
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId)));

        // assert
        var record = await Exec(x => x.Get<User>(customerId));
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
        var record = await ExecTenant2User1(x => x.Get<User>(customerId));
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
        var record = await ExecTenant1User1(x => x.Get<User>(customerId));
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
        var record = await Exec(x => x.Get<User>(customerId));
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
        var record = await Exec(x => x.Get<User>(customerId));
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
        var record = await ExecTenant1User1(x => x.Get<User>(customerId));
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

    private InsertRequest CreateInsertCustomerRequest(Guid customerId) =>
        InsertRequest.Create(customerId, User.Type, new User
        {
            Id = customerId, Name = "John Doe", Email = "user@example.com"
        });

    [Fact]
    public async Task Test_get_with_valid_guid_string_returns_record()
    {
        // arrange
        var customerId = Guid.NewGuid();
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId)));

        // act
        var record = await ExecTenant1User1(x => x.Get<User>(customerId.ToString()));

        // assert
        record.ShouldNotBeNull();
        record.Id.ShouldBe(customerId);
        record.TenantId.ShouldBe(Tenant1.TenantId);
        record.CreatedBy.ShouldBe(Tenant1.UserId1);
    }

    [Fact]
    public async Task Test_get_with_valid_guid_string_for_nonexistent_record_returns_null()
    {
        // arrange
        var nonExistentId = Guid.NewGuid();

        // act
        var record = await ExecTenant1User1(x => x.Get<User>(nonExistentId.ToString()));

        // assert
        record.ShouldBeNull();
    }

    [Fact]
    public async Task Test_get_with_invalid_guid_string_throws_argument_exception()
    {
        // arrange
        const string invalidGuid = "not-a-valid-guid";

        // act & assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await ExecTenant1User1(x => x.Get<User>(invalidGuid)));
        
        exception.ParamName.ShouldBe("id");
        exception.Message.ShouldContain("not a valid GUID format");
    }

    [Fact]
    public async Task Test_get_with_null_string_throws_argument_exception()
    {
        // act & assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await ExecTenant1User1(x => x.Get<User>((string)null!)));
        
        exception.ParamName.ShouldBe("id");
        exception.Message.ShouldContain("cannot be null, empty, or whitespace");
    }

    [Fact]
    public async Task Test_get_with_empty_string_throws_argument_exception()
    {
        // act & assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await ExecTenant1User1(x => x.Get<User>("")));
        
        exception.ParamName.ShouldBe("id");
        exception.Message.ShouldContain("cannot be null, empty, or whitespace");
    }

    [Fact]
    public async Task Test_get_with_whitespace_string_throws_argument_exception()
    {
        // act & assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await ExecTenant1User1(x => x.Get<User>("   ")));
        
        exception.ParamName.ShouldBe("id");
        exception.Message.ShouldContain("cannot be null, empty, or whitespace");
    }

    [Fact]
    public async Task Test_methods_accept_cancellation_tokens()
    {
        // arrange
        var customerId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        
        // Test that all methods accept cancellation tokens without compilation errors
        await ExecTenant1User1(async x => 
        {
            await x.Insert(CreateInsertCustomerRequest(customerId), cancellationToken);
            var record = await x.Get<User>(customerId, cancellationToken);
            record.ShouldNotBeNull();
            
            var records = await x.List<User>(new ListRequest(User.Type), cancellationToken);
            records.ShouldNotBeEmpty();
            
            var updateRequest = UpdateRequest.Create(customerId, User.Type, new User
            {
                Id = customerId, Name = "Updated Name", Email = "updated@example.com"
            });
            await x.Update(updateRequest, cancellationToken);
            await x.SoftDelete(customerId, cancellationToken);
        });
    }
}