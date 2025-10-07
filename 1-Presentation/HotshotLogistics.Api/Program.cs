// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using HotshotLogistics.Application;
using HotshotLogistics.Data;
using HotshotLogistics.Application.Authorization;
using HotshotLogistics.Api.Middleware;
using HotshotLogistics.Application.Hubs;
using Microsoft.AspNetCore.Authorization;
using System;
using System.IO;
using System.Text.Json;
using Azure.Identity;
using HotshotLogistics.Api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using HotshotLogistics.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Configure configuration
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Load local.settings.json for local development (for compatibility with existing setup)
if (builder.Environment.IsDevelopment())
{
    var localSettingsPath = Path.Combine(builder.Environment.ContentRootPath, "local.settings.json");
    if (File.Exists(localSettingsPath))
    {
        using var stream = File.OpenRead(localSettingsPath);
        using var doc = JsonDocument.Parse(stream);
        if (doc.RootElement.TryGetProperty("Values", out var values))
        {
            foreach (var prop in values.EnumerateObject())
            {
                var key = prop.Name;
                var value = prop.Value.GetString();
                if (!string.IsNullOrEmpty(key) && value != null)
                {
                    builder.Configuration[key] = value;
                }
            }
        }
    }
}

// Get Azure App Configuration endpoint from configuration
var appConfigEndpoint = builder.Configuration["AppConfig:Endpoint"];
if (!string.IsNullOrEmpty(appConfigEndpoint))
{
    builder.Configuration.AddAzureAppConfiguration(options =>
    {
        options.Connect(new Uri(appConfigEndpoint), new DefaultAzureCredential())
               .ConfigureRefresh(refresh =>
               {
                   refresh.Register("Sentinel", refreshAll: true)
                          .SetRefreshInterval(TimeSpan.FromSeconds(30));
               })
               .Select("*");
    });
}

// Add services to the container
if (builder.Environment.IsDevelopment())
{
    // Use test authentication handler for local development
    builder.Services.AddAuthentication("Test")
        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
}
else
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));
}

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ignore null values to reduce payload size
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;

        // Handle circular references gracefully
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add HTTP client factory for external API calls
builder.Services.AddHttpClient();

// Add distributed cache (using in-memory for development)
builder.Services.AddDistributedMemoryCache();

// Register Azure App Configuration refresh service
builder.Services.AddAzureAppConfiguration();

// Register repositories (ADO.NET-based). DbContext removed in favor of native ADO.NET + FluentMigrator.
builder.Services.AddHotshotRepositories();

// Register application services
builder.Services.AddApplicationServices();

// Register GraphServiceClient
builder.Services.AddScoped(sp =>
{
    var options = new DefaultAzureCredentialOptions
    {
        ExcludeSharedTokenCacheCredential = true,
        ExcludeAzureCliCredential = true,
        ExcludeEnvironmentCredential = true,
        ExcludeManagedIdentityCredential = false,
        ExcludeVisualStudioCodeCredential = true,
        ExcludeVisualStudioCredential = true,
        ExcludeInteractiveBrowserCredential = true
    };
    var credential = new DefaultAzureCredential(options);
    return new GraphServiceClient(credential);
});

// Add authorization
builder.Services.AddAuthorization(options =>
{
    // Role-based policies
    options.AddPolicy(AuthorizationPolicies.Admin, policy =>
        policy.RequireRole("Admin"));
    options.AddPolicy(AuthorizationPolicies.Manager, policy =>
        policy.RequireRole("Manager"));
    options.AddPolicy(AuthorizationPolicies.Driver, policy =>
        policy.RequireRole("Driver"));
    options.AddPolicy(AuthorizationPolicies.Customer, policy =>
        policy.RequireRole("Customer"));

    // Composite role policies
    options.AddPolicy(AuthorizationPolicies.ManagerOrAdmin, policy =>
        policy.RequireRole("Manager", "Admin"));
    options.AddPolicy(AuthorizationPolicies.ManagerOrDriver, policy =>
        policy.RequireRole("Manager", "Driver"));

    // Resource-based policies
    options.AddPolicy(AuthorizationPolicies.OwnResource, policy =>
        policy.AddRequirements(new ResourceOwnerRequirement("Own")));
    options.AddPolicy(AuthorizationPolicies.CustomerResource, policy =>
        policy.AddRequirements(new ResourceOwnerRequirement("Customer")));
});

// Register authorization handlers
builder.Services.AddSingleton<IAuthorizationHandler, ResourceOwnerAuthorizationHandler>();

// Add SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable HTTPS redirection and security headers
app.UseHttpsRedirection();
app.UseHsts();

// Enable CORS for development (configure appropriately for production)
app.UseCors(policy =>
{
    policy.AllowAnyOrigin()
          .AllowAnyMethod()
          .AllowAnyHeader();
});

// Use custom exception handling middleware
app.UseExceptionHandling();

// Enable authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map a root endpoint
app.MapGet("/", () => Results.Redirect("/swagger"));

// Map SignalR hub
app.MapHub<RealtimeHub>("/hubs/realtime");
        
// Map controllers
app.MapControllers();

app.Run();


/// &lt;summary&gt;
/// Main entry point for the application. This partial class is required for WebApplicationFactory in integration tests.
/// &lt;/summary&gt;
public partial class Program { }
