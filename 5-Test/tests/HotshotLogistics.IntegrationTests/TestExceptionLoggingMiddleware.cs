// <copyright file="TestExceptionLoggingMiddleware.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace HotshotLogistics.IntegrationTests
{
    /// <summary>
    ///     Middleware for capturing and logging detailed exception information during integration tests.
    /// </summary>
    public class TestExceptionLoggingMiddleware
    {
        private readonly ILogger<TestExceptionLoggingMiddleware> _logger;
        private readonly RequestDelegate _next;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TestExceptionLoggingMiddleware" /> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The _logger.</param>
        public TestExceptionLoggingMiddleware(RequestDelegate next, ILogger<TestExceptionLoggingMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        ///     Invokes the middleware.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Log the request details
                _logger.LogInformation("TEST REQUEST: {Method} {Path} {QueryString}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.QueryString);

                // Capture request body if needed
                if (context.Request.Method == "POST" || context.Request.Method == "PUT")
                {
                    context.Request.EnableBuffering();
                    string requestBody = await ReadRequestBodyAsync(context.Request);
                    if (!string.IsNullOrEmpty(requestBody))
                    {
                        _logger.LogInformation("TEST REQUEST BODY: {RequestBody}", requestBody);
                    }

                    context.Request.Body.Position = 0;
                }

                await _next(context);

                // Log successful response
                _logger.LogInformation("TEST RESPONSE: {StatusCode} for {Method} {Path}",
                    context.Response.StatusCode,
                    context.Request.Method,
                    context.Request.Path);
            }
            catch (Exception ex)
            {
                // Log detailed exception information
                _logger.LogError(ex,
                    "TEST EXCEPTION: {ExceptionType} during {Method} {Path}\n" +
                    "Message: {Message}\n" +
                    "Stack Trace: {StackTrace}\n" +
                    "Inner Exception: {InnerException}",
                    ex.GetType().Name,
                    context.Request.Method,
                    context.Request.Path,
                    ex.Message,
                    ex.StackTrace,
                    ex.InnerException?.ToString() ?? "None");

                // Re-throw to maintain normal error handling flow
                throw;
            }
        }

        private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            using StreamReader reader = new(request.Body, Encoding.UTF8, leaveOpen: true);
            string body = await reader.ReadToEndAsync();
            return body;
        }
    }
}
