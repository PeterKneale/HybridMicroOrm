namespace HybridMicroOrm.Tests;

public class FilterTests(IntegrationTestFixture fixture, ITestOutputHelper output) : MultiUserScenario(fixture, output)
{
    [Fact]
    public async Task Test_record_is_returned_when_type_matches()
    {
        // arrange
        var id1 = await PerformInsert("Toyota", "Corolla");
        var id2 = await PerformInsert("Toyota", "Celica");
        var id3 = await PerformInsert("Mazda", "3");

        // act & assert
        var toyotas = await ListWithFilter(new Filter("data->>'Make' = @Make", new { Make = "Toyota" }));
        toyotas.Select(x => x.Id).ShouldBe([id1, id2], ignoreOrder: false);

        var celicas = await ListWithFilter(new Filter("data->>'Make' = @Make AND data->>'Model' = @Model", new { Make = "Toyota", Model = "Celica" }));
        celicas.Select(x => x.Id).ShouldBe([id2], ignoreOrder: false);

        var mazdas = await ListWithFilter(new Filter("data->>'Make' = @Make", new { Make = "Mazda" }));
        mazdas.Select(x => x.Id).ShouldBe([id3], ignoreOrder: false);
    }

    private async Task<IEnumerable<Record>> ListWithFilter(Filter filter) =>
        await ExecUser1(x => x.List(new ListRequest(Car.Type)
        {
            Filter = filter
        }));

    private async Task<Guid> PerformInsert(string make, string model)
    {
        var id = Guid.NewGuid();
        await ExecUser1(x => x.Insert(InsertRequest.Create(id, Car.Type, new Car
        {
            Id = id, Make = make, Model = model
        })));
        return id;
    }
}