# Data Access Standards

This project enforces the exclusive use of native ADO.NET for all data access operations, prohibiting Entity Framework usage to maintain fine-grained control over SQL execution, performance optimization, and database operations.

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

## Integration with FluentMigrator

### Schema Management
- **No EF migrations**: All database schema changes must be implemented as FluentMigrator migrations
- **Migration-first approach**: Create migration scripts for all schema changes before implementing data access code
- **Rollback support**: Ensure all migrations support rollback operations
- **Seed data**: Include seed data in migrations for development and testing environments