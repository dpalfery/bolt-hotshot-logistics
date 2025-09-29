# Code Quality Examples

This document provides detailed examples for implementing code quality practices in Hotshot Logistics.

## Build Quality Examples

### Local Development Configuration
```xml
<PropertyGroup>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <WarningsAsErrors />
</PropertyGroup>
```

This configuration ensures that warnings are treated as errors during local builds, preventing technical debt accumulation.

## Validation Examples

### Data Annotations Example
```csharp
public class CustomerDto
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; }
}
```

### Custom Validation Attribute
```csharp
public class FutureDateAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is DateTime date && date <= DateTime.Now)
        {
            return new ValidationResult("Date must be in the future");
        }
        return ValidationResult.Success;
    }
}
```

## Observability Examples

### Structured Logging
```csharp
public async Task<IActionResult> GetCustomerAsync(int id)
{
    _logger.LogInformation("Retrieving customer {CustomerId}", id);

    var customer = await _customerService.GetByIdAsync(id);
    if (customer == null)
    {
        _logger.LogWarning("Customer {CustomerId} not found", id);
        return NotFound();
    }

    return Ok(customer);
}
```

### Metrics Implementation
```csharp
public class MetricsService
{
    private readonly Counter<long> _requestCounter;

    public MetricsService(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("HotshotLogistics.Api");
        _requestCounter = meter.CreateCounter<long>("api_requests_total", "Total number of API requests");
    }

    public void IncrementRequestCount(string endpoint)
    {
        _requestCounter.Add(1, new KeyValuePair<string, object>("endpoint", endpoint));
    }
}
```

### Correlation ID Middleware
```csharp
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        await _next(context);
    }
}
```

These examples demonstrate best practices for implementing code quality measures in the Hotshot Logistics application.