# HybridMicroOrm

[![Build Status](https://github.com/PeterKneale/HybridMicroOrm/workflows/Build/badge.svg)](https://github.com/PeterKneale/HybridMicroOrm/actions)
[![NuGet Version](https://img.shields.io/nuget/v/HybridMicroOrm.svg)](https://www.nuget.org/packages/HybridMicroOrm/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/HybridMicroOrm.svg)](https://www.nuget.org/packages/HybridMicroOrm/)

A lightweight, JSON-based micro ORM for PostgreSQL with built-in multi-tenant support, designed for SaaS applications that need the flexibility of JSON storage with the reliability of PostgreSQL.

## What Problem Does It Solve?

HybridMicroOrm addresses several common challenges in modern SaaS application development:

- **Schema Evolution**: Store dynamic data structures without complex database migrations
- **Multi-Tenancy**: Built-in tenant isolation for SaaS applications
- **Audit Trails**: Automatic tracking of who created, updated, or deleted records
- **JSON Flexibility**: Store complex nested objects while maintaining query capabilities
- **Type Safety**: Strongly-typed C# interfaces with JSON serialization/deserialization
- **Soft Deletes**: Mark records as deleted without physically removing them
- **Performance**: Leverages PostgreSQL's JSONB indexing and Dapper's efficiency

## Main Functionality

### Core Features

- **CRUD Operations**: Create, Read, Update, Delete with type-safe methods
- **Multi-Tenant Data Isolation**: Automatic tenant filtering based on context
- **JSON Storage**: Store any serializable object as JSONB in PostgreSQL
- **Audit Trails**: Automatic `created_at`, `updated_at`, `deleted_at` timestamps with user tracking
- **Soft Deletes**: Mark records as deleted while preserving data
- **Filtering and Sorting**: Flexible query capabilities with custom filters
- **Schema Management**: Automatic table creation and management

### Data Model

Each record contains:
- `id` (UUID): Primary key
- `tenant_id` (UUID): Tenant identifier for multi-tenant isolation
- `type` (string): Logical type identifier for the stored object
- `data` (JSONB): The actual JSON data
- `created_at`/`created_by`: Audit trail for creation
- `updated_at`/`updated_by`: Audit trail for updates
- `deleted_at`/`deleted_by`: Soft delete tracking

## Getting Started

### Installation

Install the NuGet packages:

```bash
dotnet add package HybridMicroOrm
dotnet add package HybridMicroOrm.Contracts
```

### Basic Setup

1. **Configure Services** in your `Program.cs`:

```csharp
using HybridMicroOrm;

var builder = WebApplication.CreateBuilder(args);

// Register JSON converter implementation (required)
builder.Services.AddSingleton<IJsonConverter, NewtonsoftJsonConverter>(); // or your preferred implementation

// Add HybridMicroOrm services
builder.Services.AddHybridMicroOrm(options =>
{
    options.ConnectionString = "Host=localhost;Database=mydb;Username=user;Password=pass";
    options.TableName = "records"; // Optional: defaults to "records"
});

// Implement required context interfaces
builder.Services.AddScoped<ITenantContext, YourTenantContext>();
builder.Services.AddScoped<IUserContext, YourUserContext>();

var app = builder.Build();

// Initialize database schema
using (var scope = app.Services.CreateScope())
{
    var manager = scope.ServiceProvider.GetRequiredService<IHybridMicroOrmManager>();
    await manager.Init();
}
```

2. **Implement Context Interfaces**:

```csharp
public class YourTenantContext : ITenantContext
{
    public Guid? TenantId => GetCurrentTenantId(); // Your tenant resolution logic
}

public class YourUserContext : IUserContext
{
    public Guid? UserId => GetCurrentUserId(); // Your user resolution logic
}
```

3. **Implement JSON Converter** (required):

You must implement `IJsonConverter` using your preferred JSON serializer. Here are examples for popular serializers:

**Using Newtonsoft.Json:**
```csharp
using HybridMicroOrm.Contracts;
using Newtonsoft.Json;

public class NewtonsoftJsonConverter : IJsonConverter
{
    public string Serialize(object value)
    {
        return JsonConvert.SerializeObject(value);
    }

    public T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json)!;
    }
}
```

**Using System.Text.Json:**
```csharp
using HybridMicroOrm.Contracts;
using System.Text.Json;

public class SystemTextJsonConverter : IJsonConverter
{
    public string Serialize(object value)
    {
        return JsonSerializer.Serialize(value);
    }

    public T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json)!;
    }
}
```

4. **Use in Your Services**:

```csharp
public class CustomerService
{
    private readonly IHybridMicroOrm _orm;
    private readonly IJsonConverter _jsonConverter;

    public CustomerService(IHybridMicroOrm orm, IJsonConverter jsonConverter)
    {
        _orm = orm;
        _jsonConverter = jsonConverter;
    }

    public async Task<Customer?> GetCustomer(Guid id)
    {
        var record = await _orm.Get(id);
        return record?.Get<Customer>(_jsonConverter);
    }

    public async Task CreateCustomer(Customer customer)
    {
        var request = InsertRequest.Create(
            id: Guid.NewGuid(),
            type: "customer",
            data: customer,
            jsonConverter: _jsonConverter,
            isTenantData: true
        );
        await _orm.Insert(request);
    }
}
```

## Public Methods Documentation

### IHybridMicroOrm Interface

The main interface provides CRUD operations:

#### Get Operations
```csharp
// Get by GUID
Task<Record?> Get(Guid id)

// Get by string ID (converts to GUID)
Task<Record?> Get(string id)

// Get with additional options
Task<Record?> Get(GetRequest request)
```

**GetRequest Options:**
- `Id`: Record identifier
- `Type`: Optional type filter
- `IncludeDeleted`: Include soft-deleted records

#### List Operations
```csharp
Task<IEnumerable<Record>> List(ListRequest request)
```

**ListRequest Options:**
- `Type`: Required type filter
- `Filter`: Custom SQL filter with parameters
- `IncludeDeleted`: Include soft-deleted records
- `SortBy`: Sort by Created, Updated, or Deleted
- `SortOrder`: Ascending or Descending

#### Insert Operations
```csharp
Task Insert(InsertRequest request)
```

**InsertRequest Creation:**
```csharp
var request = InsertRequest.Create(
    id: Guid.NewGuid(),
    type: "product",
    data: productObject,
    isTenantData: true // Set to false for shared data
);
```

#### Update Operations
```csharp
Task Update(UpdateRequest request)
```

**UpdateRequest Creation:**
```csharp
var request = UpdateRequest.Create(
    id: existingId,
    type: "product",
    data: updatedProductObject
);
```

#### Delete Operations
```csharp
// Hard delete (permanently removes record)
Task Delete(Guid id)

// Soft delete (marks as deleted)
Task SoftDelete(Guid id)
```

### Record Class

The returned `Record` object provides:

```csharp
public class Record
{
    public Guid Id { get; init; }
    public Guid? TenantId { get; init; }
    public string Type { get; set; }
    public string Data { get; set; }
    public DateTime CreatedAt { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }

    // Type-safe deserialization using IJsonConverter
    public T Get<T>(IJsonConverter jsonConverter) => jsonConverter.Deserialize<T>(Data);
}
```

## Difference Between ORM and ORM Manager

### IHybridMicroOrm
- **Purpose**: Data access and manipulation
- **Operations**: CRUD operations (Create, Read, Update, Delete)
- **Scope**: Application runtime operations
- **Multi-tenant**: All operations are tenant-aware
- **Usage**: Injected into services for business logic

### IHybridMicroOrmManager
- **Purpose**: Database schema and infrastructure management
- **Operations**: Schema initialization and cleanup
- **Scope**: Application startup/deployment operations
- **Multi-tenant**: Works at the database level, not tenant-specific
- **Usage**: Called during application initialization

```csharp
// Manager usage - typically in Program.cs
var manager = serviceProvider.GetRequiredService<IHybridMicroOrmManager>();
await manager.Init(); // Creates table and indexes if they don't exist
await manager.Drop(); // Drops the table (use with caution!)

// ORM usage - in your business services
var orm = serviceProvider.GetRequiredService<IHybridMicroOrm>();
await orm.Insert(request); // Tenant-aware data operations
```

## Multi-Tenant Support

HybridMicroOrm provides comprehensive multi-tenant support:

### Tenant Isolation
- **Automatic Filtering**: All queries automatically filter by tenant ID
- **Data Separation**: Tenants cannot access each other's data
- **Shared Data**: Option to mark records as non-tenant-specific
- **Context-Driven**: Uses `ITenantContext` to determine current tenant

### Implementation Details
```csharp
public interface ITenantContext
{
    Guid? TenantId { get; }
}
```

### Tenant Data vs Shared Data
```csharp
// Tenant-specific data (default)
var tenantRequest = InsertRequest.Create(id, "order", order, isTenantData: true);

// Shared data (accessible to all tenants)
var sharedRequest = InsertRequest.Create(id, "config", config, isTenantData: false);
```

### Database Schema
The table includes a `tenant_id` column with appropriate indexes:
```sql
CREATE INDEX idx_id_tenant ON records (id, tenant_id);
CREATE INDEX idx_id_type_tenant ON records (id, type, tenant_id);
```

## CI/CD Support

### GitHub Actions Workflow

The project includes a comprehensive CI/CD pipeline:

#### Build Pipeline (`build.yml`)
- **Triggers**: Push/PR to main branch, manual workflow dispatch
- **Environment**: Ubuntu with .NET 8 SDK and PostgreSQL 15
- **Steps**:
  1. Checkout code
  2. Setup .NET 8
  3. Restore dependencies
  4. Build solution
  5. Run tests with PostgreSQL integration
  6. Generate code coverage reports
  7. Upload coverage artifacts

#### Package Pipeline
- **Triggers**: After successful build
- **Steps**:
  1. Pack NuGet packages
  2. Push to NuGet.org (both main and contracts packages)
  3. Uses `NUGET_KEY` secret for authentication

### Test Infrastructure
- **Integration Tests**: Full PostgreSQL database testing
- **Coverage**: XPlat Code Coverage with ReportGenerator
- **Test Framework**: xUnit with Shouldly assertions
- **Docker**: PostgreSQL service container for isolated testing

## Package Publishing

### NuGet Packages

Two packages are published:

1. **HybridMicroOrm** ([NuGet](https://www.nuget.org/packages/HybridMicroOrm/))
   - Main implementation with full ORM functionality
   - Dependencies: Dapper, Npgsql, Microsoft.Extensions.*
   - **No longer includes JSON serializer** - consumers must provide their own IJsonConverter implementation
   - Includes symbol package (.snupkg) for debugging

2. **HybridMicroOrm.Contracts** ([NuGet](https://www.nuget.org/packages/HybridMicroOrm.Contracts/))
   - Interfaces and contracts only (including IJsonConverter)
   - **No external dependencies** - completely standalone
   - Includes symbol package (.snupkg) for debugging

### Publishing Strategy

The project follows NuGet packaging best practices:

- **Automatic Versioning**: Uses [MinVer](https://github.com/adamralph/minver) for Git-based semantic versioning
- **Conditional Publishing**: Packages are only published on GitHub releases, not every push
- **Symbol Packages**: Debugging symbols (.snupkg) are published alongside main packages
- **Package Validation**: Comprehensive validation before publishing including vulnerability scanning
- **Rich Metadata**: Complete package descriptions, repository URLs, and release notes

### Creating Releases

To publish a new version:

1. Create and push a semantic version tag: `git tag v1.0.0 && git push origin v1.0.0`
2. Create a GitHub release with release notes
3. The CI/CD pipeline automatically validates and publishes packages

See [PACKAGING.md](PACKAGING.md) for detailed publishing guidelines.

### Local Package Validation

Use the included validation script to test packages locally:

```bash
./scripts/validate-packages.sh
```

This script performs the same validations as the CI/CD pipeline.

### Package Configuration
```xml
<PropertyGroup>
    <PackageId>HybridMicroOrm</PackageId>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <PackageTags>postgres;sql;json;multi-tenant;saas;dapper;table;storage</PackageTags>
    <PackageDescription>A lightweight, JSON-based micro ORM for PostgreSQL with built-in multi-tenant support</PackageDescription>
    <RepositoryUrl>https://github.com/PeterKneale/HybridMicroOrm</RepositoryUrl>
    
    <!-- MinVer automatic versioning -->
    <MinVerDefaultPreReleaseIdentifiers>alpha</MinVerDefaultPreReleaseIdentifiers>
    <MinVerTagPrefix>v</MinVerTagPrefix>
    
    <!-- Symbol packages for debugging -->
    <IncludeSymbols>true</IncludeSymbols>
    <SymbolPackageFormat>snupkg</SymbolPackageFormat>
</PropertyGroup>
```

## Required Libraries

### Runtime Dependencies

#### HybridMicroOrm Package:
- **Dapper** (2.1.1 - 2.2): High-performance micro-ORM
- **Npgsql** (9.0 - 10.0): PostgreSQL .NET driver
- **Microsoft.Extensions.Options** (8.0.0 - 9.0.0): Configuration options pattern
- **Microsoft.Extensions.Logging** (implicit): Logging abstraction

#### Consumer Dependencies:
- **IJsonConverter Implementation**: You must provide an implementation using your preferred JSON serializer (e.g., Newtonsoft.Json, System.Text.Json)

#### Development Dependencies:
- **Microsoft.SourceLink.GitHub**: Source linking for debugging
- **MinVer**: Automatic semantic versioning

### Test Dependencies:
- **xUnit**: Test framework
- **Shouldly**: Fluent assertions
- **Microsoft.NET.Test.Sdk**: Test SDK
- **coverlet.collector**: Code coverage
- **MartinCostello.Logging.XUnit**: xUnit logging integration

### System Requirements:
- **.NET 8.0**: Target framework
- **PostgreSQL 12+**: Database (tested with PostgreSQL 15)
- **C# 12**: Language features

## Test Coverage

### Test Types

#### Integration Tests
- **Database Integration**: Full PostgreSQL testing with real database
- **Multi-Tenant Scenarios**: Comprehensive tenant isolation testing
- **CRUD Operations**: All Create, Read, Update, Delete operations
- **Edge Cases**: Error handling and boundary conditions

#### Test Categories
1. **Get Tests**: Record retrieval with various filters
2. **List Tests**: Querying with sorting and filtering
3. **Update Tests**: Record modification scenarios
4. **Delete Tests**: Both hard and soft delete operations
5. **Filter Tests**: Custom filtering capabilities
6. **DateTime Tests**: Audit trail functionality
7. **Multi-Tenant Tests**: Tenant isolation verification

#### Test Infrastructure
- **Base Test Classes**: Shared test infrastructure
- **Test Fixtures**: Database setup and teardown
- **Test Data**: Realistic domain objects (Cars, Customers, etc.)
- **Contexts**: Mock implementations of tenant and user contexts

### Coverage Reporting
- **Tool**: ReportGenerator with Cobertura format
- **Format**: HTML reports with historical tracking
- **CI Integration**: Automatic coverage artifact upload
- **History**: Coverage trends tracked across builds

### Example Test
```csharp
[Fact]
public async Task Test_tenant_record_can_be_read_with_tenant_context()
{
    // arrange
    var customerId = Guid.NewGuid();
    await ExecTenant1User1(x => x.Insert(CreateInsertRequest(customerId)));

    // act
    var record = await ExecTenant1User1(x => x.Get(customerId));

    // assert
    record.ShouldNotBeNull();
    record.Id.ShouldBe(customerId);
    record.TenantId.ShouldBe(Tenant1.TenantId);
    record.CreatedBy.ShouldBe(Tenant1.UserId1);
}
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## Support

- **Issues**: [GitHub Issues](https://github.com/PeterKneale/HybridMicroOrm/issues)
- **Documentation**: This README and inline code documentation
- **Examples**: See the test suite for comprehensive usage examples