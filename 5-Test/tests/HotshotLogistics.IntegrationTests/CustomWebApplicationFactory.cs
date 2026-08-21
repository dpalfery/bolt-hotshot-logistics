// <copyright file="CustomWebApplicationFactory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using HotshotLogistics.Core.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HotshotLogistics.IntegrationTests
{
    /// <summary>
    ///     Custom web application factory for integration tests that configures test authentication.
    /// </summary>
    /// <typeparam name="TProgram">The program type.</typeparam>
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>
        where TProgram : class
    {
        /// <inheritdoc />
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Authentication scheme is configured in Program.cs for Development; avoid re-registering here to prevent "Scheme already exists: Test".

            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddUserSecrets(typeof(Program).Assembly, true);
                config.AddEnvironmentVariables();
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Mapping:Provider"] = "Mock"
                });
                config.AddAzureAppConfigurationIfConfigured(false);
            });

            // Enable detailed logging for debugging
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Trace); // Show ALL logs including trace

                // Add specific loggers for troubleshooting
                logging.AddFilter("HotshotLogistics", LogLevel.Debug);
                logging.AddFilter("Microsoft.AspNetCore", LogLevel.Information);
                logging.AddFilter("HotshotLogistics.IntegrationTests", LogLevel.Trace);
            });

            // Use Development environment for better error messages
            builder.UseEnvironment("Development");
        }
    }
}
