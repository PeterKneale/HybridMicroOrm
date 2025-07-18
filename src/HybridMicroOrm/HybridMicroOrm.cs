using static HybridMicroOrm.Constants;

namespace HybridMicroOrm;

internal class HybridMicroOrm(ITenantContext tenantContext, IUserContext userContext, ICurrentDateTime currentDateTime, IOptions<HybridMicroOrmOptions> options, ILogger<HybridMicroOrm> log) : IHybridMicroOrm
{
    private readonly HybridMicroOrmOptions _options = options.Value ?? throw new ArgumentNullException(nameof(options));

    private NpgsqlConnection GetConnection() => new(_options.ConnectionString);

    public async Task Insert(InsertRequest request)
    {
        var sql = $"""
                   INSERT INTO {_options.TableName} ({Id}, {TypeColumn}, {TenantId}, {Data}, {CreatedAt}, {CreatedBy}) 
                   VALUES (@Id, @Type, @TenantId, @Data::jsonb, @CreatedAt, @CreatedBy)
                   """;
        var parameters = new
        {
            request.Id,
            request.Type,
            request.Data,
            CreatedAt = currentDateTime.UtcNow,
            CreatedBy = userContext.UserId,
            TenantId = request.IsTenantData ? tenantContext.TenantId : null
        };
        Log(sql, parameters);

        await using var connection = GetConnection();
        await connection.ExecuteAsync(sql, parameters);
    }

    public async Task<Record?> Get(Guid id) => await Get(new GetRequest(id));

    public async Task<Record?> Get(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("ID cannot be null, empty, or whitespace.", nameof(id));

        if (!Guid.TryParse(id, out var guidId))
            throw new ArgumentException($"ID '{id}' is not a valid GUID format.", nameof(id));

        return await Get(guidId);
    }

    public async Task<Record?> Get(GetRequest request)
    {
        var sql = $"""
                    SELECT * FROM {_options.TableName} 
                    WHERE {Id} = @Id 
                    AND (@Type IS NULL OR {TypeColumn} = @Type) 
                    AND ({TenantId} IS NULL OR {TenantId} = @TenantId)
                    AND ({DeletedAt} IS NULL OR @IncludeDeleted = TRUE);
                   """;
        var parameters = new
        {
            Id = request.Id,
            Type = request.Type,
            IncludeDeleted = request.IncludeDeleted,
            tenantContext.TenantId
        };
        Log(sql, parameters);
        await using var connection = GetConnection();
        return await connection.QuerySingleOrDefaultAsync<Record>(sql, parameters);
    }

    public async Task<IEnumerable<Record>> List(ListRequest request)
    {
        var filterSql = request.Filter == null ? "" : $"AND {request.Filter.Query}";
        var sql = $"""
                   SELECT * FROM {_options.TableName} WHERE {TypeColumn} = @Type 
                   AND ({TenantId} IS NULL OR {TenantId} = @TenantId)
                   AND ({DeletedAt} IS NULL OR @IncludeDeleted = TRUE)
                   {filterSql}
                   ORDER BY {request.SortBy.ToColumnName()} {request.SortOrder.ToSqlOrder()}
                   """;
        object parameters = new
        {
            request.Type,
            tenantContext.TenantId,
            request.IncludeDeleted
        };
        if (request.Filter != null)
        {
            parameters = MergeFilterIntoParameters(request.Filter, parameters);
        }

        Log(sql, parameters);
        await using var connection = GetConnection();
        return await connection.QueryAsync<Record>(sql, parameters);
    }

    private static object MergeFilterIntoParameters(Filter filter, object parameters)
    {
        dynamic merged = new System.Dynamic.ExpandoObject();
        var dict = (IDictionary<string, object>)merged;

        foreach (var prop in parameters.GetType().GetProperties())
            dict[prop.Name] = prop.GetValue(parameters)!;

        foreach (var prop in filter!.Parameters.GetType().GetProperties())
            dict[prop.Name] = prop.GetValue(filter.Parameters)!;
        return merged;
    }

    public async Task Update(UpdateRequest request)
    {
        var sql = $"""
                   UPDATE {_options.TableName} 
                   SET {Data} = @Data::jsonb, 
                       {UpdatedAt} = @UpdatedAt, 
                       {UpdatedBy} = @UpdatedBy 
                   WHERE 
                       {Id} = @Id 
                   AND 
                       {TypeColumn} = @Type 
                   AND 
                       ({TenantId} IS NULL OR {TenantId} = @TenantId)
                   """;
        var parameters = new
        {
            request.Id,
            request.Type,
            request.Data,
            UpdatedAt = currentDateTime.UtcNow,
            UpdatedBy = userContext.UserId,
            tenantContext.TenantId
        };
        Log(sql, parameters);

        await using var connection = GetConnection();
        await connection.ExecuteAsync(sql, parameters);
    }

    public async Task Delete(Guid id)
    {
        var sql = $"DELETE FROM {_options.TableName} WHERE {Id} = @Id AND ({TenantId} IS NULL OR {TenantId} = @TenantId)";
        var parameters = new
        {
            Id = id, tenantContext.TenantId
        };
        Log(sql, parameters);
        await using var connection = GetConnection();
        await connection.ExecuteAsync(sql, parameters);
    }

    public async Task SoftDelete(Guid id)
    {
        var sql = $"UPDATE {_options.TableName} SET {DeletedAt} = @DeletedAt, {DeletedBy} = @DeletedBy WHERE {Id} = @Id AND ({TenantId} IS NULL OR {TenantId} = @TenantId)";
        var parameters = new
        {
            Id = id,
            DeletedAt = currentDateTime.UtcNow,
            DeletedBy = userContext.UserId,
            tenantContext.TenantId
        };
        Log(sql, parameters);
        await using var connection = GetConnection();
        await connection.ExecuteAsync(sql, parameters);
    }

    private void Log(string sql, object parameters)
    {
        log.LogInformation("Executing SQL: {sql}\nParameters: {parameters}", sql, parameters);
    }
}