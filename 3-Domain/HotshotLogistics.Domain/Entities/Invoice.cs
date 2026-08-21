// <copyright file="Invoice.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using HotshotLogistics.Core.Enums;
using HotshotLogistics.Domain.ValueObjects;

namespace HotshotLogistics.Domain.Entities
{
    /// <summary>
    ///     Represents an invoice in the system.
    /// </summary>
    public class Invoice
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Invoice" /> class.
        /// </summary>
        public Invoice()
        {
            if (CreatedAt == default)
            {
                CreatedAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        ///     Gets or sets the invoice identifier.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        ///     Gets or sets the invoice number.
        /// </summary>
        public string InvoiceNumber { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the customer identifier.
        /// </summary>
        public string CustomerId { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the associated job identifier.
        /// </summary>
        public string? JobId { get; set; }

        /// <summary>
        ///     Gets or sets the invoice date.
        /// </summary>
        public DateTime InvoiceDate { get; set; }

        /// <summary>
        ///     Gets or sets the payment due date.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        ///     Gets or sets the invoice status.
        /// </summary>
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;

        /// <summary>
        ///     Gets or sets the invoice line items.
        /// </summary>
        public List<InvoiceLineItem> LineItems { get; set; } = new();

        /// <summary>
        ///     Gets or sets the subtotal amount before tax and discount.
        /// </summary>
        public decimal SubTotal { get; set; }

        /// <summary>
        ///     Gets or sets the tax rate applied to taxable items.
        /// </summary>
        public decimal TaxRate { get; set; }

        /// <summary>
        ///     Gets or sets the total tax amount.
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        ///     Gets or sets the discount amount applied.
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        ///     Gets or sets the total invoice amount.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        ///     Gets or sets the total amount paid toward the invoice.
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        ///     Gets the remaining balance due on the invoice.
        /// </summary>
        public decimal BalanceDue => TotalAmount - PaidAmount;

        /// <summary>
        ///     Gets or sets the payment terms.
        /// </summary>
        public PaymentTerms Terms { get; set; } = new();

        /// <summary>
        ///     Gets or sets additional notes or remarks.
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        ///     Gets or sets the last updated timestamp.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        ///     Adds a line item to the invoice.
        /// </summary>
        /// <param name="lineItem">The line item to add.</param>
        public void AddLineItem(InvoiceLineItem lineItem)
        {
            if (lineItem == null)
            {
                throw new ArgumentNullException(nameof(lineItem));
            }

            lineItem.SortOrder = LineItems.Count + 1;
            LineItems.Add(lineItem);
            CalculateTotals();
        }

        /// <summary>
        ///     Removes a line item from the invoice.
        /// </summary>
        /// <param name="lineItemId">The line item ID to remove.</param>
        /// <returns>True if the item was removed, false otherwise.</returns>
        public bool RemoveLineItem(int lineItemId)
        {
            InvoiceLineItem? item = LineItems.FirstOrDefault(li => li.Id == lineItemId);
            if (item != null)
            {
                LineItems.Remove(item);
                CalculateTotals();
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Calculates the invoice totals based on line items.
        /// </summary>
        public void CalculateTotals()
        {
            SubTotal = LineItems.Sum(li => li.Amount);

            decimal taxableAmount = LineItems.Where(li => li.TaxApplicable).Sum(li => li.Amount);
            TaxAmount = taxableAmount * TaxRate;

            TotalAmount = SubTotal + TaxAmount - DiscountAmount;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        ///     Applies a discount to the invoice.
        /// </summary>
        /// <param name="discountAmount">The discount amount.</param>
        public void ApplyDiscount(decimal discountAmount)
        {
            if (discountAmount < 0)
            {
                throw new ArgumentException("Discount amount cannot be negative", nameof(discountAmount));
            }

            DiscountAmount = Math.Min(discountAmount, SubTotal);
            CalculateTotals();
        }

        /// <summary>
        ///     Applies a payment to the invoice.
        /// </summary>
        /// <param name="paymentAmount">The payment amount.</param>
        public void ApplyPayment(decimal paymentAmount)
        {
            if (paymentAmount < 0)
            {
                throw new ArgumentException("Payment amount cannot be negative", nameof(paymentAmount));
            }

            if (paymentAmount > BalanceDue)
            {
                throw new ArgumentException("Payment amount cannot exceed balance due", nameof(paymentAmount));
            }

            PaidAmount += paymentAmount;

            // Update status based on payment
            if (BalanceDue == 0)
            {
                Status = InvoiceStatus.Paid;
            }
            else if (PaidAmount > 0)
            {
                Status = InvoiceStatus.PartiallyPaid;
            }

            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        ///     Checks if the invoice is overdue.
        /// </summary>
        /// <returns>True if the invoice is overdue, false otherwise.</returns>
        public bool IsOverdue()
        {
            return DateTime.UtcNow.Date > DueDate.Date && BalanceDue > 0;
        }

        /// <summary>
        ///     Gets the number of days overdue.
        /// </summary>
        /// <returns>The number of days overdue, or 0 if not overdue.</returns>
        public int DaysOverdue()
        {
            if (!IsOverdue())
            {
                return 0;
            }

            return (DateTime.UtcNow.Date - DueDate.Date).Days;
        }

        /// <summary>
        ///     Calculates early payment discount if applicable.
        /// </summary>
        /// <returns>The early payment discount amount.</returns>
        public decimal CalculateEarlyPaymentDiscount()
        {
            if (Terms.EarlyPaymentDiscount <= 0 || Terms.EarlyPaymentDiscountDays <= 0)
            {
                return 0;
            }

            DateTime discountDeadline = InvoiceDate.AddDays(Terms.EarlyPaymentDiscountDays);
            if (DateTime.UtcNow.Date <= discountDeadline.Date)
            {
                return TotalAmount * Terms.EarlyPaymentDiscount;
            }

            return 0;
        }

        /// <summary>
        ///     Calculates late payment penalty if applicable.
        /// </summary>
        /// <returns>The late payment penalty amount.</returns>
        public decimal CalculateLatePenalty()
        {
            if (!IsOverdue() || Terms.LatePaymentPenalty <= 0)
            {
                return 0;
            }

            DateTime penaltyStartDate = DueDate.AddDays(Terms.LatePaymentPenaltyDays);
            if (DateTime.UtcNow.Date > penaltyStartDate.Date)
            {
                return TotalAmount * Terms.LatePaymentPenalty;
            }

            return 0;
        }

        /// <summary>
        ///     Validates the invoice data.
        /// </summary>
        /// <returns>True if the invoice is valid, false otherwise.</returns>
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(CustomerId) &&
                   !string.IsNullOrWhiteSpace(InvoiceNumber) &&
                   InvoiceDate != default &&
                   DueDate >= InvoiceDate &&
                   LineItems.Any() &&
                   TotalAmount >= 0 &&
                   PaidAmount >= 0 &&
                   PaidAmount <= TotalAmount;
        }
    }
}
