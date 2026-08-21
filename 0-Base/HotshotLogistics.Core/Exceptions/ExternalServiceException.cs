// <copyright file="ExternalServiceException.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Core.Exceptions
{
    /// <summary>
    ///     Exception thrown when an external service call fails.
    /// </summary>
    public class ExternalServiceException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ExternalServiceException" /> class.
        /// </summary>
        /// <param name="serviceName">The name of the external service.</param>
        public ExternalServiceException(string serviceName)
            : base($"External service '{serviceName}' failed")
        {
            ServiceName = serviceName;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExternalServiceException" /> class with a specified error message.
        /// </summary>
        /// <param name="serviceName">The name of the external service.</param>
        /// <param name="message">The message that describes the error.</param>
        public ExternalServiceException(string serviceName, string message)
            : base(message)
        {
            ServiceName = serviceName;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExternalServiceException" /> class with a specified error message and
        ///     status code.
        /// </summary>
        /// <param name="serviceName">The name of the external service.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="statusCode">The HTTP status code returned by the external service.</param>
        public ExternalServiceException(string serviceName, string message, int statusCode)
            : base(message)
        {
            ServiceName = serviceName;
            StatusCode = statusCode;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExternalServiceException" /> class with a specified error message,
        ///     status code, and endpoint.
        /// </summary>
        /// <param name="serviceName">The name of the external service.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="statusCode">The HTTP status code returned by the external service.</param>
        /// <param name="endpoint">The endpoint that was called.</param>
        public ExternalServiceException(string serviceName, string message, int statusCode, string endpoint)
            : base(message)
        {
            ServiceName = serviceName;
            StatusCode = statusCode;
            Endpoint = endpoint;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExternalServiceException" /> class with a specified error message and
        ///     a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="serviceName">The name of the external service.</param>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ExternalServiceException(string serviceName, string message, Exception innerException)
            : base(message, innerException)
        {
            ServiceName = serviceName;
        }

        /// <summary>
        ///     Gets the name of the external service.
        /// </summary>
        public string ServiceName { get; }

        /// <summary>
        ///     Gets the HTTP status code returned by the external service.
        /// </summary>
        public int? StatusCode { get; }

        /// <summary>
        ///     Gets the endpoint that was called.
        /// </summary>
        public string? Endpoint { get; }
    }
}
