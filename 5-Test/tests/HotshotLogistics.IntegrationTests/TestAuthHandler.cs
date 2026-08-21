// <copyright file="TestAuthHandler.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace HotshotLogistics.IntegrationTests
{
    /// <summary>
    ///     Authentication handler for integration tests that validates the "Test" authentication scheme.
    /// </summary>
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        /// <inheritdoc />
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Require Authorization header
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
            }

            string authHeader = Request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Test", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Scheme"));
            }

            // Allow tests to set the user's role dynamically per request.
            // Default to Admin if no role header is provided.
            string requestedRole = "Admin";
            if (Request.Headers.TryGetValue("X-Test-Role", out StringValues roleHeader) &&
                !string.IsNullOrWhiteSpace(roleHeader.ToString()))
            {
                requestedRole = roleHeader.ToString().Trim();
            }

            // Build a principal with the requested role
            Claim[] claims =
            [
                new(ClaimTypes.NameIdentifier, "test-user-id"),
                new(ClaimTypes.Name, "Test User"),
                new(ClaimTypes.Email, "test@example.com"),
                new(ClaimTypes.Role, requestedRole),
                new("roles", requestedRole) // Mirror Entra ID "roles" claim for policy evaluation
            ];

            ClaimsIdentity identity = new(claims, "Test");
            ClaimsPrincipal principal = new(identity);
            AuthenticationTicket ticket = new(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
