// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Api
{
    using System;
    using System.IO;
    using System.Text.Json;
    using Azure.Identity;
    using HotshotLogistics.Api.Middleware;
    using HotshotLogistics.Application;
    using HotshotLogistics.Application.Authorization;
    using HotshotLogistics.Data;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Identity.Web;
    using FluentValidation.AspNetCore;

    /// <summary>
    /// The main program class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure configuration
            builder.Configuration.AddEnvironmentVariables();

            // Use local.settings.json for local development
            if (builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

                // Explicitly load Values from local.settings.json and add as environment variables
                var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
                var localSettingsPath = assemblyDirectory != null ? Path.Combine(assemblyDirectory, "local.settings.json") : string.Empty;
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
                                Environment.SetEnvironmentVariable(key, value);
                            }
                        }
                    }
                }
            }

            // Get Azure App Configuration endpoint from environment or local.settings.json
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

            // Register Azure App Configuration refresh service
            builder.Services.AddAzureAppConfiguration();

            // Commented out for local development without Azure AD
            // builder.Services.AddAuthentication("Bearer")
            //                .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));

            // builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration.GetSection("AzureAdB2C"))
            //                .EnableTokenAcquisitionToCallDownstreamApi()
            //                .AddMicrosoftGraph()
            //                .AddInMemoryTokenCaches();

            // Register repositories (ADO.NET-based). DbContext removed in favor of native ADO.NET + FluentMigrator.
            builder.Services.AddHotshotRepositories();

            // Register application services
            builder.Services.AddApplicationServices();

            // Add controllers
            builder.Services.AddControllers();

            // Add FluentValidation
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddFluentValidationClientsideAdapters();

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
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
                options.AddPolicy(AuthorizationPolicies.AdminOrManager, policy =>
                    policy.RequireRole("Admin", "Manager"));
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

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
