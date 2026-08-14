// <copyright file="AzureAppConfigurationExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Core.Extensions
{
    using Azure.Identity;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Adds Azure App Configuration when it is already pointed to by user secrets or environment variables.
    /// When it is not configured, existing sources (user secrets, environment) remain the configuration.
    /// </summary>
    public static class OptionalAzureAppConfigurationExtensions
    {
        /// <summary>
        /// Configuration key for the App Configuration endpoint URI.
        /// </summary>
        public const string EndpointKey = "AppConfiguration:Endpoint";

        /// <summary>
        /// Named connection string used to connect to App Configuration.
        /// </summary>
        public const string ConnectionStringName = "AppConfig";

        /// <summary>
        /// Adds Azure App Configuration when a connection string or endpoint is present.
        /// </summary>
        /// <param name="builder">The configuration builder (already containing user secrets and environment variables).</param>
        /// <param name="useDefaultAzureCredential">
        /// When true, an endpoint URI can authenticate with <see cref="DefaultAzureCredential"/>.
        /// When false (tests), only a connection string is used so credential discovery cannot hang.
        /// </param>
        /// <returns><see langword="true"/> when App Configuration was added; otherwise <see langword="false"/>.</returns>
        public static bool AddAzureAppConfigurationIfConfigured(
            this IConfigurationBuilder builder,
            bool useDefaultAzureCredential = true)
        {
            ArgumentNullException.ThrowIfNull(builder);

            IConfiguration lookup = builder is IConfiguration configuration
                ? configuration
                : builder.Build();

            var connectionString = lookup.GetConnectionString(ConnectionStringName)
                ?? lookup["AZURE_APPCONFIGURATION_CONNECTION_STRING"];

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                builder.AddAzureAppConfiguration(connectionString);
                return true;
            }

            if (!useDefaultAzureCredential)
            {
                return false;
            }

            var endpoint = lookup[EndpointKey] ?? lookup["AZURE_APPCONFIGURATION_ENDPOINT"];
            if (string.IsNullOrWhiteSpace(endpoint)
                || !Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
            {
                return false;
            }

            builder.AddAzureAppConfiguration(options =>
                options.Connect(
                    endpointUri,
                    new DefaultAzureCredential(new DefaultAzureCredentialOptions
                    {
                        ExcludeInteractiveBrowserCredential = true,
                    })));

            return true;
        }
    }
}
