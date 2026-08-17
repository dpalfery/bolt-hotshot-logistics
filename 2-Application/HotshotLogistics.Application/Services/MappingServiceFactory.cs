// <copyright file="MappingServiceFactory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Application.Services
{
    using System;
    using System.Linq;
    using HotshotLogistics.Contracts.Factories;
    using HotshotLogistics.Contracts.Services;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Factory for creating mapping service instances based on configuration.
    /// </summary>
    public class MappingServiceFactory : IMappingServiceFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<MappingServiceFactory> _logger;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingServiceFactory"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        /// <param name="logger">The logger for the factory.</param>
        /// <param name="serviceProvider">The service provider to resolve mapping services.</param>
        public MappingServiceFactory(
            IConfiguration configuration,
            ILogger<MappingServiceFactory> logger,
            IServiceProvider serviceProvider)
        {
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc/>
        public IMappingService CreateMappingService()
        {
            var providerName = NormalizeProviderName(_configuration["Mapping:Provider"] ?? "Mock");
            var mappingServiceDict = _serviceProvider.GetServices<IMappingService>()
                .GroupBy(s => s.GetType())
                .Select(g => g.First())
                .ToDictionary(
                    s => s.GetType().Name.Replace("Service", string.Empty, StringComparison.Ordinal),
                    StringComparer.OrdinalIgnoreCase);

            if (mappingServiceDict.TryGetValue(providerName, out var service))
            {
                _logger.LogInformation("Using mapping service: {ProviderName}", providerName);
                return service;
            }

            _logger.LogError("Unsupported mapping provider: {ProviderName}. Falling back to Mock.", providerName);
            if (mappingServiceDict.TryGetValue("MockMapping", out var mockService) || mappingServiceDict.TryGetValue("Mock", out mockService))
            {
                return mockService;
            }

            throw new InvalidOperationException($"Unsupported mapping provider: {providerName} and no Mock service found.");
        }

        private static string NormalizeProviderName(string providerName)
        {
            return string.Equals(providerName, "Mock", StringComparison.OrdinalIgnoreCase)
                ? "MockMapping"
                : providerName;
        }
    }
}
