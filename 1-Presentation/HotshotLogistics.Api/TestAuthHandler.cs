// <copyright file="TestAuthHandler.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace HotshotLogistics.Api
{
    /// <summary>
    ///     Test authentication handler for development and integration testing.
    /// </summary>
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly string _defaultRole;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TestAuthHandler" /> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="encoder">The URL encoder.</param>
        /// <param name="defaultRole">The default role.</param>
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            string defaultRole = "Admin")
            : base(options, logger, encoder)
        {
            _defaultRole = defaultRole;
        }

        /// <inheritdoc />
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Require Authorization header and the 'Test' scheme for dev/testing
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
            }

            string authHeader = Request.Headers["Authorization"].ToString();
            if (!authHeader.StartsWith("Test", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Scheme"));
            }

            // Allow dynamic role per request via X-Test-Role header; default to configured _defaultRole
            string requestedRole = _defaultRole;
            if (Request.Headers.TryGetValue("X-Test-Role", out StringValues roleHeader) &&
                !string.IsNullOrWhiteSpace(roleHeader.ToString()))
            {
                requestedRole = roleHeader.ToString().Trim();
            }

            Claim[] claims =
            [
                new(ClaimTypes.NameIdentifier, "test-user-id"),
                new(ClaimTypes.Name, "Test User"),
                new(ClaimTypes.Email, "test@example.com"),
                new(ClaimTypes.Role, requestedRole),
                new("roles", requestedRole)
            ];

            ClaimsIdentity identity = new(claims, "Test");
            ClaimsPrincipal principal = new(identity);
            AuthenticationTicket ticket = new(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
