# Architecture Rule

Enforces Clean Architecture principles, native ADO.NET data access, and ASP.NET Core best practices in Hotshot Logistics.

## When to Apply

Apply when creating, moving, adding, or searching files to maintain separation of concerns and dependency direction, and when implementing data access or ASP.NET Core features.

## Clean Architecture Structure

### File Placement Guidelines

Place files in appropriate numbered folder based on purpose and architectural layer:

#### 0-Base/
**Shared foundational code and abstractions**
- Base classes, interfaces, utilities, extensions
- Cross-cutting concerns (logging, error handling, result types)
- Shared constants, enums, extension methods
- No dependencies on higher layers

#### 1-Presentation/
**Presentation layer projects and components**
- API controllers, functions, endpoints (Azure Functions)
- Web dashboard components (Next.js/React)
- Mobile app screens and navigation (Expo React Native)
- HTTP request/response handling, routing, UI rendering
- Depends only on Application layer

#### 2-Application/
**Core business logic and orchestration**
- Use cases, application services, business workflows
- Command/query handlers (CQRS)
- Business rules, validation logic, policies
- DTOs for layer communication
- Depends only on Domain layer

#### 3-Domain/
**Domain models, contracts, and interfaces**
- Entity definitions, value objects, domain models
- Repository interfaces, service contracts
- Domain events, specifications
- No dependencies on other layers

#### 4-Persistence/
**Data access implementations**
- Native ADO.NET implementations with schema and migrations managed by FluentMigrator
- Repository implementations using ADO.NET (parameterized SQL, async operations, proper disposal)
- Database seeding, external API integrations
- Implements Domain contracts

#### 5-Test/
**Testing infrastructure and test files**
- Unit tests, integration tests, architecture tests
- Test utilities, fixtures, mock data
- Test configurations and helpers
- Uses xUnit, FluentAssertions

#### 6-Docs/
**Documentation and specifications**
- README files, API documentation, guides
- Technical specifications, architecture docs
- Development workflows, contributing guidelines

#### 7-Deployment/
**Infrastructure and deployment assets**
- CI/CD pipelines, Docker configurations
- Infrastructure as Code (Terraform, Bicep, Ansible)
- Deployment scripts, environment configurations

### Search Guidelines

When searching files:
1. Search within most relevant architectural layer based on query context
2. If not found, expand to related layers following dependency direction
3. Use folder prefixes (e.g., "2-Application/") to narrow searches
4. For cross-cutting concerns, check 0-Base/ first

### Enforcement
- Place new files in correct folder immediately upon creation
- File moves must maintain architectural integrity
- Regular audits verify adherence to this structure
- Exceptions require explicit architectural review and documentation

## Data Access (ADO.NET)

See [ado-net.md](ado-net.md) for comprehensive data access rules, including:
- Native ADO.NET implementation requirements
- Repository pattern standards
- Performance optimization guidelines
- Integration with FluentMigrator

## ASP.NET Core Practices

### Configuration
- Centralize settings with Options pattern and DI.
- Use `IOptions<T>`, `IOptionsSnapshot<T>`, `IOptionsMonitor<T>` for binding.
- Override via `appsettings.{Environment}.json` and environment variables.
- Never hardcode values.

### Logging
- Use `ILogger<T>` for structured logging with semantic values.
- Include correlation IDs for tracing.
- Configure providers per environment (console, file, Application Insights, etc.).
- Log at levels: Debug, Information, Warning, Error, Critical.

### API Documentation
- Generate OpenAPI specs with `Microsoft.AspNetCore.OpenApi`.
- Provide Swagger UI via Swashbuckle.
- Version APIs with URL or header versioning.
- Include XML comments on public APIs.

### Middleware Order
Configure middleware in this order:
1. `UseHttpsRedirection` - Redirect HTTP to HTTPS
2. `UseCors` - Enable CORS
3. `UseRateLimiter` - Apply rate limiting
4. `UseAuthentication` - Authenticate requests
5. `UseAuthorization` - Authorize requests
6. `UseOutputCaching` or `UseResponseCaching` - Cache responses where safe
7. Endpoint routing

### Performance Practices
- Use async/await for all I/O operations.
- Reuse HttpClient via `IHttpClientFactory`.
- Implement caching for cacheable GET endpoints.
- Use rate limiting to prevent abuse.
- Measure performance with diagnostics and Application Insights.

### Health Checks
- Implement `/health` endpoints with database, queue, API checks.
- Integrate with orchestrators like Kubernetes for readiness/liveness probes.
- Use `Microsoft.AspNetCore.Diagnostics.HealthChecks` package.

## References

- [ADO.NET Rules](ado-net.md) - Data access implementation standards
- [Code Quality Rules](code-quality.md) - Build and testing standards
- [Security Rules](security.md) - Security implementation guidelines

For detailed implementation examples, see 6-Docs/architecture-examples.md.