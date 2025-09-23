# ADO.NET Rule

Enforces exclusive native ADO.NET use for data access, prohibiting Entity Framework to maintain SQL control, performance, and operations.

## Core Requirements

### Entity Framework Prohibition
- No Entity Framework dependencies: Avoid referencing EntityFrameworkCore, EntityFramework, or EF packages
- No DbContext usage: Use native ADO.NET classes (`SqlConnection`, `SqlCommand`, `SqlDataReader`, etc.)
- No LINQ to Entities: Write queries as raw SQL or stored procedures
- No EF migrations: Use FluentMigrator exclusively for schema changes

### ADO.NET Implementation Requirements
- Native ADO.NET only: Use `System.Data.SqlClient` or `Microsoft.Data.SqlClient` for SQL Server
- Connection management: Implement connection pooling and disposal with `await using`
- Parameterized queries: Use parameterized commands to prevent SQL injection
- Async operations: Use async methods like `SqlConnection.OpenAsync()`, `SqlCommand.ExecuteReaderAsync()`
- Repository pattern: Encapsulate data access in repository classes implementing Domain interfaces

## Implementation Guidelines

### Connection Management
```csharp
// Correct: Proper async connection management
public async Task<IEnumerable<Customer>> GetAllAsync()
{
    var customers = new List<Customer>();

    await using var connection = new SqlConnection(_connectionString);
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = "SELECT Id, Name, Email FROM Customers WHERE IsActive = @IsActive";
    command.Parameters.AddWithValue("@IsActive", true);

    await using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        customers.Add(new Customer
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Email = reader.GetString(2)
        });
    }

    return customers;
}
```

### Repository Pattern Structure
- Interface in Domain layer: Define in `3-Domain/HotshotLogistics.Contracts/Repositories/`
- Implementation in Persistence layer: Implement in `4-Persistence/HotshotLogistics.Data/Repositories/`
- Base repository: Use shared class in `0-Base/HotshotLogistics.Core/Repositories/`
- Dependency injection: Register in DI container with proper lifetimes

### SQL Best Practices
- Stored procedures: Use for complex and CRUD operations
- Query optimization: Write efficient SQL with indexing
- Transaction management: Use `SqlTransaction` for ACID-compliant multi-statement operations
- Batch operations: Use `SqlDataAdapter` or `SqlBulkCopy` for bulk data

### Error Handling
```csharp
// Correct: Proper error handling with specific exceptions
public async Task<Customer> GetByIdAsync(int id)
{
    try
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Email FROM Customers WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Customer
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2)
            };
        }

        throw new KeyNotFoundException($"Customer with ID {id} not found");
    }
    catch (SqlException ex)
    {
        _logger.LogError(ex, "Database error while retrieving customer {CustomerId}", id);
        throw new DataAccessException("Failed to retrieve customer", ex);
    }
}
```

### Performance Considerations
- Connection pooling: Rely on built-in pooling
- Command reuse: Reuse `SqlCommand` for similar queries
- Reader efficiency: Use `SqlDataReader` for forward-only, read-only access
- Batch size optimization: Configure appropriate sizes for bulk operations
- Query plan caching: Design queries for SQL Server caching

## Integration with FluentMigrator

### Schema Management
- No EF migrations: Implement all schema changes as FluentMigrator migrations
- Migration-first approach: Create migration scripts before data access code
- Rollback support: Ensure migrations support rollback
- Seed data: Include in migrations for development and testing

### Migration Structure
```csharp
// Example migration file in 4-Persistence/HotshotLogistics.Data/Migrations/
[Migration(20250121000000)]
public class CreateCustomersTable : Migration
{
    public override void Up()
    {
        Create.Table("Customers")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsString(255).NotNullable()
            .WithColumn("Email").AsString(255).NotNullable().Unique()
            .WithColumn("IsActive").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefaultValue(SystemMethods.CurrentDateTime);
    }

    public override void Down()
    {
        Delete.Table("Customers");
    }
}
```

## Testing Requirements

### Repository Testing
- Unit tests: Mock repository interfaces for application service testing
- Integration tests: Test implementations against real databases
- Test data management: Use FluentMigrator for setup and teardown
- Transaction rollbacks: Use transactions for clean integration test state

### Mock Implementation Example
```csharp
// For unit testing application services
public class MockCustomerRepository : ICustomerRepository
{
    private readonly List<Customer> _customers = new();

    public Task<Customer> GetByIdAsync(int id)
    {
        var customer = _customers.FirstOrDefault(c => c.Id == id);
        return Task.FromResult(customer ?? throw new KeyNotFoundException());
    }

    // Other methods...
}
```

## Enforcement

### Code Review Checklist
- [ ] No Entity Framework references in project files
- [ ] All database operations use native ADO.NET
- [ ] SQL queries are parameterized
- [ ] Async/await patterns used throughout
- [ ] Proper connection disposal with `await using`
- [ ] Repository pattern implemented correctly
- [ ] Error handling includes specific exception types
- [ ] Integration with FluentMigrator for schema changes

### Automated Checks
- Build pipeline fails if Entity Framework packages detected
- Static analysis flags direct database access outside repository pattern
- Code coverage requirements for repository classes
- Performance benchmarks for database operations

## Migration from Entity Framework

If converting existing EF code to ADO.NET:
1. Remove all Entity Framework package references
2. Create FluentMigrator migrations for existing schema
3. Implement repository interfaces and classes
4. Replace `DbContext` operations with ADO.NET equivalents
5. Update dependency injection configuration
6. Add comprehensive error handling
7. Create integration tests for new implementations
