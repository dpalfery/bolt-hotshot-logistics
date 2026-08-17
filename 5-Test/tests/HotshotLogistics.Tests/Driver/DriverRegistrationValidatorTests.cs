using FluentValidation.TestHelper;
using HotshotLogistics.Application.Validators;

namespace HotshotLogistics.Tests.Drivers
{
    /// <summary>
    /// Unit tests for DriverRegistrationValidator.
    /// </summary>
    public class DriverRegistrationValidatorTests
    {
        private readonly DriverRegistrationValidator validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DriverRegistrationValidatorTests"/> class.
        /// </summary>
        public DriverRegistrationValidatorTests()
        {
            this.validator = new DriverRegistrationValidator();
        }

        /// <summary>
        /// Tests that validation passes for a valid driver registration.
        /// </summary>
        [Fact]
        public void Validate_ValidDriverRegistration_ShouldPass()
        {
            var driver = CreateValidDriver();

            var result = this.validator.TestValidate(driver);

            result.ShouldNotHaveAnyValidationErrors();
        }

        /// <summary>
        /// Tests that validation fails when first name is empty.
        /// </summary>
        [Fact]
        public void Validate_EmptyFirstName_ShouldFail()
        {
            var driver = CreateValidDriver();
            driver.PersonalInfo.FirstName = string.Empty;

            var result = this.validator.TestValidate(driver);

            result.ShouldHaveValidationErrorFor(x => x.PersonalInfo.FirstName)
                  .WithErrorMessage("First name is required.");
        }

        /// <summary>
        /// Tests that validation fails when first name contains invalid characters.
        /// </summary>
        [Fact]
        public void Validate_InvalidFirstNameCharacters_ShouldFail()
        {
            var driver = CreateValidDriver();
            driver.PersonalInfo.FirstName = "John123";

            var result = this.validator.TestValidate(driver);

            result.ShouldHaveValidationErrorFor(x => x.PersonalInfo.FirstName)
                  .WithErrorMessage("First name can only contain letters, spaces, hyphens, and apostrophes.");
        }

        /// <summary>
        /// Tests that validation fails when last name is empty.
        /// </summary>
        [Fact]
        public void Validate_EmptyLastName_ShouldFail()
        {
            var driver = CreateValidDriver();
            driver.PersonalInfo.LastName = string.Empty;

            var result = this.validator.TestValidate(driver);

            result.ShouldHaveValidationErrorFor(x => x.PersonalInfo.LastName)
                  .WithErrorMessage("Last name is required.");
        }

        /// <summary>
        /// Tests that validation fails when email is empty.
        /// </summary>
        [Fact]
        public void Validate_EmptyEmail_ShouldFail()
        {
            var driver = CreateValidDriver();
            driver.PersonalInfo.Email = string.Empty;

            var result = this.validator.TestValidate(driver);

            result.ShouldHaveValidationErrorFor(x => x.PersonalInfo.Email)
                  .WithErrorMessage("Email address is required.");
        }

        /// <summary>
        /// Tests that validation fails when email format is invalid.
        /// </summary>
        [Fact]
        public void Validate_InvalidEmailFormat_ShouldFail()
        {
            var driver = CreateValidDriver();
            driver.PersonalInfo.Email = "invalid-email";

            var result = this.validator.TestValidate(driver);

            result.ShouldHaveValidationErrorFor(x => x.PersonalInfo.Email)
                  .WithErrorMessage("A valid email address is required.");
        }

        /// <summary>
        /// Tests that validation fails when phone number is empty.
        /// </summary>
        [Fact]
        public void Validate_EmptyPhoneNumber_ShouldFail()
        {
            var driver = CreateValidDriver();
            driver.PersonalInfo.PhoneNumber = string.Empty;

            var result = this.validator.TestValidate(driver);

            result.ShouldHaveValidationErrorFor(x => x.PersonalInfo.PhoneNumber)
                  .WithErrorMessage("Phone number is required.");
        }

        /// <summary>
        /// Tests that validation fails when phone number format is invalid.
        /// </summary>
        [Fact]
        public void Validate_InvalidPhoneNumberFormat_ShouldFail()
        {
            var driver = CreateValidDriver();
            driver.PersonalInfo.PhoneNumber = "123-456-789";

            var result = this.validator.TestValidate(driver);

            result.ShouldHaveValidationErrorFor(x => x.PersonalInfo.PhoneNumber)
                  .WithErrorMessage("A valid US phone number is required.");
        }

        /// <summary>
        /// Tests that validation fails when license number is empty.
        /// </summary>
        [Fact]
        public void Validate_EmptyLicenseNumber_ShouldFail()
        {
            var driver = CreateValidDriver();
            driver.License.LicenseNumber = string.Empty;

            var result = this.validator.TestValidate(driver);

            result.ShouldHaveValidationErrorFor(x => x.License.LicenseNumber)
                  .WithErrorMessage("Driver's license number is required.");
        }

        /// <summary>
        /// Tests that validation fails when license number contains invalid characters.
        /// </summary>
        [Fact]
        public void Validate_InvalidLicenseNumberCharacters_ShouldFail()
        {
            var driver = CreateValidDriver();
            driver.License.LicenseNumber = "DL@123";

            var result = this.validator.TestValidate(driver);

            result.ShouldHaveValidationErrorFor(x => x.License.LicenseNumber)
                  .WithErrorMessage("License number can only contain letters, numbers, and hyphens.");
        }

        /// <summary>
        /// Tests that validation fails when license expiry date is not set.
        /// </summary>
        [Fact]
        public void Validate_MissingLicenseExpiryDate_ShouldFail()
        {
            var driver = CreateValidDriver();
            driver.License.LicenseExpiryDate = default;

            var result = this.validator.TestValidate(driver);

            result.ShouldHaveValidationErrorFor(x => x.License.LicenseExpiryDate)
                  .WithErrorMessage("License expiry date is required.");
        }

        /// <summary>
        /// Tests that validation fails when license expires too soon.
        /// </summary>
        [Fact]
        public void Validate_LicenseExpiresTooSoon_ShouldFail()
        {
            var driver = CreateValidDriver();
            driver.License.LicenseExpiryDate = DateTime.UtcNow.AddDays(20);

            var result = this.validator.TestValidate(driver);

            result.ShouldHaveValidationErrorFor(x => x)
                  .WithErrorMessage("License must be valid for at least 30 days.");
        }

        /// <summary>
        /// Tests that validation fails when license is not valid for registration.
        /// </summary>
        [Fact]
        public void Validate_LicenseNotValidForRegistration_ShouldFail()
        {
            var driver = CreateValidDriver();
            driver.License.LicenseExpiryDate = DateTime.UtcNow.AddDays(100);

            var result = this.validator.TestValidate(driver);

            result.ShouldHaveValidationErrorFor(x => x)
                  .WithErrorMessage("License must be valid and meet minimum requirements for registration.");
        }

        private static DriverDto CreateValidDriver()
        {
            return new DriverDto
            {
                PersonalInfo = new PersonalInfo
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    PhoneNumber = "+1-555-123-4567"
                },
                License = new LicenseInfo
                {
                    LicenseNumber = "DL123456789",
                    LicenseExpiryDate = DateTime.UtcNow.AddYears(3)
                },
                IsActive = true
            };
        }
    }

    /// <summary>
    /// Unit tests for DriverUpdateValidator.
    /// </summary>
    public class DriverUpdateValidatorTests
    {
        private readonly DriverUpdateValidator validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DriverUpdateValidatorTests"/> class.
        /// </summary>
        public DriverUpdateValidatorTests()
        {
            this.validator = new DriverUpdateValidator();
        }

        /// <summary>
        /// Tests that validation passes for a valid driver update.
        /// </summary>
        [Fact]
        public void Validate_ValidDriverUpdate_ShouldPass()
        {
            var driver = new DriverDto
            {
                Id = 1,
                PersonalInfo = new PersonalInfo
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    PhoneNumber = "+1-555-123-4567"
                },
                License = new LicenseInfo
                {
                    LicenseNumber = "DL123456789",
                    LicenseExpiryDate = DateTime.UtcNow.AddMonths(8)
                }
            };

            var result = this.validator.TestValidate(driver);

            result.ShouldNotHaveAnyValidationErrors();
        }

        /// <summary>
        /// Tests that validation fails when driver ID is missing for update.
        /// </summary>
        [Fact]
        public void Validate_MissingIdForUpdate_ShouldFail()
        {
            var driver = new DriverDto { Id = 0 };

            var result = this.validator.TestValidate(driver);

            result.ShouldHaveValidationErrorFor(x => x.Id)
                  .WithErrorMessage("Driver ID is required for updates.");
        }
    }
}
