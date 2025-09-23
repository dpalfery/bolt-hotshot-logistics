# Resilience Rule

Enforces resilience patterns for ASP.NET Core applications in Hotshot Logistics, ensuring robust failure and external dependency handling.

## When to Apply
Apply when implementing external API calls, database operations, or other potentially failing operations in ASP.NET Core services.

## Polly Usage
- Use Polly for resilience patterns (retry, circuit breaker, timeout).
- Configure policies for database connections and external API calls.
- Define appropriate retry counts and backoff strategies.
- Implement circuit breakers to prevent cascading failures.

## Timeouts
- Set reasonable timeouts for all external operations.
- Use `CancellationToken` for cooperative cancellation.
- Configure timeouts at the HttpClient level using `IHttpClientFactory`.

## Retries
- Implement exponential backoff for transient failures.
- Distinguish between retryable and non-retryable errors.
- Limit retry attempts to prevent resource exhaustion.

## Circuit Breakers
- Use circuit breakers to fail fast during prolonged outages.
- Configure appropriate thresholds for opening/closing circuits.
- Implement fallback behaviors when circuits are open.

## Implementation Guidelines
- Register Polly policies in the DI container.
- Apply policies to HttpClient instances via `IHttpClientFactory`.
- Use typed clients for better testability and configuration.
- Log resilience events for monitoring and debugging.