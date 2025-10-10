// <copyright file="MappingServiceFactory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Application.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HotshotLogistics.Contracts.Factories;
    using HotshotLogistics.Contracts.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Factory for creating mapping service instances based on configuration.
    /// </summary>
    public class MappingServiceFactory : IMappingServiceFactory
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<MappingServiceFactory> logger;
        private readonly IDictionary<string, IMappingService> mappingServices;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingServiceFactory"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="logger">The logger for the factory.</param>
        /// <param name="mappingServices">A collection of all registered IMappingService implementations.</param>
        public MappingServiceFactory(
            IConfiguration configuration,
            ILogger<MappingServiceFactory> logger,
            IEnumerable<IMappingService> mappingServices)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.mappingServices = mappingServices?.ToDictionary(s => s.GetType().Name.Replace("Service", string.Empty), StringComparer.OrdinalIgnoreCase)
                                   ?? throw new ArgumentNullException(nameof(mappingServices));
        }

        /// <summary>
        /// Creates a mapping service instance based on the configured provider.
        /// </summary>
        /// <returns>The mapping service instance.</returns>
        public IMappingService CreateMappingService()
        {
            var providerName = configuration["Mapping:Provider"] ?? "Mock";

            if (mappingServices.TryGetValue(providerName, out var service))
            {
                logger.LogInformation("Using mapping service: {ProviderName}", providerName);
                return service;
            }

            logger.LogError("Unsupported mapping provider: {ProviderName}. Falling back to Mock.", providerName);
            if (mappingServices.TryGetValue("Mock", out var mockService))
            {
                return mockService;
            }

            throw new InvalidOperationException($"Unsupported mapping provider: {providerName} and no Mock service found.");
        }
    }
}
