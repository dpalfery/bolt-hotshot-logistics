namespace HotshotLogistics.Contracts.Models;

using HotshotLogistics.Contracts.Models;
/// <summary>
/// Data Transfer Object for Driver information.
/// </summary>
public class DriverDto
{
    /// <inheritdoc/>
    public int Id { get; set; }

    /// <inheritdoc/>
    public string FirstName { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string LastName { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string Email { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string LicenseNumber { get; set; } = string.Empty;

    /// <inheritdoc/>
    public DateTime LicenseExpiryDate { get; set; }

    /// <inheritdoc/>
    public bool IsActive { get; set; }

    /// <inheritdoc/>
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc/>
    public DateTime? UpdatedAt { get; set; }
}



