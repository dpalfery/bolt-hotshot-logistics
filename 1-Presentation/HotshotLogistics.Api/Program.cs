// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using HotshotLogistics.Application;
using HotshotLogistics.Core.Extensions;
using HotshotLogistics.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Text.Json;
using Azure.Identity;
using HotshotLogistics.Api;
using Microsoft.Graph;
using HotshotLogistics.Application.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using HotshotLogistics.Domain.DTOs;

var builder = WebApplication.CreateBuilder(args);

// User secrets (Development, or optional elsewhere) supply App Config connection details locally.
builder.Configuration.AddUserSecrets(typeof(Program).Assembly, optional: true);
var azureAppConfigurationEnabled = builder.Configuration.AddAzureAppConfigurationIfConfigured();

// Configure settings
builder.Services.Configure<GoogleMapsSettings>(builder.Configuration.GetSection("Mapping:GoogleMaps"));
builder.Services.Configure<AzureMapsSettings>(builder.Configuration.GetSection("Mapping:AzureMaps"));
builder.Services.Configure<SendGridSettings>(builder.Configuration.GetSection("Communication:SendGrid"));

// Add services to the container.
builder.Services.AddHotshotRepositories();
builder.Services.AddApplicationServices();
builder.Services.AddMappingServices();
builder.Services.AddCommunicationServices();

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
    .AddNewtonsoftJson(options =>
    {
        // System.Text.Json 9+/10 requires PipeWriter.UnflushedBytes, which the ASP.NET Core 8
        // test host does not implement when tests roll forward to .NET 10.
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HotshotLogistics.Api", Version = "v1" });
});

// Add HTTP client factory for external API calls
builder.Services.AddHttpClient();

// Add distributed cache (using in-memory for development)
builder.Services.AddDistributedMemoryCache();

if (azureAppConfigurationEnabled)
{
    builder.Services.AddAzureAppConfiguration();
}

// Register GraphServiceClient
builder.Services.AddScoped(sp =>
{
    var options = new DefaultAzureCredentialOptions
    {
        ExcludeSharedTokenCacheCredential = true,
        ExcludeAzureCliCredential = true,
        ExcludeEnvironmentCredential = true,
        ExcludeManagedIdentityCredential = false,
        ExcludeVisualStudioCredential = true,
        ExcludeInteractiveBrowserCredential = true
    };
    var credential = new DefaultAzureCredential(options);
    return new GraphServiceClient(credential);
});

// Add authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Manager", policy => policy.RequireRole("Manager"));
    options.AddPolicy("Driver", policy => policy.RequireRole("Driver"));
    options.AddPolicy("Customer", policy => policy.RequireRole("Customer"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Admin", "Manager"));
    options.AddPolicy("ManagerOrDriver", policy => policy.RequireRole("Manager", "Driver"));
    options.AddPolicy("OwnResource", policy => policy.RequireAuthenticatedUser()); // Placeholder for resource-based auth
    options.AddPolicy("CustomerResource", policy => policy.RequireAuthenticatedUser()); // Placeholder for resource-based auth
});

// Configure SignalR
builder.Services.AddSignalR();

var app = builder.Build();

if (azureAppConfigurationEnabled)
{
    app.UseAzureAppConfiguration();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure CORS policy - must be after UseHttpsRedirection but before UseAuthentication
app.UseHttpsRedirection();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

app.UseCors(policy =>
{
    policy.WithOrigins(allowedOrigins)
          .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS") // Be specific
          .WithHeaders("Content-Type", "Authorization", "X-Requested-With") // Be specific
          .AllowCredentials();
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }