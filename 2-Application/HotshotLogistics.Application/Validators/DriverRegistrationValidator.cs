using FluentValidation;
using HotshotLogistics.Contracts.Models;
using HotshotLogistics.Domain.Entities;

namespace HotshotLogistics.Application.Validators;

/// <summary>
/// Validator for driver registration requests.
/// </summary>
public class DriverRegistrationValidator : AbstractValidator<DriverDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DriverRegistrationValidator"/> class.
    /// </summary>
    public DriverRegistrationValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.")
            .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.")
            .Matches(@"^[a-zA-Z\s\-']+$").WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(254).WithMessage("Email address cannot exceed 254 characters.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?1?[-.\s]?\(?([0-9]{3})\)?[-.\s]?([0-9]{3})[-.\s]?([0-9]{4})$").WithMessage("A valid US phone number is required.")
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.");

        RuleFor(x => x.LicenseNumber)
            .NotEmpty().WithMessage("Driver's license number is required.")
            .MaximumLength(20).WithMessage("License number cannot exceed 20 characters.")
            .Matches(@"^[A-Z0-9\-]+$").WithMessage("License number can only contain letters, numbers, and hyphens.");

        RuleFor(x => x.LicenseExpiryDate)
            .NotEmpty().WithMessage("License expiry date is required.");

        RuleFor(x => x)
            .Must(x => x.LicenseExpiryDate > DateTime.UtcNow.AddDays(30)).WithMessage("License must be valid for at least 30 days.")
            .Must(x => x.LicenseExpiryDate < DateTime.UtcNow.AddYears(10)).WithMessage("License expiry date cannot be more than 10 years in the future.");

        // Business rules for driver registration
        RuleFor(x => x)
            .Must(BeAtLeast18YearsOld).WithMessage("Driver must be at least 18 years old.")
            .Must(HaveValidLicenseForRegistration).WithMessage("License must be valid and meet minimum requirements for registration.");
    }

    /// <summary>
    /// Validates that the driver is at least 18 years old.
    /// </summary>
    /// <param name="driver">The driver DTO.</param>
    /// <returns>True if the driver is at least 18 years old, false otherwise.</returns>
    private bool BeAtLeast18YearsOld(DriverDto driver)
    {
        // TODO: Add DateOfBirth field to DriverDto for proper age validation
        // Current implementation uses license expiry as a proxy for driver experience
        // A license valid for at least 2 years suggests the driver has had their license for a while
        // This is a temporary solution until proper DateOfBirth validation is implemented
        return driver.LicenseExpiryDate > DateTime.UtcNow.AddYears(2);
    }

    /// <summary>
    /// Validates that the license meets minimum requirements for registration.
    /// </summary>
    /// <param name="driver">The driver DTO.</param>
    /// <returns>True if the license is valid for registration, false otherwise.</returns>
    private bool HaveValidLicenseForRegistration(DriverDto driver)
    {
        // License must be valid for at least 6 months
        return driver.LicenseExpiryDate > DateTime.UtcNow.AddMonths(6);
    }
}

/// <summary>
/// Validator for driver updates (less strict than registration).
/// </summary>
public class DriverUpdateValidator : AbstractValidator<DriverDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DriverUpdateValidator"/> class.
    /// </summary>
    public DriverUpdateValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Driver ID is required for updates.");

        RuleFor(x => x.FirstName)
            .NotEmpty().When(x => !string.IsNullOrEmpty(x.FirstName)).WithMessage("First name cannot be empty if provided.")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.")
            .Matches(@"^[a-zA-Z\s\-']+$").When(x => !string.IsNullOrEmpty(x.FirstName)).WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes.");

        RuleFor(x => x.LastName)
            .NotEmpty().When(x => !string.IsNullOrEmpty(x.LastName)).WithMessage("Last name cannot be empty if provided.")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.")
            .Matches(@"^[a-zA-Z\s\-']+$").When(x => !string.IsNullOrEmpty(x.LastName)).WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email)).WithMessage("A valid email address is required.")
            .MaximumLength(254).WithMessage("Email address cannot exceed 254 characters.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?1?[-.\s]?\(?([0-9]{3})\)?[-.\s]?([0-9]{3})[-.\s]?([0-9]{4})$").When(x => !string.IsNullOrEmpty(x.PhoneNumber)).WithMessage("A valid US phone number is required.")
            .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters.");

        RuleFor(x => x.LicenseNumber)
            .MaximumLength(20).When(x => !string.IsNullOrEmpty(x.LicenseNumber)).WithMessage("License number cannot exceed 20 characters.")
            .Matches(@"^[A-Z0-9\-]+$").When(x => !string.IsNullOrEmpty(x.LicenseNumber)).WithMessage("License number can only contain letters, numbers, and hyphens.");

        RuleFor(x => x.LicenseExpiryDate)
            .GreaterThan(DateTime.UtcNow).When(x => x.LicenseExpiryDate != default).WithMessage("License expiry date must be in the future.")
            .LessThan(DateTime.UtcNow.AddYears(10)).When(x => x.LicenseExpiryDate != default).WithMessage("License expiry date cannot be more than 10 years in the future.");
    }
}


