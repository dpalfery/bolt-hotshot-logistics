// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using HotshotLogistics.Application;
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

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HotshotLogistics.Api", Version = "v1" });
});

// Add HTTP client factory for external API calls
builder.Services.AddHttpClient();

// Add distributed cache (using in-memory for development)
builder.Services.AddDistributedMemoryCache();

// Register Azure App Configuration refresh service
builder.Services.AddAzureAppConfiguration();

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
});

// Configure SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }