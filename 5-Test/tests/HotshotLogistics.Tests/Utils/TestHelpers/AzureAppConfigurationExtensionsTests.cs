// <copyright file="AzureAppConfigurationExtensionsTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using FluentAssertions;
using HotshotLogistics.Core.Extensions;
using Microsoft.Extensions.Configuration;

namespace HotshotLogistics.Tests.Utils.TestHelpers
{
    /// <summary>
    ///     Tests for optional Azure App Configuration loading.
    /// </summary>
    public class AzureAppConfigurationExtensionsTests
    {
        [Fact]
        public void AddAzureAppConfigurationIfConfigured_WhenNotConfigured_ReturnsFalse()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>());

            bool added = builder.AddAzureAppConfigurationIfConfigured();

            added.Should().BeFalse();
        }

        [Fact]
        public void AddAzureAppConfigurationIfConfigured_WhenEndpointSetWithoutCredential_ReturnsFalse()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [OptionalAzureAppConfigurationExtensions.EndpointKey] = "https://example.azconfig.io"
                });

            bool added = builder.AddAzureAppConfigurationIfConfigured(false);

            added.Should().BeFalse();
        }
    }
}
