using HybridMicroOrm.Contracts;

namespace HybridMicroOrm.Tests;

public class ListPagedTests(IntegrationTestFixture fixture, ITestOutputHelper output) : MultiUserScenario(fixture, output)
{
    [Fact]
    public async Task Test_paged_list_returns_correct_first_page()
    {
        // arrange - create 15 records
        var carIds = new List<Guid>();
        for (int i = 0; i < 15; i++)
        {
            var carId = Guid.NewGuid();
            carIds.Add(carId);
            await ExecUser1(x => x.Insert(CreateInsertCarRequest(carId, $"Car {i:D2}")));
        }

        // act
        var request = new ListRequest(Car.Type) { PageNumber = 1, PageSize = 10 };
        var result = await Exec(x => x.ListPaged<Car>(request));

        // assert
        result.ShouldNotBeNull();
        result.Records.Count().ShouldBe(10);
        result.TotalCount.ShouldBe(15);
        result.PageNumber.ShouldBe(1);
        result.PageSize.ShouldBe(10);
        result.TotalPages.ShouldBe(2);
        result.IsFirstPage.ShouldBeTrue();
        result.IsLastPage.ShouldBeFalse();
        result.StartIndex.ShouldBe(1);
        result.EndIndex.ShouldBe(10);
    }

    [Fact]
    public async Task Test_paged_list_returns_correct_second_page()
    {
        // arrange - create 15 records
        var carIds = new List<Guid>();
        for (int i = 0; i < 15; i++)
        {
            var carId = Guid.NewGuid();
            carIds.Add(carId);
            await ExecUser1(x => x.Insert(CreateInsertCarRequest(carId, $"Car {i:D2}")));
        }

        // act
        var request = new ListRequest(Car.Type) { PageNumber = 2, PageSize = 10 };
        var result = await Exec(x => x.ListPaged<Car>(request));

        // assert
        result.ShouldNotBeNull();
        result.Records.Count().ShouldBe(5);
        result.TotalCount.ShouldBe(15);
        result.PageNumber.ShouldBe(2);
        result.PageSize.ShouldBe(10);
        result.TotalPages.ShouldBe(2);
        result.IsFirstPage.ShouldBeFalse();
        result.IsLastPage.ShouldBeTrue();
        result.StartIndex.ShouldBe(11);
        result.EndIndex.ShouldBe(15);
    }

    [Fact]
    public async Task Test_paged_list_with_default_page_size()
    {
        // arrange - create 25 records
        for (int i = 0; i < 25; i++)
        {
            var carId = Guid.NewGuid();
            await ExecUser1(x => x.Insert(CreateInsertCarRequest(carId, $"Car {i:D2}")));
        }

        // act - using default page size (10)
        var request = new ListRequest(Car.Type);
        var result = await Exec(x => x.ListPaged<Car>(request));

        // assert
        result.ShouldNotBeNull();
        result.Records.Count().ShouldBe(10);
        result.TotalCount.ShouldBe(25);
        result.PageNumber.ShouldBe(1);
        result.PageSize.ShouldBe(10);
        result.TotalPages.ShouldBe(3);
    }

    [Fact]
    public async Task Test_paged_list_respects_filters()
    {
        // arrange - create records with different makes
        for (int i = 0; i < 10; i++)
        {
            var carId = Guid.NewGuid();
            var make = i % 2 == 0 ? "Toyota" : "Honda";
            await ExecUser1(x => x.Insert(CreateInsertCarRequest(carId, $"Car {i:D2}", make)));
        }

        // act - filter for Toyota only
        var request = new ListRequest(Car.Type)
        {
            Filter = new Filter("data->>'Make' = @Make", new { Make = "Toyota" }),
            PageSize = 3
        };
        var result = await Exec(x => x.ListPaged<Car>(request));

        // assert
        result.ShouldNotBeNull();
        result.Records.Count().ShouldBe(3);
        result.TotalCount.ShouldBe(5); // 5 Toyota cars total
        result.TotalPages.ShouldBe(2);
        foreach (var record in result.Records)
        {
            record.Data.Make.ShouldBe("Toyota");
        }
    }

    [Fact]
    public async Task Test_paged_list_with_no_records()
    {
        // act
        var request = new ListRequest("NonExistentType") { PageSize = 10 };
        var result = await Exec(x => x.ListPaged<Car>(request));

        // assert
        result.ShouldNotBeNull();
        result.Records.Count().ShouldBe(0);
        result.TotalCount.ShouldBe(0);
        result.PageNumber.ShouldBe(1);
        result.PageSize.ShouldBe(10);
        result.TotalPages.ShouldBe(0);
        result.IsFirstPage.ShouldBeTrue();
        result.IsLastPage.ShouldBeTrue();
        result.StartIndex.ShouldBe(1);
        result.EndIndex.ShouldBe(0);
    }

    [Fact]
    public async Task Test_paged_list_excludes_soft_deleted_records()
    {
        // arrange
        var carIds = new List<Guid>();
        for (int i = 0; i < 5; i++)
        {
            var carId = Guid.NewGuid();
            carIds.Add(carId);
            await ExecUser1(x => x.Insert(CreateInsertCarRequest(carId, $"Car {i:D2}")));
        }

        // soft delete two records
        await ExecUser1(x => x.SoftDelete(carIds[0]));
        await ExecUser1(x => x.SoftDelete(carIds[1]));

        // act
        var request = new ListRequest(Car.Type) { PageSize = 10 };
        var result = await Exec(x => x.ListPaged<Car>(request));

        // assert
        result.ShouldNotBeNull();
        result.Records.Count().ShouldBe(3);
        result.TotalCount.ShouldBe(3);
    }

    [Fact]
    public async Task Test_paged_list_includes_soft_deleted_records_when_requested()
    {
        // arrange
        var carIds = new List<Guid>();
        for (int i = 0; i < 5; i++)
        {
            var carId = Guid.NewGuid();
            carIds.Add(carId);
            await ExecUser1(x => x.Insert(CreateInsertCarRequest(carId, $"Car {i:D2}")));
        }

        // soft delete two records
        await ExecUser1(x => x.SoftDelete(carIds[0]));
        await ExecUser1(x => x.SoftDelete(carIds[1]));

        // act
        var request = new ListRequest(Car.Type) { PageSize = 10, IncludeDeleted = true };
        var result = await Exec(x => x.ListPaged<Car>(request));

        // assert
        result.ShouldNotBeNull();
        result.Records.Count().ShouldBe(5);
        result.TotalCount.ShouldBe(5);
    }

    private InsertRequest CreateInsertCarRequest(Guid carId, string model, string make = "Toyota") =>
        InsertRequest.Create(carId, Car.Type, new Car
        {
            Id = carId, Make = make, Model = model
        });
}