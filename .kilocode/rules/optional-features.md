# Optional Features Rule

This rule provides guidance on implementing optional features in ASP.NET Core applications for the Hotshot Logistics project.

## When to Apply
Apply these practices when implementing advanced features like rate limiting, caching, or gRPC in ASP.NET Core services.

## Rate Limiting
- Use `Microsoft.AspNetCore.RateLimiting` for rate limiting.
- Implement per IP, user, or key-based rate limiting.
- Configure appropriate limits to prevent abuse.
- Use rate limiting as a security measure.

## Output Caching
- Implement output/response caching for cacheable GET endpoints.
- Use `Microsoft.AspNetCore.OutputCaching` for response caching.
- Configure Redis provider for scale when needed.
- Cache responses where safe to improve performance.

## gRPC
- Add gRPC health checks if using gRPC services.
- Use `Grpc.AspNetCore.HealthChecks` package.
- Implement health checks for gRPC endpoints.
- Ensure proper health check integration with orchestrators.