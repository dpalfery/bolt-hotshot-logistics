# Optional Features Rule

Provides guidance for implementing optional features in ASP.NET Core applications for Hotshot Logistics.

## When to Apply
Apply when implementing advanced features like rate limiting, caching, or gRPC in ASP.NET Core services.

## Rate Limiting
- Use `Microsoft.AspNetCore.RateLimiting`.
- Implement per IP, user, or key-based limiting.
- Configure limits to prevent abuse.
- Use as security measure.

## Output Caching
- Implement for cacheable GET endpoints.
- Use `Microsoft.AspNetCore.OutputCaching`.
- Configure Redis for scale when needed.
- Cache safe responses to improve performance.

## gRPC
- Add health checks for gRPC services.
- Use `Grpc.AspNetCore.HealthChecks` package.
- Implement checks for gRPC endpoints.
- Ensure integration with orchestrators.