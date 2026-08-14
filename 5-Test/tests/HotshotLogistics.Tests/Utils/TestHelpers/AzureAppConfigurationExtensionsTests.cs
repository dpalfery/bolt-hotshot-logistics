// <copyright file="AzureAppConfigurationExtensionsTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Tests
{
    using FluentAssertions;
    using HotshotLogistics.Core.Extensions;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Tests for optional Azure App Configuration loading.
    /// </summary>
    public class AzureAppConfigurationExtensionsTests
    {
        [Fact]
        public void AddAzureAppConfigurationIfConfigured_WhenNotConfigured_ReturnsFalse()
        {
            var builder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>());

            var added = builder.AddAzureAppConfigurationIfConfigured();

            added.Should().BeFalse();
        }

        [Fact]
        public void AddAzureAppConfigurationIfConfigured_WhenEndpointSetWithoutCredential_ReturnsFalse()
        {
            var builder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    [OptionalAzureAppConfigurationExtensions.EndpointKey] = "https://example.azconfig.io",
                });

            var added = builder.AddAzureAppConfigurationIfConfigured(useDefaultAzureCredential: false);

            added.Should().BeFalse();
        }
    }
}
