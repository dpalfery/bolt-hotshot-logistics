using FluentValidation;
using HotshotLogistics.Contracts.Models;
using HotshotLogistics.Domain.Models;

namespace HotshotLogistics.Application.Validators;

/// <summary>
/// Validator for invoice creation requests.
/// </summary>
public class CreateInvoiceValidator : AbstractValidator<Invoice>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateInvoiceValidator"/> class.
    /// </summary>
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.InvoiceNumber)
            .NotEmpty().WithMessage("Invoice number is required.")
            .MaximumLength(50).WithMessage("Invoice number cannot exceed 50 characters.")
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("Invoice number can only contain letters, numbers, and hyphens.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.JobId)
            .NotEmpty().When(x => x.JobId != null).WithMessage("Job ID cannot be empty if provided.");

        RuleFor(x => x.InvoiceDate)
            .NotEmpty().WithMessage("Invoice date is required.")
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1)).WithMessage("Invoice date cannot be in the future.");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("Due date is required.")
            .GreaterThan(x => x.InvoiceDate).WithMessage("Due date must be after invoice date.")
            .LessThan(x => x.InvoiceDate.AddDays(365)).WithMessage("Due date cannot be more than 365 days after invoice date.");

        RuleFor(x => x.LineItems)
            .NotEmpty().WithMessage("At least one line item is required.")
            .Must(x => x.Count <= 100).WithMessage("Cannot have more than 100 line items.");

        RuleForEach(x => x.LineItems)
            .SetValidator(new InvoiceLineItemValidator());

        RuleFor(x => x.SubTotal)
            .GreaterThanOrEqualTo(0).WithMessage("Subtotal cannot be negative.")
            .Equal(x => x.LineItems.Sum(li => li.Amount)).WithMessage("Subtotal must equal the sum of line item amounts.");

        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0, 100).WithMessage("Tax rate must be between 0 and 100 percent.");

        RuleFor(x => x.TaxAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Tax amount cannot be negative.")
            .Equal(x => Math.Round(x.SubTotal * (x.TaxRate / 100), 2)).WithMessage("Tax amount must be correctly calculated.");

        RuleFor(x => x.DiscountAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount amount cannot be negative.")
            .LessThanOrEqualTo(x => x.SubTotal).WithMessage("Discount amount cannot exceed subtotal.");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("Total amount must be greater than zero.")
            .Equal(x => x.SubTotal + x.TaxAmount - x.DiscountAmount).WithMessage("Total amount must be correctly calculated.");

        RuleFor(x => x.PaidAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Paid amount cannot be negative.")
            .LessThanOrEqualTo(x => x.TotalAmount).WithMessage("Paid amount cannot exceed total amount.");

        RuleFor(x => x.BalanceDue)
            .Equal(x => x.TotalAmount - x.PaidAmount).WithMessage("Balance due must equal total amount minus paid amount.");

        RuleFor(x => x.Terms)
            .NotNull().WithMessage("Payment terms are required.")
            .SetValidator(new PaymentTermsValidator());

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes cannot exceed 1000 characters.");
    }
}

/// <summary>
/// Validator for invoice line items.
/// </summary>
public class InvoiceLineItemValidator : AbstractValidator<InvoiceLineItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceLineItemValidator"/> class.
    /// </summary>
    public InvoiceLineItemValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Line item ID must be greater than zero.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Line item description is required.")
            .MaximumLength(500).WithMessage("Line item description cannot exceed 500 characters.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.")
            .LessThanOrEqualTo(10000).WithMessage("Quantity cannot exceed 10,000.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.")
            .LessThanOrEqualTo(100000).WithMessage("Unit price cannot exceed $100,000.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Line item amount must be greater than zero.")
            .Equal(x => Math.Round(x.Quantity * x.UnitPrice, 2)).WithMessage("Amount must equal quantity times unit price.");

        RuleFor(x => x.TaxApplicable)
            .NotNull().WithMessage("Tax applicable flag is required.");

        RuleFor(x => x.SortOrder)
            .GreaterThan(0).WithMessage("Sort order must be greater than zero.");
    }
}

/// <summary>
/// Validator for payment terms.
/// </summary>
public class PaymentTermsValidator : AbstractValidator<PaymentTerms>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentTermsValidator"/> class.
    /// </summary>
    public PaymentTermsValidator()
    {
        RuleFor(x => x.Days)
            .GreaterThanOrEqualTo(0).WithMessage("Payment terms days cannot be negative.")
            .LessThanOrEqualTo(365).WithMessage("Payment terms days cannot exceed 365.");

        RuleFor(x => x.EarlyPaymentDiscount)
            .InclusiveBetween(0, 50).WithMessage("Early payment discount must be between 0 and 50 percent.");

        RuleFor(x => x.EarlyPaymentDiscountDays)
            .GreaterThanOrEqualTo(0).WithMessage("Early payment discount days cannot be negative.")
            .LessThanOrEqualTo(x => x.Days).WithMessage("Early payment discount days cannot exceed payment terms days.");

        RuleFor(x => x.LatePaymentPenalty)
            .InclusiveBetween(0, 50).WithMessage("Late payment penalty must be between 0 and 50 percent.");

        RuleFor(x => x.LatePaymentPenaltyDays)
            .GreaterThanOrEqualTo(0).WithMessage("Late payment penalty days cannot be negative.");
    }
}
