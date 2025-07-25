using static HybridMicroOrm.Constants;

namespace HybridMicroOrm;

// Internal class for database mapping
internal class RecordData
{
    public Guid Id { get; init; }
    public Guid? TenantId { get; init; }
    public string Type { get; set; } = "";
    public string Data { get; set; } = "";
    public DateTime CreatedAt { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}

internal class HybridMicroOrm(ITenantContext tenantContext, IUserContext userContext, ICurrentDateTime currentDateTime, IOptions<HybridMicroOrmOptions> options, IJsonConverter jsonConverter, ILogger<HybridMicroOrm> log) : IHybridMicroOrm
{
    private readonly HybridMicroOrmOptions _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    private readonly IJsonConverter _jsonConverter = jsonConverter ?? throw new ArgumentNullException(nameof(jsonConverter));

    private NpgsqlConnection GetConnection() => new(_options.ConnectionString);

    public async Task Insert(InsertRequest request, CancellationToken cancellationToken = default)
    {
        var sql = $"""
                   INSERT INTO {_options.TableName} ({Id}, {TypeColumn}, {TenantId}, {Data}, {CreatedAt}, {CreatedBy}) 
                   VALUES (@Id, @Type, @TenantId, @Data::jsonb, @CreatedAt, @CreatedBy)
                   """;
        var parameters = new
        {
            request.Id,
            request.Type,
            Data = _jsonConverter.Serialize(request.Data),
            CreatedAt = currentDateTime.UtcNow,
            CreatedBy = userContext.UserId,
            TenantId = request.IsTenantData ? tenantContext.TenantId : null
        };
        Log(sql, parameters);

        await using var connection = GetConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }

    public async Task<Record<T>?> Get<T>(Guid id, CancellationToken cancellationToken = default) => await Get<T>(new GetRequest(id), cancellationToken);

    public async Task<Record<T>?> Get<T>(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("ID cannot be null, empty, or whitespace.", nameof(id));

        if (!Guid.TryParse(id, out var guidId))
            throw new ArgumentException($"ID '{id}' is not a valid GUID format.", nameof(id));

        return await Get<T>(guidId, cancellationToken);
    }

    public async Task<Record<T>?> Get<T>(GetRequest request, CancellationToken cancellationToken = default)
    {
        var sql = $"""
                    SELECT * FROM {_options.TableName} 
                    WHERE 
                    ({TenantId} IS NULL OR {TenantId} = @TenantId)
                    AND 
                    {Id} = @Id 
                    AND 
                    (@Type IS NULL OR {TypeColumn} = @Type) 
                    AND 
                    ({DeletedAt} IS NULL OR @IncludeDeleted = TRUE);
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
        var recordData = await connection.QuerySingleOrDefaultAsync<RecordData>(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
        
        if (recordData == null)
            return null;

        return new Record<T>
        {
            Id = recordData.Id,
            TenantId = recordData.TenantId,
            Type = recordData.Type,
            Data = _jsonConverter.Deserialize<T>(recordData.Data),
            CreatedAt = recordData.CreatedAt,
            CreatedBy = recordData.CreatedBy,
            UpdatedAt = recordData.UpdatedAt,
            UpdatedBy = recordData.UpdatedBy,
            DeletedAt = recordData.DeletedAt,
            DeletedBy = recordData.DeletedBy
        };
    }

    public async Task<IEnumerable<Record<T>>> List<T>(ListRequest request, CancellationToken cancellationToken = default)
    {
        var filterSql = request.Filter == null ? "" : $"AND {request.Filter.Query}";
        var sql = $"""
                   SELECT * FROM {_options.TableName} WHERE 
                   ({TenantId} IS NULL OR {TenantId} = @TenantId)
                   AND 
                   {TypeColumn} = @Type 
                   AND 
                   ({DeletedAt} IS NULL OR @IncludeDeleted = TRUE)
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
        var recordDataList = await connection.QueryAsync<RecordData>(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
        
        return recordDataList.Select(recordData => new Record<T>
        {
            Id = recordData.Id,
            TenantId = recordData.TenantId,
            Type = recordData.Type,
            Data = _jsonConverter.Deserialize<T>(recordData.Data),
            CreatedAt = recordData.CreatedAt,
            CreatedBy = recordData.CreatedBy,
            UpdatedAt = recordData.UpdatedAt,
            UpdatedBy = recordData.UpdatedBy,
            DeletedAt = recordData.DeletedAt,
            DeletedBy = recordData.DeletedBy
        });
    }

    public async Task<PagedResponse<T>> ListPaged<T>(ListRequest request, CancellationToken cancellationToken = default)
    {
        var filterSql = request.Filter == null ? "" : $"AND {request.Filter.Query}";
        var whereClause = $"""
                          WHERE 
                          ({TenantId} IS NULL OR {TenantId} = @TenantId)
                          AND 
                          {TypeColumn} = @Type 
                          AND 
                          ({DeletedAt} IS NULL OR @IncludeDeleted = TRUE)
                          {filterSql}
                          """;
        
        // Get total count
        var countSql = $"SELECT COUNT(*) FROM {_options.TableName} {whereClause}";
        
        // Get paged data
        var offset = (request.PageNumber - 1) * request.PageSize;
        var dataSql = $"""
                      SELECT * FROM {_options.TableName} {whereClause}
                      ORDER BY {request.SortBy.ToColumnName()} {request.SortOrder.ToSqlOrder()}
                      LIMIT @PageSize OFFSET @Offset
                      """;
        
        object parameters = new
        {
            request.Type,
            tenantContext.TenantId,
            request.IncludeDeleted,
            request.PageSize,
            Offset = offset
        };
        
        if (request.Filter != null)
        {
            parameters = MergeFilterIntoParameters(request.Filter, parameters);
        }

        Log(countSql, parameters);
        Log(dataSql, parameters);
        
        await using var connection = GetConnection();
        
        // Execute both queries
        var totalCount = await connection.QuerySingleAsync<int>(new CommandDefinition(countSql, parameters, cancellationToken: cancellationToken));
        var recordDataList = await connection.QueryAsync<RecordData>(new CommandDefinition(dataSql, parameters, cancellationToken: cancellationToken));
        
        var records = recordDataList.Select(recordData => new Record<T>
        {
            Id = recordData.Id,
            TenantId = recordData.TenantId,
            Type = recordData.Type,
            Data = _jsonConverter.Deserialize<T>(recordData.Data),
            CreatedAt = recordData.CreatedAt,
            CreatedBy = recordData.CreatedBy,
            UpdatedAt = recordData.UpdatedAt,
            UpdatedBy = recordData.UpdatedBy,
            DeletedAt = recordData.DeletedAt,
            DeletedBy = recordData.DeletedBy
        });
        
        return new PagedResponse<T>(records, totalCount, request.PageNumber, request.PageSize);
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

    public async Task Update(UpdateRequest request, CancellationToken cancellationToken = default)
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
            Data = _jsonConverter.Serialize(request.Data),
            UpdatedAt = currentDateTime.UtcNow,
            UpdatedBy = userContext.UserId,
            tenantContext.TenantId
        };
        Log(sql, parameters);

        await using var connection = GetConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }

    public async Task Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var sql = $"DELETE FROM {_options.TableName} WHERE {Id} = @Id AND ({TenantId} IS NULL OR {TenantId} = @TenantId)";
        var parameters = new
        {
            Id = id, tenantContext.TenantId
        };
        Log(sql, parameters);
        await using var connection = GetConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }

    public async Task SoftDelete(Guid id, CancellationToken cancellationToken = default)
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
        await connection.ExecuteAsync(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }

    public bool Exists(Guid id)
    {
        return ExistsAsync(id, CancellationToken.None).GetAwaiter().GetResult();
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sql = $"""
                   SELECT EXISTS(
                       SELECT 1 FROM {_options.TableName} 
                       WHERE {Id} = @Id 
                       AND ({TenantId} IS NULL OR {TenantId} = @TenantId)
                       AND ({DeletedAt} IS NULL)
                   )
                   """;
        var parameters = new
        {
            Id = id,
            tenantContext.TenantId
        };
        Log(sql, parameters);
        await using var connection = GetConnection();
        return await connection.QuerySingleAsync<bool>(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }

    private void Log(string sql, object parameters)
    {
        log.LogInformation("Executing SQL: {sql}\nParameters: {parameters}", sql, parameters);
    }
}