---
id: rules/resilience
title: Resilience Rule
doc-type: rule
status: current
owner: unassigned
last-reviewed: 2026-08-14
---

# Resilience Rule

This rule enforces resilience patterns for ASP.NET Core applications in the Hotshot Logistics project, ensuring robust handling of failures and external dependencies.

## When to Apply
Apply these practices whenever implementing external API calls, database operations, or other potentially failing operations in ASP.NET Core services.

## Polly Usage
- Use Polly for implementing resilience patterns (retry, circuit breaker, timeout).
- Configure policies for external HTTP/API calls. Database retries require separate review of transaction boundaries and operation idempotency.
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
