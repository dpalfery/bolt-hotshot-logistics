// <copyright file="PaymentProcessingException.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Core.Exceptions
{
    /// <summary>
    ///     Exception thrown when payment processing fails.
    /// </summary>
    public class PaymentProcessingException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PaymentProcessingException" /> class.
        /// </summary>
        public PaymentProcessingException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PaymentProcessingException" /> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public PaymentProcessingException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PaymentProcessingException" /> class with a specified error message
        ///     and transaction ID.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="transactionId">The payment transaction ID.</param>
        public PaymentProcessingException(string message, string transactionId)
            : base(message)
        {
            TransactionId = transactionId;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PaymentProcessingException" /> class with a specified error message,
        ///     transaction ID, and provider error code.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="transactionId">The payment transaction ID.</param>
        /// <param name="providerErrorCode">The payment provider error code.</param>
        public PaymentProcessingException(string message, string transactionId, string providerErrorCode)
            : base(message)
        {
            TransactionId = transactionId;
            ProviderErrorCode = providerErrorCode;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PaymentProcessingException" /> class with a specified error message
        ///     and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public PaymentProcessingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Gets the payment transaction ID.
        /// </summary>
        public string? TransactionId { get; }

        /// <summary>
        ///     Gets the payment provider error code.
        /// </summary>
        public string? ProviderErrorCode { get; }
    }
}
