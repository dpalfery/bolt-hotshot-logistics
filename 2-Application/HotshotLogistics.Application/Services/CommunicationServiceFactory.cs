// <copyright file="CommunicationServiceFactory.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using HotshotLogistics.Contracts.Services;
using Microsoft.Extensions.Logging;

namespace HotshotLogistics.Application.Services
{
    /// <summary>
    ///     Factory for creating communication service instances.
    /// </summary>
    public class CommunicationServiceFactory : ICommunicationServiceFactory
    {
        private readonly Dictionary<string, ICommunicationService> _services;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommunicationServiceFactory" /> class.
        /// </summary>
        /// <param name="services">The collection of communication services.</param>
        /// <param name="logger">The logger.</param>
        public CommunicationServiceFactory(
            IEnumerable<ICommunicationService> services,
            ILogger<CommunicationServiceFactory> logger)
        {
            ArgumentNullException.ThrowIfNull(logger);
            _services = services.ToDictionary(s => s.Type);

            // Validate that all communication types are registered
            string[] expectedTypes = ["Sms", "Email", "Push"];
            foreach (string type in expectedTypes)
            {
                if (!_services.ContainsKey(type))
                {
                    logger.LogWarning("Communication service for type {Type} is not registered", type);
                }
            }
        }

        /// <inheritdoc />
        public ICommunicationService GetService(string type)
        {
            if (_services.TryGetValue(type, out ICommunicationService? service))
            {
                return service;
            }

            throw new InvalidOperationException($"Communication service for type {type} is not available");
        }
    }
}
