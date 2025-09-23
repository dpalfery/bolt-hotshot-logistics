// <copyright file="BillingController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Api.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using HotshotLogistics.Contracts.Models;
    using HotshotLogistics.Contracts.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// API controller for billing and financial operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BillingController : ControllerBase
    {
        private readonly IBillingService billingService;
        private readonly ILogger<BillingController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BillingController"/> class.
        /// </summary>
        /// <param name="billingService">The billing service.</param>
        /// <param name="logger">The logger.</param>
        public BillingController(
            IBillingService billingService,
            ILogger<BillingController> logger)
        {
            this.billingService = billingService ?? throw new ArgumentNullException(nameof(billingService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates an invoice for a completed job.
        /// </summary>
        /// <param name="jobId">The job ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The generated invoice.</returns>
        [HttpPost("invoices/generate/{jobId}")]
        [ProducesResponseType(typeof(IInvoice), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IInvoice>> GenerateInvoice(string jobId, CancellationToken cancellationToken = default)
        {
            try
            {
                var invoice = await billingService.GenerateInvoiceAsync(jobId, cancellationToken);
                return CreatedAtAction(
                    nameof(GetInvoice),
                    new { id = invoice.Id },
                    invoice);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, "Invalid job ID provided for invoice generation: {JobId}", jobId);
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogWarning(ex, "Job not found for invoice generation: {JobId}", jobId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while generating invoice for job {JobId}", jobId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Gets an invoice by ID.
        /// </summary>
        /// <param name="id">The invoice ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The invoice if found; otherwise, 404 Not Found.</returns>
        [HttpGet("invoices/{id}")]
        [ProducesResponseType(typeof(IInvoice), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public Task<ActionResult<IInvoice>> GetInvoice(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                // This would need to be implemented in the billing service
                // For now, return NotFound as placeholder
                return Task.FromResult<ActionResult<IInvoice>>(NotFound($"Invoice with ID {id} not found"));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving invoice {InvoiceId}", id);
                return Task.FromResult<ActionResult<IInvoice>>(StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request."));
            }
        }

        /// <summary>
        /// Gets invoices for a specific customer.
        /// </summary>
        /// <param name="customerId">The customer ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of invoices for the customer.</returns>
        [HttpGet("invoices/customer/{customerId}")]
        [ProducesResponseType(typeof(IEnumerable<IInvoice>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<IInvoice>>> GetCustomerInvoices(string customerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var invoices = await billingService.GetCustomerInvoicesAsync(customerId, cancellationToken);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving invoices for customer {CustomerId}", customerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Gets overdue invoices.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A list of overdue invoices.</returns>
        [HttpGet("invoices/overdue")]
        [ProducesResponseType(typeof(IEnumerable<IInvoice>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<IInvoice>>> GetOverdueInvoices(CancellationToken cancellationToken = default)
        {
            try
            {
                var invoices = await billingService.GetOverdueInvoicesAsync(cancellationToken);
                return Ok(invoices);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving overdue invoices");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Processes a payment for an invoice.
        /// </summary>
        /// <param name="invoiceId">The invoice ID.</param>
        /// <param name="request">The payment request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Success status of the payment processing.</returns>
        [HttpPost("invoices/{invoiceId}/payments")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentResult>> ProcessPayment(
            string invoiceId,
            [FromBody] ProcessPaymentRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Payment request is required");
                }

                if (request.Amount <= 0)
                {
                    return BadRequest("Payment amount must be greater than zero");
                }

                if (string.IsNullOrWhiteSpace(request.PaymentMethod))
                {
                    return BadRequest("Payment method is required");
                }

                var success = await billingService.ProcessPaymentAsync(
                    invoiceId,
                    request.Amount,
                    request.PaymentMethod,
                    cancellationToken);

                var result = new PaymentResult
                {
                    Success = success,
                    InvoiceId = invoiceId,
                    Amount = request.Amount,
                    PaymentMethod = request.PaymentMethod,
                    ProcessedAt = DateTime.UtcNow
                };

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, "Invalid payment request for invoice {InvoiceId}: {Message}", invoiceId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                logger.LogWarning(ex, "Invoice not found for payment processing: {InvoiceId}", invoiceId);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while processing payment for invoice {InvoiceId}", invoiceId);
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Calculates tax for a given amount and location.
        /// </summary>
        /// <param name="request">The tax calculation request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The calculated tax amount.</returns>
        [HttpPost("tax/calculate")]
        [ProducesResponseType(typeof(TaxCalculationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TaxCalculationResult>> CalculateTax(
            [FromBody] TaxCalculationRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest("Tax calculation request is required");
                }

                if (request.Amount <= 0)
                {
                    return BadRequest("Amount must be greater than zero");
                }

                if (string.IsNullOrWhiteSpace(request.State))
                {
                    return BadRequest("State is required for tax calculation");
                }

                var taxAmount = await billingService.CalculateTaxAsync(request.Amount, request.State, cancellationToken);

                var result = new TaxCalculationResult
                {
                    Amount = request.Amount,
                    State = request.State,
                    TaxAmount = taxAmount,
                    TotalAmount = request.Amount + taxAmount,
                    TaxRate = request.Amount > 0 ? taxAmount / request.Amount : 0
                };

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, "Invalid tax calculation request: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while calculating tax");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Gets accounts receivable report.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Accounts receivable summary.</returns>
        [HttpGet("reports/accounts-receivable")]
        [ProducesResponseType(typeof(AccountsReceivableReport), StatusCodes.Status200OK)]
        public async Task<ActionResult<AccountsReceivableReport>> GetAccountsReceivableReport(CancellationToken cancellationToken = default)
        {
            try
            {
                var overdueInvoices = await billingService.GetOverdueInvoicesAsync(cancellationToken);
                
                var report = new AccountsReceivableReport
                {
                    TotalOverdueAmount = overdueInvoices.Sum(i => i.BalanceDue),
                    OverdueInvoiceCount = overdueInvoices.Count(),
                    GeneratedAt = DateTime.UtcNow,
                    OverdueInvoices = overdueInvoices.Select(i => new OverdueInvoiceSummary
                    {
                        InvoiceId = i.Id,
                        InvoiceNumber = i.InvoiceNumber,
                        CustomerId = i.CustomerId,
                        Amount = i.TotalAmount,
                        BalanceDue = i.BalanceDue,
                        DueDate = i.DueDate,
                        DaysOverdue = (int)(DateTime.UtcNow - i.DueDate).TotalDays
                    }).ToList()
                };

                return Ok(report);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while generating accounts receivable report");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
    }

    /// <summary>
    /// Request model for processing payments.
    /// </summary>
    public class ProcessPaymentRequest
    {
        /// <summary>
        /// Gets or sets the payment amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the payment method.
        /// </summary>
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets optional payment reference.
        /// </summary>
        public string? Reference { get; set; }
    }

    /// <summary>
    /// Result model for payment processing.
    /// </summary>
    public class PaymentResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the payment was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the invoice ID.
        /// </summary>
        public string InvoiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the payment amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the payment method.
        /// </summary>
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the processing timestamp.
        /// </summary>
        public DateTime ProcessedAt { get; set; }
    }

    /// <summary>
    /// Request model for tax calculations.
    /// </summary>
    public class TaxCalculationRequest
    {
        /// <summary>
        /// Gets or sets the amount to calculate tax for.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the state for tax calculation.
        /// </summary>
        public string State { get; set; } = string.Empty;
    }

    /// <summary>
    /// Result model for tax calculations.
    /// </summary>
    public class TaxCalculationResult
    {
        /// <summary>
        /// Gets or sets the original amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        public string State { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the calculated tax amount.
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Gets or sets the total amount including tax.
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the tax rate applied.
        /// </summary>
        public decimal TaxRate { get; set; }
    }

    /// <summary>
    /// Report model for accounts receivable.
    /// </summary>
    public class AccountsReceivableReport
    {
        /// <summary>
        /// Gets or sets the total overdue amount.
        /// </summary>
        public decimal TotalOverdueAmount { get; set; }

        /// <summary>
        /// Gets or sets the count of overdue invoices.
        /// </summary>
        public int OverdueInvoiceCount { get; set; }

        /// <summary>
        /// Gets or sets the report generation timestamp.
        /// </summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// Gets or sets the list of overdue invoices.
        /// </summary>
        public List<OverdueInvoiceSummary> OverdueInvoices { get; set; } = new List<OverdueInvoiceSummary>();
    }

    /// <summary>
    /// Summary model for overdue invoices.
    /// </summary>
    public class OverdueInvoiceSummary
    {
        /// <summary>
        /// Gets or sets the invoice ID.
        /// </summary>
        public string InvoiceId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the invoice number.
        /// </summary>
        public string InvoiceNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the customer ID.
        /// </summary>
        public string CustomerId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the invoice amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the balance due.
        /// </summary>
        public decimal BalanceDue { get; set; }

        /// <summary>
        /// Gets or sets the due date.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Gets or sets the number of days overdue.
        /// </summary>
        public int DaysOverdue { get; set; }
    }
}