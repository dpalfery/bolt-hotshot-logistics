# Observability Rule

Enforces observability practices for ASP.NET Core applications in Hotshot Logistics, ensuring comprehensive monitoring and debugging.

## When to Apply
Apply when implementing logging, metrics, or tracing in ASP.NET Core services for monitoring and debugging.

## Structured Logging
- Use `ILogger<T>` for structured logging with semantic values.
- Include correlation IDs for request tracing across services.
- Configure logging providers per environment (console, file, Application Insights, etc.).
- Log at appropriate levels: Debug, Information, Warning, Error, Critical.

## Metrics
- Implement metrics using Application Insights or custom counters.
- Track KPIs (response times, error rates, throughput).
- Use built-in ASP.NET Core metrics or custom collection.
- Configure exporters for monitoring dashboards.

## Tracing
- Implement distributed tracing for request flows across services.
- Use correlation IDs to link related operations.
- Configure exporters (Application Insights, Jaeger, etc.).
- Include contextual information in trace spans.

## Correlation IDs
- Generate and propagate correlation IDs for all requests.
- Include correlation IDs in logs, metrics, and traces.
- Use middleware for automatic propagation.
- Ensure inclusion in error responses.

## Implementation Guidelines
- Configure observability in DI container at startup.
- Use consistent naming for metrics and traces.
- Implement health checks including observability status.
- Log structured data with message templates and properties.