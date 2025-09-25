namespace HotshotLogistics.Contracts.Models;

using System.Collections.Generic;

/// <summary>
/// Represents an invoice in the system.
/// </summary>
public interface IInvoice
{
    /// <summary>
    /// Gets or sets the unique identifier for the invoice.
    /// </summary>
    string Id { get; set; }

    /// <summary>
    /// Gets or sets the invoice number.
    /// </summary>
    string InvoiceNumber { get; set; }

    /// <summary>
    /// Gets or sets the customer identifier.
    /// </summary>
    string CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the job identifier (optional, for job-specific invoices).
    /// </summary>
    string? JobId { get; set; }

    /// <summary>
    /// Gets or sets the invoice date.
    /// </summary>
    DateTime InvoiceDate { get; set; }

    /// <summary>
    /// Gets or sets the due date.
    /// </summary>
    DateTime DueDate { get; set; }

    /// <summary>
    /// Gets or sets the current status of the invoice.
    /// </summary>
    InvoiceStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the list of line items.
    /// </summary>
    List<InvoiceLineItem> LineItems { get; set; }

    /// <summary>
    /// Gets or sets the subtotal amount before tax and discounts.
    /// </summary>
    decimal SubTotal { get; set; }

    /// <summary>
    /// Gets or sets the tax rate (as a decimal, e.g., 0.08 for 8%).
    /// </summary>
    decimal TaxRate { get; set; }

    /// <summary>
    /// Gets or sets the tax amount.
    /// </summary>
    decimal TaxAmount { get; set; }

    /// <summary>
    /// Gets or sets the discount amount.
    /// </summary>
    decimal DiscountAmount { get; set; }

    /// <summary>
    /// Gets or sets the total amount including tax.
    /// </summary>
    decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the amount that has been paid.
    /// </summary>
    decimal PaidAmount { get; set; }

    /// <summary>
    /// Gets the balance due (calculated property).
    /// </summary>
    decimal BalanceDue { get; }

    /// <summary>
    /// Gets or sets the payment terms.
    /// </summary>
    PaymentTerms Terms { get; set; }

    /// <summary>
    /// Gets or sets any notes or special instructions.
    /// </summary>
    string Notes { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Determines if the invoice is overdue.
    /// </summary>
    /// <returns>True if the invoice is overdue, false otherwise.</returns>
    bool IsOverdue()
    {
        return DateTime.UtcNow > DueDate && Status != InvoiceStatus.Paid && Status != InvoiceStatus.Cancelled;
    }
}

/// <summary>
/// Represents a line item on an invoice.
/// </summary>
public class InvoiceLineItem
{
    /// <summary>
    /// Gets or sets the unique identifier for the line item.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the description of the item or service.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets the total amount for this line item (calculated property).
    /// </summary>
    public decimal Amount => Quantity * UnitPrice;

    /// <summary>
    /// Gets or sets a value indicating whether tax applies to this item.
    /// </summary>
    public bool TaxApplicable { get; set; } = true;

    /// <summary>
    /// Gets or sets the sort order for display.
    /// </summary>
    public int SortOrder { get; set; }
}

/// <summary>
/// Represents payment terms for an invoice.
/// </summary>
public class PaymentTerms
{
    /// <summary>
    /// Gets or sets the payment terms in days (e.g., NET 15, NET 30).
    /// </summary>
    public int Days { get; set; }

    /// <summary>
    /// Gets or sets the early payment discount percentage.
    /// </summary>
    public decimal EarlyPaymentDiscount { get; set; }

    /// <summary>
    /// Gets or sets the early payment discount days.
    /// </summary>
    public int EarlyPaymentDiscountDays { get; set; }

    /// <summary>
    /// Gets or sets the late payment penalty percentage.
    /// </summary>
    public decimal LatePaymentPenalty { get; set; }

    /// <summary>
    /// Gets or sets the late payment penalty days.
    /// </summary>
    public int LatePaymentPenaltyDays { get; set; }
}

/// <summary>
/// Represents the status of an invoice.
/// </summary>
public enum InvoiceStatus
{
    /// <summary>
    /// Invoice is in draft state.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// Invoice has been sent to the customer.
    /// </summary>
    Sent = 1,

    /// <summary>
    /// Invoice has been viewed by the customer.
    /// </summary>
    Viewed = 2,

    /// <summary>
    /// Invoice has been paid in full.
    /// </summary>
    Paid = 3,

    /// <summary>
    /// Invoice is overdue.
    /// </summary>
    Overdue = 4,

    /// <summary>
    /// Invoice has been partially paid.
    /// </summary>
    PartiallyPaid = 5,

    /// <summary>
    /// Invoice has been cancelled.
    /// </summary>
    Cancelled = 6,

    /// <summary>
    /// Invoice is under dispute.
    /// </summary>
    Disputed = 7
}
