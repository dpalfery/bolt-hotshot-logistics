# Code Quality Rule

Enforces comprehensive code quality practices for Hotshot Logistics, including build quality, input validation, and observability.

## When to Apply
Apply to all .NET development, ASP.NET Core APIs, and system monitoring implementations.

## Build Quality

### Zero Tolerance Policy
- Fix all build errors immediately - no exceptions.
- Resolve all warnings before merging or deploying.
- Treat warnings as errors in CI/CD to prevent technical debt.

### Configuration & Monitoring
- Set `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` and `<WarningsAsErrors />` in project files.
- Suppress specific warnings with `<WarningsNotAsErrors>CS1234</WarningsNotAsErrors>` only when documented.
- Implement CI/CD quality gates that fail on errors/warnings.
- Generate build reports with warning trends and alerts for degradation.

### StyleCop Rules
- Enable analyzers with StyleCop.Analyzers package.
- Use consistent stylecop.json and configure in .editorconfig or Directory.Build.props.
- Enforce mandatory categories: Spacing (SA1000-SA1999), Readability (SA2000-SA2999), Ordering (SA3000-SA3999), Maintainability (SA4000-SA4999), Layout (SA5000-SA5999), Documentation (SA6000-SA6999).
- Suppress rules only with `#pragma warning disable SA1234` and justification comments.
- Prefer code fixes over suppression.
- Review suppressions in code reviews.

### Development Workflow
- Configure IDEs to show violations as warnings/errors.
- Enable auto-format on save and use `dotnet format` in pre-commit hooks.

### Enforcement
- Local Development: Fail builds on errors, warnings, or violations.
- CI/CD Requirements: Generate and archive quality reports, block PRs with new warnings.
- Code Reviews: Verify zero warnings before approval, address all violations in PRs, document suppression justifications.

## Validation

### Input Validation
- Validate all user inputs on client and server sides.
- Use built-in ASP.NET Core validation attributes or FluentValidation.
- Never trust client data; validate and sanitize inputs.
- Implement comprehensive validation for endpoints accepting user input.

### Error Handling
- Return ProblemDetails consistently for error responses.
- Use appropriate HTTP status codes (400 for validation errors, 500 for server errors).
- Include meaningful error messages without exposing sensitive information.
- Standardize error response format across APIs.

### Validation Implementation
- Use Data Annotations for simple rules.
- Implement custom validation attributes for complex business rules.
- Validate inputs in controller actions or application services.
- Provide clear validation feedback to clients.

### Security Considerations
- Prevent injection attacks through validation.
- Validate file uploads and size limits.
- Implement rate limiting to prevent abuse.
- Log validation failures for monitoring.

## Observability

### Structured Logging
- Use `ILogger<T>` for structured logging with semantic values.
- Include correlation IDs for request tracing across services.
- Configure logging providers per environment (console, file, Application Insights, etc.).
- Log at appropriate levels: Debug, Information, Warning, Error, Critical.

### Metrics
- Implement metrics using Application Insights or custom counters.
- Track KPIs (response times, error rates, throughput).
- Use built-in ASP.NET Core metrics or custom collection.
- Configure exporters for monitoring dashboards.

### Tracing
- Implement distributed tracing for request flows across services.
- Use correlation IDs to link related operations.
- Configure exporters (Application Insights, Jaeger, etc.).
- Include contextual information in trace spans.

### Correlation IDs
- Generate and propagate correlation IDs for all requests.
- Include correlation IDs in logs, metrics, and traces.
- Use middleware for automatic propagation.
- Ensure inclusion in error responses.

### Implementation Guidelines
- Configure observability in DI container at startup.
- Use consistent naming for metrics and traces.
- Implement health checks including observability status.
- Log structured data with message templates and properties.

## Testing Standards and Coverage Requirements

### Unit Testing
- All business logic must be covered by unit tests (xUnit recommended).
- Integration tests should cover key application and infrastructure boundaries.
- Mock dependencies in application tests.
- Use FluentAssertions for assertions.
- Aim for 80%+ code coverage for Application and Domain layers.

### Integration Testing
- Test repository implementations against real databases.
- Use transactions for clean test state.
- Validate data access patterns and error handling.

### Coverage Requirements
- Minimum 80% code coverage for Application and Domain layers.
- Generate coverage reports in CI/CD.
- Block PRs with coverage below thresholds.

## Performance Benchmarking Procedures

### Benchmarking Tools
- Use BenchmarkDotNet for .NET performance testing.
- Implement custom benchmarks for database operations.
- Measure response times, throughput, and memory usage.

### Key Metrics
- API response times < 500ms for 95th percentile.
- Database query execution < 100ms.
- Memory usage within limits.

### Procedures
- Run benchmarks before and after changes.
- Compare results against baselines.
- Document performance regressions.

## Code Review Quality Gates

### Automated Checks
- Build passes without warnings.
- All tests pass.
- Code coverage meets requirements.
- Static analysis tools pass.

### Manual Review Checklist
- Architecture compliance.
- Code readability and maintainability.
- Security best practices.
- Documentation updates.

### Quality Metrics
- Cyclomatic complexity < 10.
- Method length < 30 lines.
- Class responsibility single.

## Technical Debt Management

### Identification
- Code smells, duplicated code, long methods.
- Outdated dependencies.
- Performance bottlenecks.

### Management Process
- Track debt in issue tracker.
- Prioritize based on impact.
- Allocate time for refactoring in sprints.

### Prevention
- Regular code reviews.
- Automated quality checks.
- Continuous refactoring.

## References
See 6-Docs/code-quality-examples.md for detailed examples.