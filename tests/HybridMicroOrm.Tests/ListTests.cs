namespace HybridMicroOrm.Tests;

public class ListTests(IntegrationTestFixture fixture, ITestOutputHelper output) : MultiUserScenario(fixture, output)
{

    [Fact]
    public async Task Test_record_is_returned_when_type_matches()
    {
        // arrange
        await ExecUser1(x => x.Insert(CreateInsertCarRequest(Guid.NewGuid())));
        await ExecUser1(x => x.Insert(CreateInsertCustomerRequest(Guid.NewGuid())));

        // act
        var cars = await Exec(x => x.List(new ListRequest(Car.Type)));
        var customers = await Exec(x => x.List(new ListRequest(User.Type)));

        // assert
        cars.ShouldNotBeNull();
        cars.Count().ShouldBe(1);
        foreach (var result in cars)
            result.ShouldNotBeNull();
        customers.ShouldNotBeNull();
        customers.Count().ShouldBe(1);
        foreach (var result in customers)
            result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Test_no_record_is_returned_when_type_does_not_match()
    {
        await ExecUser1(x => x.Insert(CreateInsertCustomerRequest(Guid.NewGuid())));

        // assert
        var results = await ExecUser1(x => x.List(new ListRequest("xxxxxxxxxxxxx")));

        results.ShouldNotBeNull();
        results.Count().ShouldBe(0);
    }

    [Fact]
    public async Task Test_no_record_is_returned_when_soft_delete_is_performed_and_include_deleted_is_not_specified()
    {
        var customerId  = Guid.NewGuid();
        await ExecUser1(x => x.Insert(CreateInsertCustomerRequest(customerId)));
        await ExecUser1(x => x.SoftDelete(customerId));

        // assert
        var results = await Exec(x => x.List(new ListRequest(User.Type)));

        results.ShouldNotBeNull();
        results.Count().ShouldBe(0);
    }

    [Fact]
    public async Task Test_record_is_returned_when_soft_delete_is_performed_and_include_deleted_is_true()
    {
        var customerId  = Guid.NewGuid();
        await Exec(x => x.Insert(CreateInsertCustomerRequest(customerId)));
        await Exec(x => x.SoftDelete(customerId));

        // assert
        var results = await Exec(x => x.List(new ListRequest(User.Type) { IncludeDeleted = true }));

        results.ShouldNotBeNull();
        results.Count().ShouldBe(1);
        var result = results.Single();
        result.ShouldNotBeNull();
        result.Id.ShouldBe(customerId);
    }

    private InsertRequest CreateInsertCustomerRequest(Guid customerId) =>
        InsertRequest.Create(customerId, User.Type, new User
        {
            Id = customerId, Name = "John Doe", Email = "user@example.com"
        });

    private InsertRequest CreateInsertCarRequest(Guid carId) =>
        InsertRequest.Create(carId, Car.Type, new Car
        {
            Id = carId, Make = "Toyota", Model = "Corolla"
        });
}