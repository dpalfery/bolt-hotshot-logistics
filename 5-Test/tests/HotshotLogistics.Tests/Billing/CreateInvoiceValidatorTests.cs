using FluentValidation.TestHelper;
using HotshotLogistics.Application.Validators;
using HotshotLogistics.Domain.Entities;

namespace HotshotLogistics.Tests.Billing
{
    /// <summary>
    ///     Unit tests for CreateInvoiceValidator.
    /// </summary>
    public class CreateInvoiceValidatorTests
    {
        private readonly CreateInvoiceValidator _validator;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CreateInvoiceValidatorTests" /> class.
        /// </summary>
        public CreateInvoiceValidatorTests()
        {
            _validator = new CreateInvoiceValidator();
        }

        /// <summary>
        ///     Tests that validation passes for a valid invoice.
        /// </summary>
        [Fact]
        public void Validate_ValidInvoice_ShouldPass()
        {
            // Arrange
            Invoice invoice = new()
            {
                InvoiceNumber = "INV-2024-001",
                CustomerId = "customer-123",
                InvoiceDate = DateTime.UtcNow.Date,
                DueDate = DateTime.UtcNow.Date.AddDays(30),
                LineItems =
                [
                    new InvoiceLineItem
                    {
                        Id = 1,
                        Description = "Test service",
                        Quantity = 1,
                        UnitPrice = 500.00m
                    }
                ],
                SubTotal = 500.00m,
                TaxRate = 0.08m,
                TaxAmount = 40.00m,
                TotalAmount = 540.00m,
                PaidAmount = 0,
                Terms = new PaymentTerms
                {
                    Days = 30
                }
            };

            // Act
            TestValidationResult<Invoice> result = _validator.TestValidate(invoice);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        /// <summary>
        ///     Tests that validation fails when invoice number is empty.
        /// </summary>
        [Fact]
        public void Validate_EmptyInvoiceNumber_ShouldFail()
        {
            // Arrange
            Invoice invoice = new() { InvoiceNumber = string.Empty };

            // Act
            TestValidationResult<Invoice> result = _validator.TestValidate(invoice);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.InvoiceNumber)
                .WithErrorMessage("Invoice number is required.");
        }

        /// <summary>
        ///     Tests that validation fails when invoice number format is invalid.
        /// </summary>
        [Fact]
        public void Validate_InvalidInvoiceNumberFormat_ShouldFail()
        {
            // Arrange
            Invoice invoice = new() { InvoiceNumber = "INVALID@NUMBER" };

            // Act
            TestValidationResult<Invoice> result = _validator.TestValidate(invoice);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.InvoiceNumber)
                .WithErrorMessage("Invoice number can only contain letters, numbers, and hyphens.");
        }

        /// <summary>
        ///     Tests that validation fails when customer ID is empty.
        /// </summary>
        [Fact]
        public void Validate_EmptyCustomerId_ShouldFail()
        {
            // Arrange
            Invoice invoice = new() { CustomerId = string.Empty };

            // Act
            TestValidationResult<Invoice> result = _validator.TestValidate(invoice);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CustomerId)
                .WithErrorMessage("Customer ID is required.");
        }

        /// <summary>
        ///     Tests that validation fails when invoice date is in the future.
        /// </summary>
        [Fact]
        public void Validate_FutureInvoiceDate_ShouldFail()
        {
            // Arrange
            Invoice invoice = new() { InvoiceDate = DateTime.UtcNow.Date.AddDays(1) };

            // Act
            TestValidationResult<Invoice> result = _validator.TestValidate(invoice);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.InvoiceDate)
                .WithErrorMessage("Invoice date cannot be in the future.");
        }

        /// <summary>
        ///     Tests that validation fails when due date is before invoice date.
        /// </summary>
        [Fact]
        public void Validate_DueDateBeforeInvoiceDate_ShouldFail()
        {
            // Arrange
            Invoice invoice = new()
            {
                InvoiceDate = DateTime.UtcNow.Date,
                DueDate = DateTime.UtcNow.Date.AddDays(-1)
            };

            // Act
            TestValidationResult<Invoice> result = _validator.TestValidate(invoice);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.DueDate)
                .WithErrorMessage("Due date must be after invoice date.");
        }

        /// <summary>
        ///     Tests that validation fails when no line items are present.
        /// </summary>
        [Fact]
        public void Validate_NoLineItems_ShouldFail()
        {
            // Arrange
            Invoice invoice = new() { LineItems = new List<InvoiceLineItem>() };

            // Act
            TestValidationResult<Invoice> result = _validator.TestValidate(invoice);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.LineItems)
                .WithErrorMessage("At least one line item is required.");
        }

        /// <summary>
        ///     Tests that validation fails when subtotal doesn't match line items.
        /// </summary>
        [Fact]
        public void Validate_IncorrectSubtotal_ShouldFail()
        {
            // Arrange
            Invoice invoice = new()
            {
                LineItems =
                [
                    new InvoiceLineItem { Quantity = 1, UnitPrice = 500.00m }
                ],
                SubTotal = 400.00m // Incorrect subtotal
            };

            // Act
            TestValidationResult<Invoice> result = _validator.TestValidate(invoice);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SubTotal)
                .WithErrorMessage("Subtotal must equal the sum of line item amounts.");
        }

        /// <summary>
        ///     Tests that validation fails when tax rate is invalid.
        /// </summary>
        [Fact]
        public void Validate_InvalidTaxRate_ShouldFail()
        {
            // Arrange
            Invoice invoice = new() { TaxRate = 1.5m }; // 150% tax rate

            // Act
            TestValidationResult<Invoice> result = _validator.TestValidate(invoice);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TaxRate)
                .WithErrorMessage("Tax rate must be between 0 and 100 percent.");
        }

        /// <summary>
        ///     Tests that validation fails when total amount is incorrect.
        /// </summary>
        [Fact]
        public void Validate_IncorrectTotalAmount_ShouldFail()
        {
            // Arrange
            Invoice invoice = new()
            {
                SubTotal = 500.00m,
                TaxAmount = 40.00m,
                DiscountAmount = 0,
                TotalAmount = 600.00m // Should be 540.00
            };

            // Act
            TestValidationResult<Invoice> result = _validator.TestValidate(invoice);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.TotalAmount)
                .WithErrorMessage("Total amount must be correctly calculated.");
        }

        /// <summary>
        ///     Tests that validation fails when paid amount exceeds total.
        /// </summary>
        [Fact]
        public void Validate_PaidAmountExceedsTotal_ShouldFail()
        {
            // Arrange
            Invoice invoice = new()
            {
                TotalAmount = 500.00m,
                PaidAmount = 600.00m
            };

            // Act
            TestValidationResult<Invoice> result = _validator.TestValidate(invoice);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.PaidAmount)
                .WithErrorMessage("Paid amount cannot exceed total amount.");
        }

        /// <summary>
        ///     Tests that validation fails when payment terms are null.
        /// </summary>
        [Fact]
        public void Validate_NullPaymentTerms_ShouldFail()
        {
            // Arrange
            Invoice invoice = new() { Terms = null! };

            // Act
            TestValidationResult<Invoice> result = _validator.TestValidate(invoice);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Terms)
                .WithErrorMessage("Payment terms are required.");
        }
    }
}
