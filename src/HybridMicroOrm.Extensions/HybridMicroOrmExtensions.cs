using System.Data;
using System.Text.Json;
using Dapper;
using Npgsql;

namespace HybridMicroOrm.Extensions;

/// <summary>
/// Configuration options for HybridMicroOrm extension methods
/// </summary>
public class HybridRecordOptions
{
    public string TableName { get; set; } = "records";
    public string IdColumn { get; set; } = "id";
    public string TenantIdColumn { get; set; } = "tenant_id";
    public string TypeColumn { get; set; } = "type";
    public string DataColumn { get; set; } = "data";
    public string CreatedAtColumn { get; set; } = "created_at";
    public string CreatedByColumn { get; set; } = "created_by";
    public string UpdatedAtColumn { get; set; } = "updated_at";
    public string UpdatedByColumn { get; set; } = "updated_by";
    public string DeletedAtColumn { get; set; } = "deleted_at";
    public string DeletedByColumn { get; set; } = "deleted_by";
    
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
    public Func<object, string>? JsonSerializer { get; set; }
    public Func<string, Type, object>? JsonDeserializer { get; set; }
}

/// <summary>
/// Simplified record structure for extension methods approach
/// </summary>
public class HybridRecord<T>
{
    public Guid Id { get; init; }
    public Guid? TenantId { get; init; }
    public string Type { get; set; } = "";
    public T Data { get; set; } = default!;
    public DateTime CreatedAt { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}

/// <summary>
/// Extension methods for IDbConnection to provide HybridMicroOrm functionality
/// without requiring NuGet packages or dependency injection
/// </summary>
public static class HybridMicroOrmExtensions
{
    private static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    #region Get Operations

    /// <summary>
    /// Get a record by ID with default options
    /// </summary>
    public static async Task<HybridRecord<T>?> GetHybridRecord<T>(
        this IDbConnection connection, 
        Guid id,
        HybridRecordOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new HybridRecordOptions();
        
        var sql = $"""
                  SELECT * FROM {options.TableName} 
                  WHERE {options.IdColumn} = @Id 
                  AND ({options.TenantIdColumn} IS NULL OR {options.TenantIdColumn} = @TenantId)
                  AND ({options.DeletedAtColumn} IS NULL);
                  """;
                  
        var parameters = new { Id = id, TenantId = options.TenantId };
        
        var recordData = await connection.QuerySingleOrDefaultAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
            
        return MapToHybridRecord<T>(recordData, options);
    }

    /// <summary>
    /// Get a record by ID and type
    /// </summary>
    public static async Task<HybridRecord<T>?> GetHybridRecord<T>(
        this IDbConnection connection,
        Guid id,
        string type,
        HybridRecordOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new HybridRecordOptions();
        
        var sql = $"""
                  SELECT * FROM {options.TableName} 
                  WHERE {options.IdColumn} = @Id 
                  AND {options.TypeColumn} = @Type
                  AND ({options.TenantIdColumn} IS NULL OR {options.TenantIdColumn} = @TenantId)
                  AND ({options.DeletedAtColumn} IS NULL);
                  """;
                  
        var parameters = new { Id = id, Type = type, TenantId = options.TenantId };
        
        var recordData = await connection.QuerySingleOrDefaultAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
            
        return MapToHybridRecord<T>(recordData, options);
    }

    #endregion

    #region List Operations

    /// <summary>
    /// List records by type with optional filtering
    /// </summary>
    public static async Task<IEnumerable<HybridRecord<T>>> ListHybridRecords<T>(
        this IDbConnection connection,
        string type,
        HybridRecordOptions? options = null,
        string? whereClause = null,
        object? parameters = null,
        string orderBy = "created_at DESC",
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        options ??= new HybridRecordOptions();
        
        var additionalWhere = string.IsNullOrWhiteSpace(whereClause) ? "" : $"AND ({whereClause})";
        var deletedFilter = includeDeleted ? "" : $"AND ({options.DeletedAtColumn} IS NULL)";
        
        var sql = $"""
                  SELECT * FROM {options.TableName} 
                  WHERE {options.TypeColumn} = @Type
                  AND ({options.TenantIdColumn} IS NULL OR {options.TenantIdColumn} = @TenantId)
                  {deletedFilter}
                  {additionalWhere}
                  ORDER BY {orderBy}
                  """;

        var queryParams = new Dictionary<string, object?>
        {
            ["Type"] = type,
            ["TenantId"] = options.TenantId
        };

        // Merge additional parameters
        if (parameters != null)
        {
            foreach (var prop in parameters.GetType().GetProperties())
            {
                queryParams[prop.Name] = prop.GetValue(parameters);
            }
        }

        var recordDataList = await connection.QueryAsync(
            new CommandDefinition(sql, queryParams, cancellationToken: cancellationToken));

        return recordDataList.Select(recordData => MapToHybridRecord<T>(recordData, options))
                            .Where(record => record != null)
                            .Cast<HybridRecord<T>>();
    }

    #endregion

    #region Insert Operations

    /// <summary>
    /// Insert a new record
    /// </summary>
    public static async Task InsertHybridRecord<T>(
        this IDbConnection connection,
        Guid id,
        string type,
        T data,
        HybridRecordOptions? options = null,
        bool isTenantData = true,
        CancellationToken cancellationToken = default)
    {
        options ??= new HybridRecordOptions();
        
        var serializer = options.JsonSerializer ?? (obj => JsonSerializer.Serialize(obj, DefaultJsonOptions));
        var tenantId = isTenantData ? options.TenantId : null;
        
        var sql = $"""
                  INSERT INTO {options.TableName} 
                  ({options.IdColumn}, {options.TypeColumn}, {options.TenantIdColumn}, {options.DataColumn}, 
                   {options.CreatedAtColumn}, {options.CreatedByColumn}) 
                  VALUES (@Id, @Type, @TenantId, @Data::jsonb, @CreatedAt, @CreatedBy)
                  """;
                  
        var parameters = new
        {
            Id = id,
            Type = type,
            TenantId = tenantId,
            Data = serializer(data!),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = options.UserId
        };
        
        await connection.ExecuteAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }

    #endregion

    #region Update Operations

    /// <summary>
    /// Update an existing record
    /// </summary>
    public static async Task UpdateHybridRecord<T>(
        this IDbConnection connection,
        Guid id,
        string type,
        T data,
        HybridRecordOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new HybridRecordOptions();
        
        var serializer = options.JsonSerializer ?? (obj => JsonSerializer.Serialize(obj, DefaultJsonOptions));
        
        var sql = $"""
                  UPDATE {options.TableName} 
                  SET {options.DataColumn} = @Data::jsonb, 
                      {options.UpdatedAtColumn} = @UpdatedAt, 
                      {options.UpdatedByColumn} = @UpdatedBy 
                  WHERE {options.IdColumn} = @Id 
                  AND {options.TypeColumn} = @Type 
                  AND ({options.TenantIdColumn} IS NULL OR {options.TenantIdColumn} = @TenantId)
                  """;
                  
        var parameters = new
        {
            Id = id,
            Type = type,
            Data = serializer(data!),
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = options.UserId,
            TenantId = options.TenantId
        };
        
        await connection.ExecuteAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }

    #endregion

    #region Delete Operations

    /// <summary>
    /// Hard delete a record (permanently removes it)
    /// </summary>
    public static async Task DeleteHybridRecord(
        this IDbConnection connection,
        Guid id,
        HybridRecordOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new HybridRecordOptions();
        
        var sql = $"""
                  DELETE FROM {options.TableName} 
                  WHERE {options.IdColumn} = @Id 
                  AND ({options.TenantIdColumn} IS NULL OR {options.TenantIdColumn} = @TenantId)
                  """;
                  
        var parameters = new { Id = id, TenantId = options.TenantId };
        
        await connection.ExecuteAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }

    /// <summary>
    /// Soft delete a record (marks as deleted)
    /// </summary>
    public static async Task SoftDeleteHybridRecord(
        this IDbConnection connection,
        Guid id,
        HybridRecordOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new HybridRecordOptions();
        
        var sql = $"""
                  UPDATE {options.TableName} 
                  SET {options.DeletedAtColumn} = @DeletedAt, {options.DeletedByColumn} = @DeletedBy 
                  WHERE {options.IdColumn} = @Id 
                  AND ({options.TenantIdColumn} IS NULL OR {options.TenantIdColumn} = @TenantId)
                  """;
                  
        var parameters = new
        {
            Id = id,
            DeletedAt = DateTime.UtcNow,
            DeletedBy = options.UserId,
            TenantId = options.TenantId
        };
        
        await connection.ExecuteAsync(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }

    #endregion

    #region Exists Operations

    /// <summary>
    /// Check if a record exists
    /// </summary>
    public static async Task<bool> HybridRecordExists(
        this IDbConnection connection,
        Guid id,
        HybridRecordOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new HybridRecordOptions();
        
        var sql = $"""
                  SELECT EXISTS(
                      SELECT 1 FROM {options.TableName} 
                      WHERE {options.IdColumn} = @Id 
                      AND ({options.TenantIdColumn} IS NULL OR {options.TenantIdColumn} = @TenantId)
                      AND ({options.DeletedAtColumn} IS NULL)
                  )
                  """;
                  
        var parameters = new { Id = id, TenantId = options.TenantId };
        
        return await connection.QuerySingleAsync<bool>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }

    #endregion

    #region Schema Management

    /// <summary>
    /// Create the hybrid records table with indexes
    /// </summary>
    public static async Task CreateHybridRecordsTable(
        this IDbConnection connection,
        HybridRecordOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new HybridRecordOptions();
        
        var createTableSql = $"""
                             CREATE TABLE IF NOT EXISTS {options.TableName} (
                                 {options.IdColumn} UUID PRIMARY KEY,
                                 {options.TenantIdColumn} UUID,
                                 {options.TypeColumn} VARCHAR(255) NOT NULL,
                                 {options.DataColumn} JSONB NOT NULL,
                                 {options.CreatedAtColumn} TIMESTAMP NOT NULL DEFAULT NOW(),
                                 {options.CreatedByColumn} UUID,
                                 {options.UpdatedAtColumn} TIMESTAMP,
                                 {options.UpdatedByColumn} UUID,
                                 {options.DeletedAtColumn} TIMESTAMP,
                                 {options.DeletedByColumn} UUID
                             );
                             """;

        await connection.ExecuteAsync(
            new CommandDefinition(createTableSql, cancellationToken: cancellationToken));

        // Create indexes
        var indexSqls = new[]
        {
            $"CREATE INDEX IF NOT EXISTS idx_{options.TableName}_{options.IdColumn}_{options.TenantIdColumn} ON {options.TableName} ({options.IdColumn}, {options.TenantIdColumn});",
            $"CREATE INDEX IF NOT EXISTS idx_{options.TableName}_{options.TypeColumn}_{options.TenantIdColumn} ON {options.TableName} ({options.TypeColumn}, {options.TenantIdColumn});",
            $"CREATE INDEX IF NOT EXISTS idx_{options.TableName}_{options.CreatedAtColumn} ON {options.TableName} ({options.CreatedAtColumn});",
            $"CREATE INDEX IF NOT EXISTS idx_{options.TableName}_data_gin ON {options.TableName} USING GIN ({options.DataColumn});"
        };

        foreach (var indexSql in indexSqls)
        {
            await connection.ExecuteAsync(
                new CommandDefinition(indexSql, cancellationToken: cancellationToken));
        }
    }

    /// <summary>
    /// Drop the hybrid records table
    /// </summary>
    public static async Task DropHybridRecordsTable(
        this IDbConnection connection,
        HybridRecordOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new HybridRecordOptions();
        
        var sql = $"DROP TABLE IF EXISTS {options.TableName};";
        
        await connection.ExecuteAsync(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
    }

    #endregion

    #region Private Helper Methods

    private static HybridRecord<T>? MapToHybridRecord<T>(dynamic? recordData, HybridRecordOptions options)
    {
        if (recordData == null) return null;

        var deserializer = options.JsonDeserializer ?? 
            ((json, type) => JsonSerializer.Deserialize(json, type, DefaultJsonOptions)!);

        return new HybridRecord<T>
        {
            Id = recordData.id,
            TenantId = recordData.tenant_id,
            Type = recordData.type,
            Data = (T)deserializer(recordData.data, typeof(T)),
            CreatedAt = recordData.created_at,
            CreatedBy = recordData.created_by,
            UpdatedAt = recordData.updated_at,
            UpdatedBy = recordData.updated_by,
            DeletedAt = recordData.deleted_at,
            DeletedBy = recordData.deleted_by
        };
    }

    #endregion
}