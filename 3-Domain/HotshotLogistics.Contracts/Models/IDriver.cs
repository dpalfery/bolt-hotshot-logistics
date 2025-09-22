namespace HotshotLogistics.Contracts.Models;

using System.Collections.Generic;

/// <summary>
/// Represents a driver in the system.
/// </summary>
public interface IDriver
{
    /// <summary>
    /// Gets or sets the unique identifier for the driver.
    /// </summary>
    int Id { get; set; }

    /// <summary>
    /// Gets or sets the personal information for the driver.
    /// </summary>
    PersonalInfo PersonalInfo { get; set; }

    /// <summary>
    /// Gets or sets the license information for the driver.
    /// </summary>
    LicenseInfo License { get; set; }

    /// <summary>
    /// Gets or sets the vehicle information for the driver.
    /// </summary>
    VehicleInfo Vehicle { get; set; }

    /// <summary>
    /// Gets or sets the list of certifications for the driver.
    /// </summary>
    List<Certification> Certifications { get; set; }

    /// <summary>
    /// Gets or sets the availability schedule for the driver.
    /// </summary>
    AvailabilitySchedule Availability { get; set; }

    /// <summary>
    /// Gets or sets the performance metrics for the driver.
    /// </summary>
    PerformanceMetrics Performance { get; set; }

    /// <summary>
    /// Gets or sets the payment information for the driver.
    /// </summary>
    PaymentInfo PaymentDetails { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the driver is active.
    /// </summary>
    bool IsActive { get; set; }

    /// <summary>
    /// Gets or sets the current status of the driver.
    /// </summary>
    DriverStatus CurrentStatus { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp of the driver.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp of the driver.
    /// </summary>
    DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Represents personal information for a driver.
/// </summary>
public class PersonalInfo
{
    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the phone number.
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date of birth.
    /// </summary>
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Gets or sets the social security number (encrypted).
    /// </summary>
    public string SSN { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the emergency contact name.
    /// </summary>
    public string EmergencyContactName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the emergency contact phone number.
    /// </summary>
    public string EmergencyContactPhone { get; set; } = string.Empty;
}

/// <summary>
/// Represents license information for a driver.
/// </summary>
public class LicenseInfo
{
    /// <summary>
    /// Gets or sets the license number.
    /// </summary>
    public string LicenseNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the state that issued the license.
    /// </summary>
    public string LicenseState { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the license expiry date.
    /// </summary>
    public DateTime LicenseExpiryDate { get; set; }

    /// <summary>
    /// Gets or sets the license class.
    /// </summary>
    public string LicenseClass { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the license has endorsements.
    /// </summary>
    public List<string> Endorsements { get; set; } = new List<string>();
}

/// <summary>
/// Represents vehicle information for a driver.
/// </summary>
public class VehicleInfo
{
    /// <summary>
    /// Gets or sets the vehicle type.
    /// </summary>
    public string VehicleType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the vehicle make.
    /// </summary>
    public string Make { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the vehicle model.
    /// </summary>
    public string Model { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the vehicle year.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the license plate number.
    /// </summary>
    public string LicensePlateNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the VIN number.
    /// </summary>
    public string VIN { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the insurance policy number.
    /// </summary>
    public string InsurancePolicyNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the insurance expiry date.
    /// </summary>
    public DateTime InsuranceExpiryDate { get; set; }
}

/// <summary>
/// Represents a certification for a driver.
/// </summary>
public class Certification
{
    /// <summary>
    /// Gets or sets the certification name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the issuing authority.
    /// </summary>
    public string IssuingAuthority { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the certification number.
    /// </summary>
    public string CertificationNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the issue date.
    /// </summary>
    public DateTime IssueDate { get; set; }

    /// <summary>
    /// Gets or sets the expiry date.
    /// </summary>
    public DateTime? ExpiryDate { get; set; }
}

/// <summary>
/// Represents the availability schedule for a driver.
/// </summary>
public class AvailabilitySchedule
{
    /// <summary>
    /// Gets or sets the regular working hours.
    /// </summary>
    public List<WorkingHours> RegularHours { get; set; } = new List<WorkingHours>();

    /// <summary>
    /// Gets or sets the vacation days.
    /// </summary>
    public List<DateTime> VacationDays { get; set; } = new List<DateTime>();

    /// <summary>
    /// Gets or sets a value indicating whether the driver is available for emergency jobs.
    /// </summary>
    public bool AvailableForEmergency { get; set; }

    /// <summary>
    /// Gets or sets the preferred service areas.
    /// </summary>
    public List<string> PreferredServiceAreas { get; set; } = new List<string>();
}

/// <summary>
/// Represents working hours for a day.
/// </summary>
public class WorkingHours
{
    /// <summary>
    /// Gets or sets the day of the week.
    /// </summary>
    public DayOfWeek DayOfWeek { get; set; }

    /// <summary>
    /// Gets or sets the start time.
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time.
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the driver is available on this day.
    /// </summary>
    public bool IsAvailable { get; set; } = true;
}

/// <summary>
/// Represents performance metrics for a driver.
/// </summary>
public class PerformanceMetrics
{
    /// <summary>
    /// Gets or sets the on-time delivery rate (percentage).
    /// </summary>
    public decimal OnTimeDeliveryRate { get; set; }

    /// <summary>
    /// Gets or sets the average customer rating (1-5 scale).
    /// </summary>
    public decimal CustomerRating { get; set; }

    /// <summary>
    /// Gets or sets the number of completed jobs.
    /// </summary>
    public int CompletedJobs { get; set; }

    /// <summary>
    /// Gets or sets the number of cancelled jobs.
    /// </summary>
    public int CancelledJobs { get; set; }

    /// <summary>
    /// Gets or sets the total miles driven.
    /// </summary>
    public decimal TotalMilesDriven { get; set; }

    /// <summary>
    /// Gets or sets the average delivery time in hours.
    /// </summary>
    public decimal AverageDeliveryTime { get; set; }

    /// <summary>
    /// Gets or sets the safety incident count.
    /// </summary>
    public int SafetyIncidents { get; set; }
}

/// <summary>
/// Represents payment information for a driver.
/// </summary>
public class PaymentInfo
{
    /// <summary>
    /// Gets or sets the hourly rate.
    /// </summary>
    public decimal HourlyRate { get; set; }

    /// <summary>
    /// Gets or sets the per-mile rate.
    /// </summary>
    public decimal PerMileRate { get; set; }

    /// <summary>
    /// Gets or sets the bank account number (encrypted).
    /// </summary>
    public string BankAccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the routing number.
    /// </summary>
    public string RoutingNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tax identification number.
    /// </summary>
    public string TaxId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the payment method preference.
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }
}

/// <summary>
/// Represents the current status of a driver.
/// </summary>
public enum DriverStatus
{
    /// <summary>
    /// Driver is available for jobs.
    /// </summary>
    Available = 0,

    /// <summary>
    /// Driver is currently on a job.
    /// </summary>
    Busy = 1,

    /// <summary>
    /// Driver is offline/not available.
    /// </summary>
    Offline = 2,

    /// <summary>
    /// Driver is on break.
    /// </summary>
    OnBreak = 3
}

/// <summary>
/// Represents payment method options.
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Direct deposit to bank account.
    /// </summary>
    DirectDeposit = 0,

    /// <summary>
    /// Paper check.
    /// </summary>
    Check = 1,

    /// <summary>
    /// PayPal or digital wallet.
    /// </summary>
    DigitalWallet = 2
}
