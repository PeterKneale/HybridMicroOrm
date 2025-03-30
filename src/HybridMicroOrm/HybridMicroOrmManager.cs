using static HybridMicroOrm.Constants;

namespace HybridMicroOrm;

internal class HybridMicroOrmManager(IOptions<HybridMicroOrmOptions> options, ILogger<HybridMicroOrmManager> log) : IHybridMicroOrmManager
{
    private readonly HybridMicroOrmOptions _options = options.Value ?? throw new ArgumentNullException(nameof(options));

    public async Task Init()
    {
        log.LogInformation("Initializing HybridMicroOrmManager...");
        await using var connection = new NpgsqlConnection(_options.ConnectionString);
        var sql = $@"
            CREATE TABLE {_options.TableName} (
                {Id} UUID PRIMARY KEY,
                {TypeColumn} TEXT NOT NULL,
                {TenantId} UUID NULL,
                {Data} JSONB NOT NULL,
                {CreatedAt} TIMESTAMPTZ NOT NULL,
                {CreatedBy} UUID NULL,
                {UpdatedAt} TIMESTAMPTZ NULL,
                {UpdatedBy} UUID NULL,
                {DeletedAt} TIMESTAMPTZ NULL,
                {DeletedBy} UUID NULL
            );

            CREATE INDEX idx_id_tenant ON {_options.TableName} ({Id}, {TenantId});
            CREATE INDEX idx_id_type_tenant ON {_options.TableName} ({Id}, {TypeColumn}, {TenantId});
        ";
        log.LogInformation("Executing SQL: {sql}", sql);
        await connection.ExecuteAsync(sql);
        log.LogInformation("HybridMicroOrmManager initialized");
    }

    public async Task Destroy()
    {
        log.LogInformation("Destroying HybridMicroOrmManager...");
        var sql = $"DROP TABLE IF EXISTS {_options.TableName};";
        log.LogInformation("Executing SQL: {sql}", sql);
        await using var connection = new NpgsqlConnection(_options.ConnectionString);
        await connection.ExecuteAsync(sql);
    }
}