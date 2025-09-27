using FluentValidation;
using HotshotLogistics.Contracts.Models;

namespace HotshotLogistics.Application.Validators;

/// <summary>
/// Validator for job creation requests.
/// </summary>
public class CreateJobValidator : AbstractValidator<JobDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateJobValidator"/> class.
    /// </summary>
    public CreateJobValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Job title is required.")
            .MaximumLength(200).WithMessage("Job title cannot exceed 200 characters.");

        RuleFor(x => x.PickupAddress)
            .NotEmpty().WithMessage("Pickup address is required.")
            .MaximumLength(500).WithMessage("Pickup address cannot exceed 500 characters.");

        RuleFor(x => x.DropoffAddress)
            .NotEmpty().WithMessage("Dropoff address is required.")
            .MaximumLength(500).WithMessage("Dropoff address cannot exceed 500 characters.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Job amount must be greater than zero.")
            .LessThanOrEqualTo(100000).WithMessage("Job amount cannot exceed $100,000.");

        RuleFor(x => x.ScheduledPickupTime)
            .GreaterThan(DateTime.UtcNow.AddMinutes(-30)).WithMessage("Scheduled pickup time must be in the future.")
            .LessThan(DateTime.UtcNow.AddDays(365)).WithMessage("Scheduled pickup time cannot be more than 365 days in the future.");

        RuleFor(x => x.EstimatedDeliveryTime)
            .GreaterThan(x => x.ScheduledPickupTime).WithMessage("Estimated delivery time must be after scheduled pickup time.")
            .LessThan(x => x.ScheduledPickupTime.AddDays(30)).WithMessage("Estimated delivery time cannot be more than 30 days after pickup.");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("Customer ID is required.");

        RuleFor(x => x.PickupLocation)
            .NotNull().WithMessage("Pickup location is required.")
            .SetValidator(new LocationValidator());

        RuleFor(x => x.DeliveryLocation)
            .NotNull().WithMessage("Delivery location is required.")
            .SetValidator(new LocationValidator());

        RuleFor(x => x.Cargo)
            .NotNull().WithMessage("Cargo details are required.")
            .SetValidator(new CargoDetailsValidator());

        RuleFor(x => x.Pricing)
            .NotNull().WithMessage("Pricing details are required.")
            .SetValidator(new PricingDetailsValidator());

        RuleFor(x => x.SpecialInstructions)
            .MaximumLength(1000).WithMessage("Special instructions cannot exceed 1000 characters.");
    }
}

/// <summary>
/// Validator for location coordinates.
/// </summary>
public class LocationValidator : AbstractValidator<Location>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocationValidator"/> class.
    /// </summary>
    public LocationValidator()
    {
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90 degrees.")
            .When(x => x.Latitude.HasValue);

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180 degrees.")
            .When(x => x.Longitude.HasValue);

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(500).WithMessage("Address cannot exceed 500 characters.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required.")
            .MaximumLength(100).WithMessage("City cannot exceed 100 characters.");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required.")
            .MaximumLength(50).WithMessage("State cannot exceed 50 characters.");

        RuleFor(x => x.PostalCode)
            .NotEmpty().WithMessage("Postal code is required.")
            .MaximumLength(20).WithMessage("Postal code cannot exceed 20 characters.");

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .MaximumLength(50).WithMessage("Country cannot exceed 50 characters.");
    }
}

/// <summary>
/// Validator for cargo details.
/// </summary>
public class CargoDetailsValidator : AbstractValidator<CargoDetails>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CargoDetailsValidator"/> class.
    /// </summary>
    public CargoDetailsValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Cargo description is required.")
            .MaximumLength(500).WithMessage("Cargo description cannot exceed 500 characters.");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("Cargo weight must be greater than zero.")
            .LessThanOrEqualTo(50000).WithMessage("Cargo weight cannot exceed 50,000 lbs.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.")
            .LessThanOrEqualTo(1000).WithMessage("Quantity cannot exceed 1,000 pieces.");

        RuleFor(x => x.Value)
            .GreaterThanOrEqualTo(0).WithMessage("Cargo value cannot be negative.")
            .LessThanOrEqualTo(1000000).WithMessage("Cargo value cannot exceed $1,000,000.");

        RuleFor(x => x.Dimensions)
            .MaximumLength(100).WithMessage("Dimensions description cannot exceed 100 characters.");

        RuleFor(x => x.TemperatureRange)
            .MaximumLength(50).WithMessage("Temperature range cannot exceed 50 characters.");

        RuleFor(x => x.SpecialInstructions)
            .MaximumLength(500).WithMessage("Special instructions cannot exceed 500 characters.");

        RuleFor(x => x.PackagingType)
            .MaximumLength(100).WithMessage("Packaging type cannot exceed 100 characters.");
    }
}


/// <summary>
/// Validator for pricing details.
/// </summary>
public class PricingDetailsValidator : AbstractValidator<PricingDetails>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PricingDetailsValidator"/> class.
    /// </summary>
    public PricingDetailsValidator()
    {
        RuleFor(x => x.BaseRate)
            .GreaterThanOrEqualTo(0).WithMessage("Base rate cannot be negative.");

        RuleFor(x => x.MileageRate)
            .GreaterThanOrEqualTo(0).WithMessage("Mileage rate cannot be negative.");

        RuleFor(x => x.FuelSurcharge)
            .GreaterThanOrEqualTo(0).WithMessage("Fuel surcharge cannot be negative.");

        RuleFor(x => x.TollCharges)
            .GreaterThanOrEqualTo(0).WithMessage("Toll charges cannot be negative.");

        RuleFor(x => x.AdditionalCharges)
            .GreaterThanOrEqualTo(0).WithMessage("Additional charges cannot be negative.");

        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Total amount cannot be negative.");

        RuleFor(x => x.Discount)
            .GreaterThanOrEqualTo(0).WithMessage("Discount cannot be negative.");

        RuleFor(x => x.Tax)
            .GreaterThanOrEqualTo(0).WithMessage("Tax cannot be negative.");

        RuleFor(x => x.TaxRate)
            .InclusiveBetween(0, 100).WithMessage("Tax rate must be between 0 and 100 percent.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter code.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.");
    }
}