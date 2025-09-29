name: "Code-Quality-Rule"
description: "Enforces comprehensive code quality practices that apply across all development activities in Hotshot Logistics."
when-to-apply:
"Apply to all development activities, regardless of technology stack or architectural layer."
rule: |

## Build Quality

### Zero Tolerance Policy
- Fix all build errors immediately - no exceptions.
- Resolve all warnings before merging or deploying.
- Treat warnings as errors in CI/CD to prevent technical debt.

### Configuration & Monitoring
- Set warning and error policies in project configuration files.
- Suppress specific warnings only when documented with justification.
- Implement CI/CD quality gates that fail on errors/warnings.
- Generate build reports with warning trends and alerts for degradation.

### Development Workflow
- Configure IDEs to show violations as warnings/errors.
- Enable auto-format on save and use formatting tools in pre-commit hooks.

### Enforcement
- Local Development: Fail builds on errors, warnings, or violations.
- CI/CD Requirements: Generate and archive quality reports, block PRs with new warnings.
- Code Reviews: Verify zero warnings before approval, address all violations in PRs, document suppression justifications.

## Validation

### Input Validation
- Validate all user inputs on client and server sides.
- Never trust client data; validate and sanitize inputs.
- Implement comprehensive validation for all user-facing endpoints.

### Error Handling
- Return consistent error responses across all APIs.
- Use appropriate status codes for different error types.
- Include meaningful error messages without exposing sensitive information.
- Standardize error response format across all platforms.

### Security Considerations
- Prevent injection attacks through validation.
- Validate file uploads and size limits.
- Implement rate limiting to prevent abuse.
- Log validation failures for monitoring.

## Observability

### Structured Logging
- Use structured logging with semantic values and correlation IDs.
- Configure logging providers per environment.
- Log at appropriate levels: Debug, Information, Warning, Error, Critical.
- Include correlation IDs for request tracing across services.

### Metrics
- Implement metrics collection and monitoring.
- Track KPIs (response times, error rates, throughput).
- Configure exporters for monitoring dashboards.

### Tracing
- Implement distributed tracing for request flows across services.
- Use correlation IDs to link related operations.
- Include contextual information in trace spans.

### Implementation Guidelines
- Configure observability in application startup.
- Use consistent naming for metrics and traces.
- Implement health checks including observability status.
- Log structured data with message templates and properties.

## Testing Standards and Coverage Requirements

### Test Coverage Standards
- **Application and Domain layers**: Minimum 80% code coverage
- **Infrastructure/Persistence layer**: Minimum 70% code coverage
- **Presentation layer**: Minimum 60% code coverage
- **Frontend components**: Minimum 70% coverage for critical user interactions
- **Mobile app**: Minimum 65% coverage for core functionality

### Test Organization
- Place tests in appropriate test directories following project structure
- Use descriptive test class and method names
- Group related tests in test classes
- Separate unit, integration, and performance tests

### Test Data Management
- Use factory methods for test data creation
- Implement builder patterns for complex objects
- Avoid hard-coded test data; use data builders
- Clean up test resources in test teardown

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
- Cyclomatic complexity within acceptable limits.
- Method length appropriate for technology.
- Class responsibility focused and single-purpose.

## Technical Debt Management

### Identification
- Code smells, duplicated code, long methods.
- Outdated dependencies.
- Performance bottlenecks.

### Management Process
- Track debt in issue tracker.
- Prioritize based on impact.
- Allocate time for refactoring in development cycles.

### Prevention
- Regular code reviews.
- Automated quality checks.
- Continuous refactoring.

