# Testing Rule

Enforces comprehensive testing standards for Hotshot Logistics, ensuring reliability, maintainability, and performance across all components.

## When to Apply

Apply when writing, executing, or reviewing tests for all components of the Hotshot Logistics platform, including backend APIs, frontend dashboards, mobile applications, and infrastructure components.

## Core Requirements

### Test Coverage Standards
- **Application and Domain layers**: Minimum 80% code coverage
- **Infrastructure/Persistence layer**: Minimum 70% code coverage
- **Presentation layer**: Minimum 60% code coverage (focus on business logic, not framework boilerplate)
- **Frontend components**: Minimum 70% coverage for critical user interactions
- **Mobile app**: Minimum 65% coverage for core functionality

### Test Organization
- Place tests in `5-Test/tests/` directory following project structure
- Use descriptive test class and method names
- Group related tests in test classes
- Separate unit, integration, and performance tests

## Unit Testing

### Implementation Standards
- All business logic must be covered by unit tests (xUnit recommended)
- Mock dependencies using Moq or similar frameworks
- Use FluentAssertions for readable assertions
- Test both happy path and error scenarios
- Follow AAA pattern (Arrange, Act, Assert)

### Mocking Strategies
- Mock repository interfaces for service testing
- Use test doubles for external API calls
- Avoid over-mocking; test real behavior where possible
- Create reusable mock setups for common scenarios

### Test Data Management
- Use factory methods for test data creation
- Implement builder patterns for complex objects
- Avoid hard-coded test data; use data builders
- Clean up test resources in test teardown

## Integration Testing

### Database Testing Patterns
- Test repository implementations against real databases
- Use transactions to ensure test isolation
- Validate CRUD operations and complex queries
- Test stored procedure execution and error handling
- Use FluentMigrator for test database setup

### External Service Integration Tests
- Mock external APIs for unit tests
- Use test containers (Testcontainers) for isolated integration tests
- Test error scenarios (timeouts, network failures)
- Validate data serialization and deserialization
- Test authentication and authorization flows

### End-to-End API Testing
- Test complete request-response cycles
- Use tools like Playwright or RestSharp for API testing
- Validate HTTP status codes and response formats
- Test authentication middleware and security
- Include performance assertions in E2E tests

## Performance Testing

### Load Testing Procedures
- Use k6 or JMeter for load testing scenarios
- Define realistic user load patterns and ramp-up strategies
- Monitor system metrics (CPU, memory, network)
- Test under various conditions (normal, peak, stress)
- Establish baseline performance metrics

### Database Performance Benchmarks
- Benchmark query execution times using BenchmarkDotNet
- Test under concurrent user loads
- Monitor database-specific metrics (query plans, locks, deadlocks)
- Identify and optimize N+1 query problems
- Validate indexing strategies

### API Response Time Requirements
- API endpoints: < 500ms for 95th percentile
- Database queries: < 100ms average execution time
- Static asset delivery: < 200ms
- Real-time operations (SignalR): < 100ms
- Mobile API calls: < 300ms for 95th percentile

## Testing Frameworks and Tools

### Backend Testing
- **Unit Testing**: xUnit with Moq and FluentAssertions
- **Integration Testing**: xUnit with Testcontainers
- **Performance Testing**: BenchmarkDotNet, k6
- **API Testing**: Playwright, RestSharp

### Frontend Testing
- **Unit Testing**: Jest with React Testing Library
- **Integration Testing**: Cypress or Playwright
- **E2E Testing**: Playwright for cross-browser testing

### Mobile Testing
- **Unit Testing**: Jest for JavaScript logic
- **Integration Testing**: Detox for React Native
- **UI Testing**: Appium for device testing

## Test Execution and Automation

### Local Development
- Run unit tests: `dotnet test --filter "Category=Unit"`
- Run integration tests: `dotnet test --filter "Category=Integration"`
- Run performance tests: `dotnet test --filter "Category=Performance"`
- Generate coverage reports: `dotnet test /p:CollectCoverage=true`

### CI/CD Integration
- Execute all tests on every pull request
- Run tests in parallel for faster feedback
- Generate test reports and coverage artifacts
- Block deployments if tests fail or coverage drops

## Quality Gates and Enforcement

### Code Review Requirements
- All new features require corresponding tests
- Review test quality and coverage in PRs
- Ensure tests are maintainable and readable
- Validate test naming conventions

### Build Pipeline Checks
- Fail builds on test failures
- Enforce minimum coverage thresholds
- Generate coverage trend reports
- Alert on flaky or slow tests

### Test Maintenance
- Regularly review and update test suites
- Remove obsolete tests
- Refactor tests with code changes
- Monitor test execution times

## Test Data Management

### Seed Data Strategy
- Use FluentMigrator for consistent test data seeding
- Create reusable seed data scripts
- Separate development and test seed data
- Version seed data with migrations

### Test Isolation
- Use unique data per test to avoid interference
- Implement proper cleanup in test teardown
- Use database transactions for rollback
- Avoid shared state between tests

## Performance Benchmarking

### Benchmarking Tools
- Use BenchmarkDotNet for .NET performance testing
- Implement custom benchmarks for database operations
- Measure response times, throughput, and memory usage
- Compare results against established baselines

### Key Metrics
- API response times under load
- Database query performance
- Memory usage patterns
- CPU utilization during peak loads

### Procedures
- Run benchmarks before and after changes
- Document performance regressions
- Establish performance budgets
- Include benchmarks in CI/CD pipeline

## References

See 6-Docs/testing-examples.md for detailed implementation examples and test patterns.
See [Code Quality Rules](code-quality.md) for additional testing standards.
See [Process Rules](process.md) for testing execution workflows.