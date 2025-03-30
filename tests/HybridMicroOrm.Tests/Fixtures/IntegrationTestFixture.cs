using HybridMicroOrm.Contracts;
using HybridMicroOrm.Tests.Contexts;

namespace HybridMicroOrm.Tests.Fixtures;

public class IntegrationTestFixture : MartinCostello.Logging.XUnit.ITestOutputHelperAccessor
{
    public IntegrationTestFixture()
    {
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        Services = new ServiceCollection()
            .AddHybridMicroOrm(x =>
            {
                x.ConnectionString = Configuration.GetDbConnectionString();
            })
            .AddScoped<ITenantContext, TestTenantContext>()
            .AddScoped<IUserContext, TestUserContext>()
            .AddScoped<ICurrentDateTime, TestCurrentDateTime>()
            .AddLogging(c => c.AddXUnit(this))
            .BuildServiceProvider();
    }

    public ServiceProvider Services { get; set; }

    public IConfiguration Configuration { get; set; }

    public ITestOutputHelper? OutputHelper { get; set; }
}