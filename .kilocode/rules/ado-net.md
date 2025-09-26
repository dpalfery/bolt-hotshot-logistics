# ADO.NET Rule

Enforces exclusive native ADO.NET use for data access, prohibiting Entity Framework to maintain SQL control, performance, and operations.

## When to Apply

Apply when implementing data access patterns, database operations, or repository implementations in the persistence layer.

## Core Requirements

### Entity Framework Prohibition
- No Entity Framework dependencies: Avoid referencing EntityFrameworkCore, EntityFramework, or EF packages
- No DbContext usage: Use native ADO.NET classes (`SqlConnection`, `SqlCommand`, `SqlDataReader`, etc.)
- No LINQ to Entities: Write queries as raw SQL or stored procedures
- No EF migrations: Use FluentMigrator exclusively for schema changes

### ADO.NET Implementation Requirements
- Native ADO.NET only: Use `Microsoft.Data.SqlClient` for SQL Server
- Connection management: Implement connection pooling and disposal with `await using`
- Parameterized queries: Use parameterized commands to prevent SQL injection
- Async operations: Use async methods like `SqlConnection.OpenAsync()`, `SqlCommand.ExecuteReaderAsync()`
- Repository pattern: Encapsulate data access in repository classes implementing Domain interfaces

## Implementation Guidelines

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

## Testing Requirements

### Repository Testing
- Unit tests: Mock repository interfaces for application service testing
- Integration tests: Test implementations against real databases
- Test data management: Use FluentMigrator for setup and teardown
- Transaction rollbacks: Use transactions for clean integration test state

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

## References

See 6-Docs/ado-net-examples.md for detailed examples.
