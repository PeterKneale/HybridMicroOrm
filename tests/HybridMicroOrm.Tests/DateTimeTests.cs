namespace HybridMicroOrm.Tests;

public class DateTimeTests(IntegrationTestFixture fixture, ITestOutputHelper output) : MultiTenantScenario(fixture, output)
{
    [Fact]
    public async Task Test_inserted_record()
    {   
        // arrange
        var customerId = Guid.NewGuid();
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId)));

        // act
        var record = await ExecTenant1User1(x => x.Get(customerId));
        Output.WriteLine(JsonConvert.SerializeObject(record,Formatting.Indented));

        // assert
        record.ShouldNotBeNull();
        record.CreatedAt.ShouldBe(DateTime.UtcNow,TimeSpan.FromSeconds(5));
        record.UpdatedAt.ShouldBeNull();
    }
    
    [Fact]
    public async Task Test_updated_record()
    {   
        // arrange
        var customerId = Guid.NewGuid();
        await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId)));
        await ExecTenant1User1(x => x.Update(CreateUpdateRequest(customerId)));

        // act
        var record = await ExecTenant1User1(x => x.Get(customerId));
        Output.WriteLine(JsonConvert.SerializeObject(record,Formatting.Indented));

        // assert
        record.ShouldNotBeNull();
        record.CreatedAt.ShouldBe(DateTime.UtcNow,TimeSpan.FromSeconds(5));
        record.UpdatedAt.ShouldNotBeNull();
        record.UpdatedAt.Value.ShouldBe(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    private InsertRequest CreateInsertRequest(Guid customerId, bool isTenantData = true) =>
        InsertRequest.Create(customerId, "customer", new User
        {
            Id = customerId, Name = "John Doe", Email = "user@example.com"
        }, isTenantData);
    
    private UpdateRequest CreateUpdateRequest(Guid customerId) =>
        UpdateRequest.Create(customerId, "customer", new User
        {
            Id = customerId, Name = "John Doe", Email = "user@example.com"
        });
}