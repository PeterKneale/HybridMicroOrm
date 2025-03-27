namespace HybridMicroOrm.Tests;

public class ListSortTests(IntegrationTestFixture fixture, ITestOutputHelper output) : MultiUserScenario(fixture, output)
{
    [Fact]
    public async Task Ordered_by_create()
    {
        // arrange
        var id1 = await PerformInsert();
        var id2 = await PerformInsert();
        var id3 = await PerformInsert();

        // act
        var createdAsc = await Exec(x => x.List(new ListRequest(Car.Type)));
        var createdDesc = await Exec(x => x.List(new ListRequest(Car.Type) { SortOrder = SortOrder.Descending }));

        // assert
        createdAsc.Select(x => x.Id).ShouldBe([id1, id2, id3], ignoreOrder: false);
        createdDesc.Select(x => x.Id).ShouldBe([id3, id2, id1], ignoreOrder: false);
    }
    
    [Fact]
    public async Task Ordered_by_deleted()
    {
        // arrange
        var id1 = await PerformInsert();
        var id2 = await PerformInsert();
        var id3 = await PerformInsert();
        await Exec(x => x.SoftDelete(id3));
        await Exec(x => x.SoftDelete(id2));
        await Exec(x => x.SoftDelete(id1));

        // act
        var deletedAsc = await Exec(x => x.List(new ListRequest(Car.Type) { SortBy = SortBy.Deleted, SortOrder = SortOrder.Ascending, IncludeDeleted = true}));
        var deletedDesc = await Exec(x => x.List(new ListRequest(Car.Type) { SortBy = SortBy.Deleted, SortOrder = SortOrder.Descending, IncludeDeleted = true }));

        // assert
        deletedAsc.Select(x => x.Id).ShouldBe([id3, id2, id1], ignoreOrder: false);
        deletedDesc.Select(x => x.Id).ShouldBe([id1, id2, id3], ignoreOrder: false);
    }

    private async Task<Guid> PerformInsert()
    {
        var id = Guid.NewGuid();
        await ExecUser1(x => x.Insert(InsertRequest.Create(id, Car.Type, new Car
        {
            Id = id, Make = "Toyota", Model = "Corolla"
        })));
        return id;
    }
}