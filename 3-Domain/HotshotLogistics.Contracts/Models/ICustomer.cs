namespace HotshotLogistics.Contracts.Models;

/// <summary>
/// Represents a customer interface for hotshot logistics services.
/// </summary>
public interface ICustomer
{
    /// <summary>
    /// Gets or sets the unique identifier for the customer.
    /// </summary>
    string Id { get; set; }

    /// <summary>
    /// Gets or sets the company name.
    /// </summary>
    string CompanyName { get; set; }

    /// <summary>
    /// Gets or sets the tax identification number.
    /// </summary>
    string? TaxId { get; set; }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    string? Email { get; set; }

    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    string? Phone { get; set; }

    /// <summary>
    /// Gets or sets the billing address.
    /// </summary>
    Address BillingAddress { get; set; }

    /// <summary>
    /// Gets or sets the list of contacts for the customer.
    /// </summary>
    List<Contact> Contacts { get; set; }

    /// <summary>
    /// Gets or sets the credit terms for the customer.
    /// </summary>
    CreditTerms CreditTerms { get; set; }

    /// <summary>
    /// Gets or sets the credit limit for the customer.
    /// </summary>
    decimal CreditLimit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the customer is active.
    /// </summary>
    bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the customer was created.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the customer was last updated.
    /// </summary>
    DateTime? UpdatedAt { get; set; }
}
