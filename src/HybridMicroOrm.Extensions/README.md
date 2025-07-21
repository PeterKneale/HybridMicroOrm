# HybridMicroOrm Extension Methods

This directory contains an alternative implementation of HybridMicroOrm as extension methods for `IDbConnection`. This approach provides the same functionality as the NuGet package but with more flexibility and fewer dependencies.

## Why Extension Methods?

The extension methods approach offers several advantages:

- **Zero Package Dependencies**: No NuGet package management
- **Full Customization**: Modify table schemas, column names, and behavior
- **Database Flexibility**: Adapt for SQL Server, MySQL, or other databases
- **No Dependency Injection**: Works with any connection without DI setup
- **Easy Integration**: Add to existing projects without architectural changes

## Single File Implementation

The entire implementation is contained in a single file (`HybridMicroOrmExtensions.cs`) that you can copy into your project. It depends only on:

- `Dapper` (for database operations)
- `Npgsql` (for PostgreSQL - replaceable for other databases)
- `System.Text.Json` (for JSON serialization - customizable)

## Usage Examples

### Basic Setup

```csharp
using HybridMicroOrm.Extensions;
using Npgsql;

// Create connection (no DI required)
using var connection = new NpgsqlConnection("Host=localhost;Database=mydb;...");
await connection.OpenAsync();

// Configure options (optional)
var options = new HybridRecordOptions
{
    TableName = "my_records",      // Custom table name
    TenantId = currentTenantId,    // Current tenant
    UserId = currentUserId         // Current user
};

// Create table (one-time setup)
await connection.CreateHybridRecordsTable(options);
```

### CRUD Operations

```csharp
// Sample data model
public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
}

// Insert
var customerId = Guid.NewGuid();
var customer = new Customer { Id = customerId, Name = "John Doe", Email = "john@example.com" };
await connection.InsertHybridRecord(customerId, "customer", customer, options);

// Get
var record = await connection.GetHybridRecord<Customer>(customerId, options);
Console.WriteLine($"Customer: {record?.Data.Name}");

// Update
customer.Email = "newemail@example.com";
await connection.UpdateHybridRecord(customerId, "customer", customer, options);

// List with filtering
var customers = await connection.ListHybridRecords<Customer>(
    type: "customer",
    options: options,
    whereClause: "data->>'name' ILIKE @NameFilter",
    parameters: new { NameFilter = "%john%" }
);

// Check existence
var exists = await connection.HybridRecordExists(customerId, options);

// Soft delete
await connection.SoftDeleteHybridRecord(customerId, options);

// Hard delete
await connection.DeleteHybridRecord(customerId, options);
```

### Custom Table Schema

```csharp
var customOptions = new HybridRecordOptions
{
    TableName = "documents",
    IdColumn = "document_id",
    TenantIdColumn = "company_id",
    TypeColumn = "doc_type",
    DataColumn = "content",
    CreatedAtColumn = "created_date",
    // ... other custom column names
};

await connection.CreateHybridRecordsTable(customOptions);
```

### Custom JSON Serialization

```csharp
// Using Newtonsoft.Json instead of System.Text.Json
var options = new HybridRecordOptions
{
    JsonSerializer = obj => Newtonsoft.Json.JsonConvert.SerializeObject(obj),
    JsonDeserializer = (json, type) => Newtonsoft.Json.JsonConvert.DeserializeObject(json, type)!
};
```

### Multi-Database Support

To adapt for SQL Server, modify the PostgreSQL-specific parts:

```csharp
// Change JSONB to NVARCHAR(MAX) for SQL Server
var createTableSql = $"""
    CREATE TABLE {options.TableName} (
        {options.IdColumn} UNIQUEIDENTIFIER PRIMARY KEY,
        {options.TenantIdColumn} UNIQUEIDENTIFIER,
        {options.TypeColumn} NVARCHAR(255) NOT NULL,
        {options.DataColumn} NVARCHAR(MAX) NOT NULL,
        -- ... other columns
    );
    """;

// Remove PostgreSQL-specific ::jsonb casting
var sql = $"""
    INSERT INTO {options.TableName} (..., {options.DataColumn}) 
    VALUES (..., @Data)  -- Remove ::jsonb
    """;
```

## Configuration Options

### HybridRecordOptions Properties

```csharp
public class HybridRecordOptions
{
    // Table and column customization
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
    
    // Context
    public Guid? TenantId { get; set; }
    public Guid? UserId { get; set; }
    
    // Custom JSON handling
    public Func<object, string>? JsonSerializer { get; set; }
    public Func<string, Type, object>? JsonDeserializer { get; set; }
}
```

## Advanced Usage

### Transaction Support

```csharp
using var transaction = await connection.BeginTransactionAsync();
try
{
    await connection.InsertHybridRecord(id1, "customer", customer1, options);
    await connection.InsertHybridRecord(id2, "order", order1, options);
    
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### Bulk Operations

```csharp
// Bulk insert (implement as needed)
public static async Task BulkInsertHybridRecords<T>(
    this IDbConnection connection,
    IEnumerable<(Guid Id, string Type, T Data)> records,
    HybridRecordOptions? options = null)
{
    var sql = $"INSERT INTO {options.TableName} (...) VALUES (...);";
    var parameters = records.Select(r => new { ... });
    await connection.ExecuteAsync(sql, parameters);
}
```

### Custom Indexing

```csharp
// Add custom indexes for your specific queries
await connection.ExecuteAsync($"""
    CREATE INDEX idx_customer_email 
    ON {options.TableName} USING GIN ((data->>'email')) 
    WHERE type = 'customer';
    """);
```

## Migration from NuGet Package

To migrate from the NuGet package to extension methods:

1. **Copy the extension methods file** into your project
2. **Remove NuGet package references**:
   ```bash
   dotnet remove package HybridMicroOrm
   dotnet remove package HybridMicroOrm.Contracts
   ```
3. **Update service registration**:
   ```csharp
   // Remove
   services.AddHybridMicroOrm(options => ...);
   services.AddScoped<ITenantContext, ...>();
   
   // Replace with
   services.AddSingleton(new HybridRecordOptions { ... });
   ```
4. **Update usage code**:
   ```csharp
   // From
   await orm.Insert(InsertRequest.Create(...));
   
   // To
   await connection.InsertHybridRecord(id, type, data, options);
   ```

## Comparison with NuGet Package

| Feature | Extension Methods | NuGet Package |
|---------|------------------|---------------|
| Dependencies | Minimal (Dapper + driver) | Multiple packages |
| Customization | Full control | Limited |
| Setup Complexity | Low | Medium (DI required) |
| Maintenance | Self-maintained | External |
| Updates | Manual | Automatic |
| Database Support | Adaptable | PostgreSQL only |
| Schema Flexibility | Complete | Fixed |

## When to Use Extension Methods

Choose extension methods when you need:

- Custom table schemas or column names
- Support for multiple database providers
- Integration with existing data access patterns
- Minimal external dependencies
- Full control over the implementation
- Custom JSON serialization strategies
- Performance optimizations for specific use cases

## When to Use NuGet Package

Choose the NuGet package when you want:

- Standard implementation with proven reliability
- Automatic updates and bug fixes
- Minimal setup and configuration
- Consistent patterns across teams
- Community support and documentation