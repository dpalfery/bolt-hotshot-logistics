# Security Examples

This document provides detailed implementation examples for the security rules in Hotshot Logistics.

## Secrets Management Examples

### Azure Key Vault Configuration

```csharp
// In Program.cs
builder.Configuration.AddAzureKeyVault(new Uri("https://myvault.vault.azure.net/"), new DefaultAzureCredential());
```

### JWT Token Validation

```csharp
// In Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "https://login.microsoftonline.com/tenant-id";
        options.Audience = "api://client-id";
    });
```

## Resilience Examples

### Polly Policy Configuration

```csharp
// In Program.cs
var retryPolicy = Policy
    .Handle<SqlException>()
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

builder.Services.AddHttpClient("ResilientClient")
    .AddPolicyHandler(retryPolicy);
```

### Circuit Breaker Implementation

```csharp
var circuitBreakerPolicy = Policy
    .Handle<Exception>()
    .CircuitBreakerAsync(5, TimeSpan.FromMinutes(1));

builder.Services.AddHttpClient("CircuitBreakerClient")
    .AddPolicyHandler(circuitBreakerPolicy);
```

## Optional Features Examples

### Rate Limiting Setup

```csharp
// In Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
    });
});
```

### Output Caching Configuration

```csharp
// In Program.cs
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Cache());
});
```

### gRPC Health Checks

```csharp
// In Program.cs
builder.Services.AddGrpcHealthChecks()
    .AddCheck("grpc", () => HealthCheckResult.Healthy());