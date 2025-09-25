namespace HotshotLogistics.Contracts.Models;

/// <summary>
/// Represents a payment in the system.
/// </summary>
public interface IPayment
{
    /// <summary>
    /// Gets or sets the unique identifier for the payment.
    /// </summary>
    string Id { get; set; }

    /// <summary>
    /// Gets or sets the invoice identifier that this payment applies to.
    /// </summary>
    string InvoiceId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the payment was made.
    /// </summary>
    DateTime PaymentDate { get; set; }

    /// <summary>
    /// Gets or sets the payment amount.
    /// </summary>
    decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the payment method used.
    /// </summary>
    PaymentMethodType PaymentMethod { get; set; }

    /// <summary>
    /// Gets or sets the transaction identifier from the payment processor.
    /// </summary>
    string TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the response from the payment processor.
    /// </summary>
    string ProcessorResponse { get; set; }

    /// <summary>
    /// Gets or sets the current status of the payment.
    /// </summary>
    PaymentStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Represents payment method options for invoice payments.
/// </summary>
public enum PaymentMethodType
{
    /// <summary>
    /// Credit card payment.
    /// </summary>
    CreditCard = 0,

    /// <summary>
    /// ACH bank transfer.
    /// </summary>
    ACH = 1,

    /// <summary>
    /// Paper check.
    /// </summary>
    Check = 2,

    /// <summary>
    /// Cash payment.
    /// </summary>
    Cash = 3,

    /// <summary>
    /// Wire transfer.
    /// </summary>
    WireTransfer = 4,

    /// <summary>
    /// Digital wallet payment.
    /// </summary>
    DigitalWallet = 5
}

/// <summary>
/// Represents the status of a payment.
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment is pending processing.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Payment has been completed successfully.
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Payment has failed.
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Payment has been refunded.
    /// </summary>
    Refunded = 3,

    /// <summary>
    /// Payment is being processed.
    /// </summary>
    Processing = 4,

    /// <summary>
    /// Payment has been cancelled.
    /// </summary>
    Cancelled = 5
}
