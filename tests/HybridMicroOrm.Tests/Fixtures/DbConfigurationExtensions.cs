namespace HybridMicroOrm.Tests.Fixtures;

public static class DbConfigurationExtensions
{
    private const string Template = "Username={0};Password={1};Database={2};Host={3};Port={4};Include Error Detail=true;";

    public static string GetDbConnectionString(this IConfiguration configuration)
    {
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
        var username = configuration["POSTGRES_USER"] ?? "admin";
        var password = configuration["POSTGRES_PASSWORD"] ?? "password";
        var database = configuration["POSTGRES_DB"] ?? "db";
        var host = configuration["POSTGRES_HOST"] ?? "localhost";
        var port = configuration["POSTGRES_PORT"] ?? "5432";
        return string.Format(Template, username, password, database, host, port);
    }
}