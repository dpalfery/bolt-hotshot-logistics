namespace HotshotLogistics.Contracts.Models;

/// <summary>
/// Represents a customer in the system.
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

/// <summary>
/// Represents a physical address.
/// </summary>
public class Address
{
    /// <summary>
    /// Gets or sets the street address.
    /// </summary>
    public string Street { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the city.
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the state or province.
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    public string ZipCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the country.
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the latitude coordinate.
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude coordinate.
    /// </summary>
    public double Longitude { get; set; }
}

/// <summary>
/// Represents a contact person for a customer.
/// </summary>
public class Contact
{
    /// <summary>
    /// Gets or sets the contact's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the contact's title/position.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the contact's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the contact's phone number.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this is the primary contact.
    /// </summary>
    public bool IsPrimary { get; set; }
}

/// <summary>
/// Represents credit terms for a customer.
/// </summary>
public class CreditTerms
{
    /// <summary>
    /// Gets or sets the payment terms in days (e.g., NET 15, NET 30).
    /// </summary>
    public int PaymentTermsDays { get; set; }

    /// <summary>
    /// Gets or sets the credit status.
    /// </summary>
    public CreditStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the date when credit was approved.
    /// </summary>
    public DateTime ApprovedDate { get; set; }

    /// <summary>
    /// Gets or sets the date when credit expires.
    /// </summary>
    public DateTime? ExpiryDate { get; set; }
}

/// <summary>
/// Represents the credit status of a customer.
/// </summary>
public enum CreditStatus
{
    /// <summary>
    /// Credit application is pending review.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Credit has been approved.
    /// </summary>
    Approved = 1,

    /// <summary>
    /// Credit has been denied.
    /// </summary>
    Denied = 2,

    /// <summary>
    /// Credit has been suspended.
    /// </summary>
    Suspended = 3
}