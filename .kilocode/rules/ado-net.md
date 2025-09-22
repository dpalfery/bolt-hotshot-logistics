# ADO.NET Rule

This rule enforces the exclusive use of native ADO.NET for all data access operations in the Hotshot Logistics project, prohibiting Entity Framework usage to maintain fine-grained control over SQL execution, performance optimization, and database operations.

## Core Requirements

### Entity Framework Prohibition
- **No Entity Framework dependencies**: Projects must not reference EntityFrameworkCore, EntityFramework, or any EF-related packages
- **No DbContext usage**: All database operations must use native ADO.NET classes (`SqlConnection`, `SqlCommand`, `SqlDataReader`, etc.)
- **No LINQ to Entities**: Database queries must be written as raw SQL or stored procedures
- **No migrations via EF**: Schema changes must use FluentMigrator exclusively

### ADO.NET Implementation Requirements
- **Native ADO.NET only**: Use `System.Data.SqlClient` or `Microsoft.Data.SqlClient` for SQL Server operations
- **Connection management**: Implement proper connection pooling and disposal patterns using `await using` statements
- **Parameterized queries**: All SQL queries must use parameterized commands to prevent SQL injection
- **Async operations**: All database operations must be asynchronous using `SqlConnection.OpenAsync()`, `SqlCommand.ExecuteReaderAsync()`, etc.
- **Repository pattern**: Encapsulate all data access logic in repository classes that implement interfaces defined in the Domain layer

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
- **Interface in Domain layer**: Define repository interfaces in `3-Domain/HotshotLogistics.Contracts/Repositories/`
- **Implementation in Persistence layer**: Implement repositories in `4-Persistence/HotshotLogistics.Data/Repositories/`
- **Base repository**: Use shared base repository class in `0-Base/HotshotLogistics.Core/Repositories/`
- **Dependency injection**: Register repositories in the DI container with proper lifetime management

### SQL Best Practices
- **Stored procedures**: Use stored procedures for complex operations and CRUD operations where appropriate
- **Query optimization**: Write efficient SQL with proper indexing considerations
- **Transaction management**: Use `SqlTransaction` for multi-statement operations requiring ACID compliance
- **Batch operations**: Use `SqlDataAdapter` or `SqlBulkCopy` for bulk operations when needed

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
- **Connection pooling**: Rely on built-in connection pooling rather than implementing custom pooling
- **Command reuse**: Reuse `SqlCommand` objects when executing similar queries
- **Reader efficiency**: Use `SqlDataReader` for forward-only, read-only access patterns
- **Batch size optimization**: Configure appropriate batch sizes for bulk operations
- **Query plan caching**: Design queries to benefit from SQL Server's query plan caching

## Integration with FluentMigrator

### Schema Management
- **No EF migrations**: All database schema changes must be implemented as FluentMigrator migrations
- **Migration-first approach**: Create migration scripts for all schema changes before implementing data access code
- **Rollback support**: Ensure all migrations support rollback operations
- **Seed data**: Include seed data in migrations for development and testing environments

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
- **Unit tests**: Mock repository interfaces for testing application services
- **Integration tests**: Test repository implementations against real database instances
- **Test data management**: Use FluentMigrator for test database setup and teardown
- **Transaction rollbacks**: Use transactions in integration tests to ensure clean state

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
- Build pipeline should fail if Entity Framework packages are detected
- Static analysis should flag direct database access outside repository pattern
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
