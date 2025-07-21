using System.Text.Json;
using HybridMicroOrm.Extensions;
using Npgsql;

namespace HybridMicroOrm.Examples;

/// <summary>
/// Example domain model for testing
/// </summary>
public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime CreatedDate { get; set; }
    public string Status { get; set; } = "Active";
}

/// <summary>
/// Example program demonstrating extension methods approach
/// </summary>
public class ExtensionMethodsExample
{
    private readonly string _connectionString;

    public ExtensionMethodsExample(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task RunExampleAsync()
    {
        Console.WriteLine("=== HybridMicroOrm Extension Methods Example ===\n");

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        // Configure options for our tenant and user
        var options = new HybridRecordOptions
        {
            TableName = "example_records",
            TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            UserId = Guid.Parse("22222222-2222-2222-2222-222222222222")
        };

        try
        {
            // 1. Create table and indexes
            Console.WriteLine("1. Creating table and indexes...");
            await connection.CreateHybridRecordsTable(options);
            Console.WriteLine("   ✓ Table created successfully\n");

            // 2. Insert some test data
            Console.WriteLine("2. Inserting test customers...");
            var customers = new[]
            {
                new Customer { Id = Guid.NewGuid(), Name = "John Doe", Email = "john@example.com", CreatedDate = DateTime.UtcNow.AddDays(-10) },
                new Customer { Id = Guid.NewGuid(), Name = "Jane Smith", Email = "jane@example.com", CreatedDate = DateTime.UtcNow.AddDays(-5) },
                new Customer { Id = Guid.NewGuid(), Name = "Bob Johnson", Email = "bob@example.com", CreatedDate = DateTime.UtcNow.AddDays(-1) }
            };

            foreach (var customer in customers)
            {
                await connection.InsertHybridRecord(customer.Id, "customer", customer, options);
                Console.WriteLine($"   ✓ Inserted customer: {customer.Name} ({customer.Id})");
            }
            Console.WriteLine();

            // 3. Retrieve individual records
            Console.WriteLine("3. Retrieving individual records...");
            var retrievedCustomer = await connection.GetHybridRecord<Customer>(customers[0].Id, options);
            if (retrievedCustomer != null)
            {
                Console.WriteLine($"   ✓ Retrieved: {retrievedCustomer.Data.Name} (Created: {retrievedCustomer.CreatedAt})");
                Console.WriteLine($"     Tenant: {retrievedCustomer.TenantId}, Created By: {retrievedCustomer.CreatedBy}");
            }
            Console.WriteLine();

            // 4. List all customers
            Console.WriteLine("4. Listing all customers...");
            var allCustomers = await connection.ListHybridRecords<Customer>("customer", options);
            foreach (var record in allCustomers)
            {
                Console.WriteLine($"   • {record.Data.Name} - {record.Data.Email} (ID: {record.Id})");
            }
            Console.WriteLine();

            // 5. List with filtering
            Console.WriteLine("5. Filtering customers by name containing 'John'...");
            var filteredCustomers = await connection.ListHybridRecords<Customer>(
                type: "customer",
                options: options,
                whereClause: "data->>'name' ILIKE @NameFilter",
                parameters: new { NameFilter = "%John%" }
            );
            foreach (var record in filteredCustomers)
            {
                Console.WriteLine($"   • {record.Data.Name} - {record.Data.Email}");
            }
            Console.WriteLine();

            // 6. Update a record
            Console.WriteLine("6. Updating customer email...");
            var customerToUpdate = customers[0];
            customerToUpdate.Email = "john.doe.updated@example.com";
            await connection.UpdateHybridRecord(customerToUpdate.Id, "customer", customerToUpdate, options);
            
            var updatedRecord = await connection.GetHybridRecord<Customer>(customerToUpdate.Id, options);
            if (updatedRecord != null)
            {
                Console.WriteLine($"   ✓ Updated email: {updatedRecord.Data.Email}");
                Console.WriteLine($"     Updated at: {updatedRecord.UpdatedAt}, By: {updatedRecord.UpdatedBy}");
            }
            Console.WriteLine();

            // 7. Check existence
            Console.WriteLine("7. Checking record existence...");
            var exists = await connection.HybridRecordExists(customerToUpdate.Id, options);
            Console.WriteLine($"   ✓ Customer {customerToUpdate.Id} exists: {exists}");
            
            var nonExistentId = Guid.NewGuid();
            var notExists = await connection.HybridRecordExists(nonExistentId, options);
            Console.WriteLine($"   ✓ Random ID {nonExistentId} exists: {notExists}");
            Console.WriteLine();

            // 8. Soft delete
            Console.WriteLine("8. Soft deleting a customer...");
            await connection.SoftDeleteHybridRecord(customers[1].Id, options);
            
            var allAfterSoftDelete = await connection.ListHybridRecords<Customer>("customer", options);
            Console.WriteLine($"   ✓ Active customers after soft delete: {allAfterSoftDelete.Count()}");
            
            var includingDeleted = await connection.ListHybridRecords<Customer>("customer", options, includeDeleted: true);
            Console.WriteLine($"   ✓ Total customers including deleted: {includingDeleted.Count()}");
            Console.WriteLine();

            // 9. Demonstrate custom table schema
            Console.WriteLine("9. Custom table schema example...");
            var customOptions = new HybridRecordOptions
            {
                TableName = "documents",
                IdColumn = "doc_id",
                TenantIdColumn = "company_id",
                TypeColumn = "doc_type",
                DataColumn = "content",
                TenantId = options.TenantId,
                UserId = options.UserId
            };

            await connection.CreateHybridRecordsTable(customOptions);
            
            var document = new { Title = "Important Document", Content = "This is a test document" };
            var docId = Guid.NewGuid();
            await connection.InsertHybridRecord(docId, "contract", document, customOptions);
            
            var retrievedDoc = await connection.GetHybridRecord<dynamic>(docId, customOptions);
            Console.WriteLine($"   ✓ Created document in custom table: {retrievedDoc?.Data.Title}");
            Console.WriteLine();

            // 10. Demonstrate custom JSON serialization (using System.Text.Json with different options)
            Console.WriteLine("10. Custom JSON serialization example...");
            var customJsonOptions = new HybridRecordOptions
            {
                TableName = "json_test_records",
                TenantId = options.TenantId,
                UserId = options.UserId,
                JsonSerializer = obj => JsonSerializer.Serialize(obj, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                    WriteIndented = true 
                }),
                JsonDeserializer = (json, type) => JsonSerializer.Deserialize(json, type, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower 
                })!
            };

            await connection.CreateHybridRecordsTable(customJsonOptions);
            
            var testCustomer = new Customer { Id = Guid.NewGuid(), Name = "Custom JSON Test", Email = "test@json.com" };
            await connection.InsertHybridRecord(testCustomer.Id, "customer", testCustomer, customJsonOptions);
            
            var retrievedJsonCustomer = await connection.GetHybridRecord<Customer>(testCustomer.Id, customJsonOptions);
            Console.WriteLine($"   ✓ Custom JSON customer: {retrievedJsonCustomer?.Data.Name}");
            Console.WriteLine();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Inner: {ex.InnerException.Message}");
            }
        }
        finally
        {
            // Cleanup - drop test tables
            Console.WriteLine("Cleaning up test tables...");
            try
            {
                await connection.DropHybridRecordsTable(options);
                await connection.DropHybridRecordsTable(new HybridRecordOptions { TableName = "documents" });
                await connection.DropHybridRecordsTable(new HybridRecordOptions { TableName = "json_test_records" });
                Console.WriteLine("✓ Cleanup completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Cleanup warning: {ex.Message}");
            }
        }
    }
}

/// <summary>
/// Performance comparison between approaches
/// </summary>
public class PerformanceComparison
{
    private readonly string _connectionString;

    public PerformanceComparison(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task RunComparisonAsync()
    {
        Console.WriteLine("\n=== Performance Comparison ===\n");

        using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        var options = new HybridRecordOptions
        {
            TableName = "perf_test_records",
            TenantId = Guid.NewGuid(),
            UserId = Guid.NewGuid()
        };

        try
        {
            await connection.CreateHybridRecordsTable(options);

            const int recordCount = 1000;
            var customers = Enumerable.Range(1, recordCount)
                .Select(i => new Customer 
                { 
                    Id = Guid.NewGuid(), 
                    Name = $"Customer {i}", 
                    Email = $"customer{i}@example.com",
                    CreatedDate = DateTime.UtcNow.AddDays(-i)
                })
                .ToArray();

            // Test bulk insert performance
            Console.WriteLine($"Inserting {recordCount} records...");
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            foreach (var customer in customers)
            {
                await connection.InsertHybridRecord(customer.Id, "customer", customer, options);
            }

            stopwatch.Stop();
            Console.WriteLine($"✓ Inserted {recordCount} records in {stopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine($"  Average: {(double)stopwatch.ElapsedMilliseconds / recordCount:F2}ms per record");

            // Test query performance
            stopwatch.Restart();
            var allRecords = await connection.ListHybridRecords<Customer>("customer", options);
            stopwatch.Stop();
            
            Console.WriteLine($"✓ Retrieved {allRecords.Count()} records in {stopwatch.ElapsedMilliseconds}ms");

            // Test filtered query performance
            stopwatch.Restart();
            var filteredRecords = await connection.ListHybridRecords<Customer>(
                "customer", 
                options,
                whereClause: "data->>'name' ILIKE @Filter",
                parameters: new { Filter = "%1%" }
            );
            stopwatch.Stop();
            
            Console.WriteLine($"✓ Filtered query returned {filteredRecords.Count()} records in {stopwatch.ElapsedMilliseconds}ms");
        }
        finally
        {
            await connection.DropHybridRecordsTable(options);
        }
    }
}

/// <summary>
/// Main program to run examples
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        // Default connection string for testing - modify as needed
        var connectionString = args.Length > 0 
            ? args[0] 
            : "Host=localhost;Database=hybridorm_test;Username=postgres;Password=postgres";

        Console.WriteLine("HybridMicroOrm Extension Methods Demo");
        Console.WriteLine("====================================");
        Console.WriteLine($"Connection: {connectionString.Split(';')[0]}");
        Console.WriteLine();

        try
        {
            // Run main example
            var example = new ExtensionMethodsExample(connectionString);
            await example.RunExampleAsync();

            // Run performance comparison
            var performance = new PerformanceComparison(connectionString);
            await performance.RunComparisonAsync();

            Console.WriteLine("\n🎉 Demo completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Demo failed: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"   Inner: {ex.InnerException.Message}");
            }
            Environment.Exit(1);
        }
    }
}