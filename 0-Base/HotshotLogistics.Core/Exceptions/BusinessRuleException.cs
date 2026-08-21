// <copyright file="BusinessRuleException.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Core.Exceptions
{
    /// <summary>
    ///     Exception thrown when a business rule is violated.
    /// </summary>
    public class BusinessRuleException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="BusinessRuleException" /> class.
        /// </summary>
        public BusinessRuleException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BusinessRuleException" /> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public BusinessRuleException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="BusinessRuleException" /> class with a specified error message and a
        ///     reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public BusinessRuleException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
