# Observability Rule

This rule enforces observability practices for ASP.NET Core applications in the Hotshot Logistics project, ensuring comprehensive monitoring and debugging capabilities.

## When to Apply
Apply these practices whenever implementing logging, metrics, or tracing in ASP.NET Core services for monitoring and debugging.

## Structured Logging
- Use `ILogger<T>` for structured logging with semantic values.
- Include correlation IDs for request tracing across services.
- Configure logging providers per environment (console, file, Application Insights, etc.).
- Log at appropriate levels: Debug, Information, Warning, Error, Critical.

## Metrics
- Implement application metrics using Application Insights or custom counters.
- Track key performance indicators (response times, error rates, throughput).
- Use built-in ASP.NET Core metrics or custom metric collection.
- Configure metric exporters for monitoring dashboards.

## Tracing
- Implement distributed tracing for request flows across services.
- Use correlation IDs to link related operations.
- Configure tracing exporters (Application Insights, Jaeger, etc.).
- Include contextual information in trace spans.

## Correlation IDs
- Generate and propagate correlation IDs for all requests.
- Include correlation IDs in logs, metrics, and traces.
- Use middleware to automatically handle correlation ID propagation.
- Ensure correlation IDs are included in error responses.

## Implementation Guidelines
- Configure observability in the DI container during application startup.
- Use consistent naming conventions for metrics and traces.
- Implement health checks that include observability status.
- Log structured data using message templates and properties.