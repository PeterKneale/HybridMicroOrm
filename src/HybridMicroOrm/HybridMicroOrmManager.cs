using static HybridMicroOrm.Constants;

namespace HybridMicroOrm;

internal class HybridMicroOrmManager(IOptions<HybridMicroOrmOptions> options, ILogger<HybridMicroOrmManager> log) : IHybridMicroOrmManager
{
    private readonly HybridMicroOrmOptions _options = options.Value ?? throw new ArgumentNullException(nameof(options));

    private NpgsqlConnection GetConnection() => new(_options.ConnectionString);

    public async Task Init(CancellationToken cancellationToken = default)
    {
        if (await Exists(cancellationToken))
        {
            log.LogDebug("Schema exists.");
        }
        else
        {
            log.LogDebug("Schema does not exist. Creating schema");
            await Create(cancellationToken);
        }
    }

    private async Task<bool> Exists(CancellationToken cancellationToken = default)
    {
        log.LogInformation($"Checking if table {_options.TableName} exists...");
        const string sql = """
                            SELECT EXISTS (
                                SELECT 1
                                FROM   information_schema.tables
                                WHERE  table_name   = @TableName
                            );
                            """;

        await using var connection = GetConnection();
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(sql, new
        {
            _options.TableName
        }, cancellationToken: cancellationToken));
    }

    private async Task Create(CancellationToken cancellationToken = default)
    {
        log.LogInformation($"Creating ${_options.TableName}...");
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

            -- Index to speed up queries filtering by id and tenant_id (id based multi-tenant get operations)
            CREATE INDEX idx_id_tenant ON {_options.TableName} ({TenantId}, {Id});
            -- Index to speed up queries filtering by tenant_id and type (tenant-based multi-tenant list operations)
            CREATE INDEX idx_tenant_type ON {_options.TableName} ({TenantId}, {TypeColumn});
            -- GIN index on JSONB data column for efficient querying/filtering within JSON documents
            CREATE INDEX idx_data_gin ON {_options.TableName} USING GIN ({Data});
        ";
        log.LogInformation("Executing SQL: {sql}", sql);
        await using var connection = GetConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));
        log.LogInformation("HybridMicroOrmManager initialized");
    }

    public async Task Drop(CancellationToken cancellationToken = default)
    {
        log.LogInformation($"Dropping {_options.TableName}...");
        var sql = $"DROP TABLE IF EXISTS {_options.TableName};";
        log.LogInformation("Executing SQL: {sql}", sql);
        await using var connection = GetConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: cancellationToken));
    }
}