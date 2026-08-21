// <copyright file="IntegrationTestBase.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc.Testing;

namespace HotshotLogistics.Tests.Utils.TestHelpers
{
    /// <summary>
    ///     Base class for integration tests that sets up the web application factory and provides a test client.
    /// </summary>
    public abstract class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        protected readonly HttpClient Client;
        protected readonly WebApplicationFactory<Program> Factory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IntegrationTestBase" /> class.
        /// </summary>
        /// <param name="factory">The web application factory.</param>
        protected IntegrationTestBase(WebApplicationFactory<Program> factory)
        {
            Factory = factory;
            Client = Factory.CreateClient();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Gets the database connection string for tests.
        /// </summary>
        /// <returns>The connection string.</returns>
        protected static string GetConnectionString()
        {
            return TestDatabaseHelper.GetConnectionString();
        }

        /// <summary>
        ///     Disposes the HTTP client.
        /// </summary>
        /// <param name="disposing">True if called from Dispose(); false if called from a finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Client.Dispose();
            }
        }
    }
}
