// <copyright file="MappingServiceFactory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Application.Services
{
    using System;
    using System.Net.Http;
    using HotshotLogistics.Contracts.Services;
    using HotshotLogistics.Data.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Factory for creating mapping service instances based on configuration.
    /// </summary>
    public class MappingServiceFactory
    {
        private readonly IConfiguration configuration;
        private readonly ILoggerFactory loggerFactory;
        private readonly IHttpClientFactory httpClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingServiceFactory"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        /// <param name="httpClientFactory">The HTTP client factory.</param>
        public MappingServiceFactory(
            IConfiguration configuration,
            ILoggerFactory loggerFactory,
            IHttpClientFactory httpClientFactory)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        /// <summary>
        /// Creates a mapping service instance based on the configured provider.
        /// </summary>
        /// <returns>The mapping service instance.</returns>
        public IMappingService CreateMappingService()
        {
            var provider = configuration["Mapping:Provider"] ?? "AzureMaps";

            var httpClient = httpClientFactory.CreateClient("MappingService");
            httpClient.Timeout = TimeSpan.FromSeconds(30); // Set reasonable timeout for mapping APIs

            switch (provider.ToLowerInvariant())
            {
                case "azuremaps":
                    var azureMapsKey = configuration["Mapping:AzureMaps:SubscriptionKey"];
                    if (string.IsNullOrEmpty(azureMapsKey))
                    {
                        throw new InvalidOperationException("Azure Maps subscription key is not configured");
                    }

                    var azureLogger = loggerFactory.CreateLogger<AzureMapsService>();
                    return new AzureMapsService(httpClient, azureLogger, azureMapsKey);

                case "googlemaps":
                    var googleMapsKey = configuration["Mapping:GoogleMaps:ApiKey"];
                    if (string.IsNullOrEmpty(googleMapsKey))
                    {
                        throw new InvalidOperationException("Google Maps API key is not configured");
                    }

                    var googleLogger = loggerFactory.CreateLogger<GoogleMapsService>();
                    return new GoogleMapsService(httpClient, googleLogger, googleMapsKey);

                default:
                    throw new InvalidOperationException($"Unsupported mapping provider: {provider}");
            }
        }
    }
}