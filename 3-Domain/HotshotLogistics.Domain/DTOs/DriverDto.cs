using HotshotLogistics.Core.Enums;
using HotshotLogistics.Domain.ValueObjects;

namespace HotshotLogistics.Domain.DTOs
{
    /// <summary>
    ///     Data Transfer Object for Driver information.
    /// </summary>
    public class DriverDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime LicenseExpiryDate { get; set; }
        public PersonalInfo PersonalInfo { get; set; } = new();
        public LicenseInfo License { get; set; } = new();
        public VehicleInfo Vehicle { get; set; } = new();
        public List<Certification> Certifications { get; set; } = new();
        public AvailabilitySchedule Availability { get; set; } = new();
        public PerformanceMetrics Performance { get; set; } = new();
        public PaymentInfo PaymentDetails { get; set; } = new();
        public bool IsActive { get; set; }
        public DriverStatus CurrentStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
