# ASP.NET Core Practices Rule

Enforces best practices for ASP.NET Core applications in Hotshot Logistics, ensuring secure, performant, maintainable services.

## When to Apply
Apply when developing/modifying ASP.NET Core APIs, controllers, services in .NET 8+ projects.

## Configuration
- Centralize settings with Options pattern and DI.
- Use `IOptions<T>`, `IOptionsSnapshot<T>`, `IOptionsMonitor<T>` for binding.
- Override via `appsettings.{Environment}.json` and environment variables.
- Never hardcode values.

## Logging
- Use `ILogger<T>` for structured logging with semantic values.
- Include correlation IDs for tracing.
- Configure providers per environment (console, file, Application Insights, etc.).
- Log at levels: Debug, Information, Warning, Error, Critical.

## API Documentation
- Generate OpenAPI specs with `Microsoft.AspNetCore.OpenApi`.
- Provide Swagger UI via Swashbuckle.
- Version APIs with URL or header versioning.
- Include XML comments on public APIs.

## Middleware Order
Configure middleware in this order:
1. `UseHttpsRedirection` - Redirect HTTP to HTTPS
2. `UseCors` - Enable CORS
3. `UseRateLimiter` - Apply rate limiting
4. `UseAuthentication` - Authenticate requests
5. `UseAuthorization` - Authorize requests
6. `UseOutputCaching` or `UseResponseCaching` - Cache responses where safe
7. Endpoint routing

## Performance Practices
- Use async/await for all I/O operations.
- Reuse HttpClient via `IHttpClientFactory`.
- Implement caching for cacheable GET endpoints.
- Use rate limiting to prevent abuse.
- Measure performance with diagnostics and Application Insights.

## Health Checks
- Implement `/health` endpoints with database, queue, API checks.
- Integrate with orchestrators like Kubernetes for readiness/liveness probes.
- Use `Microsoft.AspNetCore.Diagnostics.HealthChecks` package.

## References
- [ASP.NET Core Configuration](https://learn.microsoft.com/aspnet/core/fundamentals/configuration)
- [ASP.NET Core Logging](https://learn.microsoft.com/aspnet/core/fundamentals/logging)
- [ASP.NET Core OpenAPI](https://learn.microsoft.com/aspnet/core/fundamentals/openapi)
- [ASP.NET Core Middleware](https://learn.microsoft.com/aspnet/core/fundamentals/middleware)
- [ASP.NET Core Health Checks](https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks)