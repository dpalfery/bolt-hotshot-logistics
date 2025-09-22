namespace HotshotLogistics.Domain.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using HotshotLogistics.Contracts.Models;

/// <summary>
/// Represents an invoice in the system.
/// </summary>
public class Invoice : IInvoice
{
    /// <inheritdoc/>
    public string Id { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string CustomerId { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string? JobId { get; set; }

    /// <inheritdoc/>
    public DateTime InvoiceDate { get; set; }

    /// <inheritdoc/>
    public DateTime DueDate { get; set; }

    /// <inheritdoc/>
    public InvoiceStatus Status { get; set; }

    /// <inheritdoc/>
    public List<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();

    /// <inheritdoc/>
    public decimal SubTotal { get; set; }

    /// <inheritdoc/>
    public decimal TaxRate { get; set; }

    /// <inheritdoc/>
    public decimal TaxAmount { get; set; }

    /// <inheritdoc/>
    public decimal DiscountAmount { get; set; }

    /// <inheritdoc/>
    public decimal TotalAmount { get; set; }

    /// <inheritdoc/>
    public decimal PaidAmount { get; set; }

    /// <inheritdoc/>
    public decimal BalanceDue => TotalAmount - PaidAmount;

    /// <inheritdoc/>
    public PaymentTerms Terms { get; set; } = new PaymentTerms();

    /// <inheritdoc/>
    public string Notes { get; set; } = string.Empty;

    /// <inheritdoc/>
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc/>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Invoice"/> class.
    /// </summary>
    public Invoice()
    {
        if (CreatedAt == default)
            CreatedAt = DateTime.UtcNow;

        if (InvoiceDate == default)
            InvoiceDate = DateTime.UtcNow.Date;

        if (DueDate == default)
            DueDate = InvoiceDate.AddDays(Terms.Days);
    }

    /// <summary>
    /// Calculates the totals for the invoice.
    /// </summary>
    public void CalculateTotals()
    {
        SubTotal = LineItems.Sum(item => item.Amount);
        TaxAmount = LineItems.Where(item => item.TaxApplicable).Sum(item => item.Amount * TaxRate);
        TotalAmount = SubTotal + TaxAmount - DiscountAmount;
    }

    /// <summary>
    /// Adds a line item to the invoice.
    /// </summary>
    /// <param name="lineItem">The line item to add.</param>
    public void AddLineItem(InvoiceLineItem lineItem)
    {
        lineItem.SortOrder = LineItems.Count + 1;
        LineItems.Add(lineItem);
        CalculateTotals();
    }

    /// <summary>
    /// Removes a line item from the invoice.
    /// </summary>
    /// <param name="lineItemId">The ID of the line item to remove.</param>
    public void RemoveLineItem(int lineItemId)
    {
        var lineItem = LineItems.FirstOrDefault(item => item.Id == lineItemId);
        if (lineItem != null)
        {
            LineItems.Remove(lineItem);
            // Reorder remaining items
            for (int i = 0; i < LineItems.Count; i++)
            {
                LineItems[i].SortOrder = i + 1;
            }
            CalculateTotals();
        }
    }

    /// <summary>
    /// Updates a line item on the invoice.
    /// </summary>
    /// <param name="lineItemId">The ID of the line item to update.</param>
    /// <param name="updatedItem">The updated line item data.</param>
    public void UpdateLineItem(int lineItemId, InvoiceLineItem updatedItem)
    {
        var existingItem = LineItems.FirstOrDefault(item => item.Id == lineItemId);
        if (existingItem != null)
        {
            existingItem.Description = updatedItem.Description;
            existingItem.Quantity = updatedItem.Quantity;
            existingItem.UnitPrice = updatedItem.UnitPrice;
            existingItem.TaxApplicable = updatedItem.TaxApplicable;
            CalculateTotals();
        }
    }

    /// <summary>
    /// Applies a payment to the invoice.
    /// </summary>
    /// <param name="paymentAmount">The amount to apply.</param>
    /// <returns>The remaining balance after payment.</returns>
    public decimal ApplyPayment(decimal paymentAmount)
    {
        PaidAmount += paymentAmount;
        if (PaidAmount >= TotalAmount)
        {
            Status = InvoiceStatus.Paid;
            PaidAmount = TotalAmount; // Don't allow overpayment
        }
        else if (PaidAmount > 0)
        {
            Status = InvoiceStatus.PartiallyPaid;
        }

        UpdatedAt = DateTime.UtcNow;
        return BalanceDue;
    }

    /// <summary>
    /// Marks the invoice as sent to the customer.
    /// </summary>
    public void MarkAsSent()
    {
        Status = InvoiceStatus.Sent;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the invoice as viewed by the customer.
    /// </summary>
    public void MarkAsViewed()
    {
        if (Status == InvoiceStatus.Sent)
        {
            Status = InvoiceStatus.Viewed;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Cancels the invoice.
    /// </summary>
    public void Cancel()
    {
        Status = InvoiceStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if the invoice is overdue.
    /// </summary>
    /// <returns>True if the invoice is overdue, false otherwise.</returns>
    public bool IsOverdue()
    {
        return Status != InvoiceStatus.Paid &&
               Status != InvoiceStatus.Cancelled &&
               DateTime.UtcNow.Date > DueDate.Date;
    }

    /// <summary>
    /// Gets the number of days overdue.
    /// </summary>
    /// <returns>The number of days overdue, or 0 if not overdue.</returns>
    public int GetDaysOverdue()
    {
        if (!IsOverdue())
            return 0;

        return (DateTime.UtcNow.Date - DueDate.Date).Days;
    }

    /// <summary>
    /// Calculates the early payment discount amount.
    /// </summary>
    /// <param name="paymentDate">The proposed payment date.</param>
    /// <returns>The discount amount available.</returns>
    public decimal CalculateEarlyPaymentDiscount(DateTime paymentDate)
    {
        if (Terms.EarlyPaymentDiscount <= 0 || Terms.EarlyPaymentDiscountDays <= 0)
            return 0;

        var daysFromInvoice = (paymentDate.Date - InvoiceDate.Date).Days;
        if (daysFromInvoice <= Terms.EarlyPaymentDiscountDays)
        {
            return TotalAmount * Terms.EarlyPaymentDiscount;
        }

        return 0;
    }

    /// <summary>
    /// Calculates the late payment penalty.
    /// </summary>
    /// <returns>The penalty amount.</returns>
    public decimal CalculateLatePaymentPenalty()
    {
        if (Terms.LatePaymentPenalty <= 0 || Terms.LatePaymentPenaltyDays <= 0)
            return 0;

        var daysOverdue = GetDaysOverdue();
        if (daysOverdue > Terms.LatePaymentPenaltyDays)
        {
            return BalanceDue * Terms.LatePaymentPenalty;
        }

        return 0;
    }

    /// <summary>
    /// Validates the invoice data.
    /// </summary>
    /// <returns>True if the invoice data is valid, false otherwise.</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(Id) &&
               !string.IsNullOrWhiteSpace(InvoiceNumber) &&
               !string.IsNullOrWhiteSpace(CustomerId) &&
               InvoiceDate <= DateTime.UtcNow.Date &&
               DueDate >= InvoiceDate &&
               LineItems.Any() &&
               TotalAmount >= 0 &&
               PaidAmount >= 0 &&
               PaidAmount <= TotalAmount;
    }

    /// <summary>
    /// Creates a copy of the invoice for adjustments.
    /// </summary>
    /// <returns>A new invoice with copied data.</returns>
    public Invoice CreateAdjustmentCopy()
    {
        return new Invoice
        {
            CustomerId = this.CustomerId,
            JobId = this.JobId,
            LineItems = this.LineItems.Select(item => new InvoiceLineItem
            {
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TaxApplicable = item.TaxApplicable,
                SortOrder = item.SortOrder
            }).ToList(),
            Terms = this.Terms,
            Notes = $"Adjustment for invoice {this.InvoiceNumber}"
        };
    }
}