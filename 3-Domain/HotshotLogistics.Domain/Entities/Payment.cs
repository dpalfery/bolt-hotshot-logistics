using HotshotLogistics.Core.Enums;

namespace HotshotLogistics.Domain.Entities
{
    /// <summary>
    ///     Represents a payment in the system.
    /// </summary>
    public class Payment
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Payment" /> class.
        /// </summary>
        public Payment()
        {
            if (CreatedAt == default)
            {
                CreatedAt = DateTime.UtcNow;
            }

            if (PaymentDate == default)
            {
                PaymentDate = DateTime.UtcNow;
            }
        }

        /// <summary>
        ///     Gets or sets the payment identifier.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the associated invoice identifier.
        /// </summary>
        public string InvoiceId { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the payment date.
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        ///     Gets or sets the payment amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        ///     Gets or sets the payment method.
        /// </summary>
        public PaymentMethodType PaymentMethod { get; set; }

        /// <summary>
        ///     Gets or sets the transaction identifier from the payment processor.
        /// </summary>
        public string TransactionId { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the raw response from the payment processor.
        /// </summary>
        public string ProcessorResponse { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the payment status.
        /// </summary>
        public PaymentStatus Status { get; set; }

        /// <summary>
        ///     Gets or sets the creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        ///     Gets or sets the last updated timestamp.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        ///     Marks the payment as completed.
        /// </summary>
        /// <param name="transactionId">The transaction identifier from the payment processor.</param>
        /// <param name="processorResponse">The response from the payment processor.</param>
        public void MarkAsCompleted(string transactionId, string processorResponse = "")
        {
            Status = PaymentStatus.Completed;
            TransactionId = transactionId;
            ProcessorResponse = processorResponse;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        ///     Marks the payment as failed.
        /// </summary>
        /// <param name="processorResponse">The response from the payment processor.</param>
        public void MarkAsFailed(string processorResponse = "")
        {
            Status = PaymentStatus.Failed;
            ProcessorResponse = processorResponse;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        ///     Marks the payment as processing.
        /// </summary>
        public void MarkAsProcessing()
        {
            Status = PaymentStatus.Processing;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        ///     Marks the payment as refunded.
        /// </summary>
        /// <param name="processorResponse">The response from the payment processor.</param>
        public void MarkAsRefunded(string processorResponse = "")
        {
            Status = PaymentStatus.Refunded;
            ProcessorResponse = processorResponse;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        ///     Cancels the payment.
        /// </summary>
        public void Cancel()
        {
            Status = PaymentStatus.Cancelled;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        ///     Validates the payment data.
        /// </summary>
        /// <returns>True if the payment data is valid, false otherwise.</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Id) &&
                   !string.IsNullOrWhiteSpace(InvoiceId) &&
                   Amount > 0 &&
                   PaymentDate <= DateTime.UtcNow;
        }

        /// <summary>
        ///     Gets a formatted description of the payment method.
        /// </summary>
        /// <returns>A human-readable description of the payment method.</returns>
        public string GetPaymentMethodDescription()
        {
            return PaymentMethod switch
            {
                PaymentMethodType.CreditCard => "Credit Card",
                PaymentMethodType.Ach => "ACH Bank Transfer",
                PaymentMethodType.Check => "Check",
                PaymentMethodType.Cash => "Cash",
                PaymentMethodType.WireTransfer => "Wire Transfer",
                PaymentMethodType.DigitalWallet => "Digital Wallet",
                _ => "Unknown"
            };
        }

        /// <summary>
        ///     Gets a formatted description of the payment status.
        /// </summary>
        /// <returns>A human-readable description of the payment status.</returns>
        public string GetStatusDescription()
        {
            return Status switch
            {
                PaymentStatus.Pending => "Pending",
                PaymentStatus.Completed => "Completed",
                PaymentStatus.Failed => "Failed",
                PaymentStatus.Refunded => "Refunded",
                PaymentStatus.Processing => "Processing",
                PaymentStatus.Cancelled => "Cancelled",
                _ => "Unknown"
            };
        }

        /// <summary>
        ///     Checks if the payment is successful.
        /// </summary>
        /// <returns>True if the payment was completed successfully, false otherwise.</returns>
        public bool IsSuccessful()
        {
            return Status == PaymentStatus.Completed;
        }

        /// <summary>
        ///     Checks if the payment is in a final state (completed, failed, or cancelled).
        /// </summary>
        /// <returns>True if the payment is in a final state, false otherwise.</returns>
        public bool IsFinal()
        {
            return Status is PaymentStatus.Completed or
                PaymentStatus.Failed or
                PaymentStatus.Cancelled or
                PaymentStatus.Refunded;
        }

        /// <summary>
        ///     Creates a copy of the payment for retry attempts.
        /// </summary>
        /// <returns>A new payment with copied data for retry.</returns>
        public Payment CreateRetryCopy()
        {
            return new Payment
            {
                InvoiceId = InvoiceId,
                Amount = Amount,
                PaymentMethod = PaymentMethod,
                Status = PaymentStatus.Pending
            };
        }
    }
}
