# Copilot Instructions for HybridMicroOrm

## Project Overview
HybridMicroOrm is a .NET 8 micro-ORM for PostgreSQL, designed for SaaS and multi-tenant applications. It stores strongly-typed C# objects as JSONB, with built-in tenant isolation, audit trails, and flexible filtering. The codebase is split into two NuGet packages:
- **HybridMicroOrm**: Main implementation (depends on Dapper, Npgsql, Microsoft.Extensions.*)
- **HybridMicroOrm.Contracts**: Interfaces and contracts (no dependencies)

## Architecture & Key Patterns
- **Multi-Tenancy**: All queries are filtered by tenant via `ITenantContext`. Shared data is supported by setting `isTenantData: false` on insert.
- **JSON Storage**: All domain objects are serialized via a user-supplied `IJsonConverter` (see `HybridMicroOrm.Contracts`).
- **Audit Trail**: `CreatedAt`, `UpdatedAt`, `DeletedAt` and user IDs are tracked automatically.
- **Soft Deletes**: Records are marked as deleted, not removed. Use `IncludeDeleted` in queries to access them.
- **Service Registration**: Consumers must register `IJsonConverter`, `ITenantContext`, and `IUserContext` in DI. See `README.md` for code samples.
- **Table Schema**: All data is stored in a single table (default: `records`) with columns for `id`, `tenant_id`, `type`, `data`, and audit fields.

## Developer Workflows
- **Build**: `dotnet build` (solution file: `HybridMicroOrm.sln`)
- **Test**: `dotnet test` (integration tests require PostgreSQL, see below)
- **Validate Packages**: `./scripts/validate-packages.sh` (runs local package validation)
- **Pack**: `dotnet pack --configuration Release`
- **Versioning**: Uses [MinVer](https://github.com/adamralph/minver) for Git-based semantic versioning. Tag releases as `vX.Y.Z`.
- **CI/CD**: GitHub Actions runs full build, test, and publish pipelines. See `README.md` for details.

## Testing & Integration
- **Integration Tests**: Located in `tests/HybridMicroOrm.Tests/`. Use xUnit, Shouldly, and run against a real PostgreSQL instance (see `mcp.json` for Docker setup).
- **Test Scenarios**: Multi-tenant and multi-user scenarios are modeled in `TestScenarios/` and `Fixtures/`.
- **Test Data**: Domain objects for tests are in `TestData/`.
- **Coverage**: Coverage is collected via coverlet and ReportGenerator.

## Project-Specific Conventions
- **Insert/Update**: Use static `Create` methods on request objects (e.g., `InsertRequest.Create(...)`).
- **Filtering**: Use the `Filter` object for custom SQL filters in queries.
- **Sorting**: Use `SortBy` and `SortOrder` enums; see `Requests.cs` for extension methods.
- **Tenant/User Context**: Always resolve tenant/user from DI, never hardcode.
- **No Built-in JSON Serializer**: Consumers must provide their own `IJsonConverter` implementation.

## Key Files & Directories
- `src/HybridMicroOrm/` — Main implementation
- `src/HybridMicroOrm.Contracts/` — Interfaces, contracts, and request/response types
- `tests/HybridMicroOrm.Tests/` — Integration tests, fixtures, and scenarios
- `scripts/validate-packages.sh` — Local package validation
- `README.md` — Full documentation and usage examples
- `PACKAGING.md` — NuGet packaging and publishing details

## Examples
- See `README.md` for service registration, usage, and test examples.
- See `tests/HybridMicroOrm.Tests/` for multi-tenant test patterns and data setup.

---
For any unclear conventions or missing details, consult the `README.md` or open an issue.
