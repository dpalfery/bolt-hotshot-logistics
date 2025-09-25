// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Api
{
    using System;
    using System.IO;
    using System.Text.Json;
    using Azure.Identity;
    using HotshotLogistics.Application;
    using HotshotLogistics.Data;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// The main program class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    config.AddEnvironmentVariables();

                    // Use local.settings.json for local development
                    if (env.IsDevelopment())
                    {
                        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

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

                    var settings = config.Build();

                    // Get Azure App Configuration endpoint from environment or local.settings.json
                    var appConfigEndpoint = settings["AppConfig:Endpoint"];
                    if (!string.IsNullOrEmpty(appConfigEndpoint))
                    {
                        _ = config.AddAzureAppConfiguration(options =>
                        {
                            _ = options.Connect(new Uri(appConfigEndpoint), new DefaultAzureCredential())
                                   // Sentinel key for refresh
                                   .ConfigureRefresh(refresh =>
                                   {
                                       _ = refresh.Register("Sentinel", refreshAll: true)
                                              .SetRefreshInterval(TimeSpan.FromSeconds(30));
                                   })
                                   .Select("*");
                        });
                    }
                })
                .ConfigureServices((context, services) =>
                {
                    // Register Azure App Configuration refresh service
                    _ = services.AddAzureAppConfiguration();

                    // Register repositories (ADO.NET-based). DbContext removed in favor of native ADO.NET + FluentMigrator.
                    _ = services.AddHotshotRepositories();

                    // Register application services
                    services.AddApplicationServices();

                    // Add controllers
                    services.AddControllers();
                })
                .Build();

            host.Run();
        }
    }
}
