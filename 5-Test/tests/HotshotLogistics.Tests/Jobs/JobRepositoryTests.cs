// <copyright file="JobRepositoryTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using FluentAssertions;
using HotshotLogistics.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace HotshotLogistics.Tests.Jobs
{
    /// <summary>
    ///     Integration tests for JobRepository.
    /// </summary>
    public class JobRepositoryTests : IClassFixture<DatabaseTestFixture>, IDisposable
    {
        private readonly List<string> _createdJobIds = new();
        private readonly JobRepository _jobRepository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="JobRepositoryTests" /> class.
        /// </summary>
        public JobRepositoryTests(DatabaseTestFixture fixture)
        {
            ArgumentNullException.ThrowIfNull(fixture);
            if (!TestDatabaseHelper.IsConfigured)
            {
                _jobRepository = null!;
                return;
            }

            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = TestDatabaseHelper.GetConnectionString()
                });

            IConfiguration configuration = configBuilder.Build();
            _jobRepository = new JobRepository(configuration);
        }

        /// <summary>
        ///     Cleans up test data.
        /// </summary>
        public void Dispose()
        {
            // Clean up created test jobs
            foreach (string jobId in _createdJobIds)
            {
                try
                {
                    _jobRepository.DeleteJobAsync(jobId).Wait();
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        /// <summary>
        ///     Tests that GetJobsAsync with filtering returns correct results.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [DatabaseFact]
        public async Task GetJobsAsync_WithFiltering_ReturnsCorrectResults()
        {
            // Arrange
            await CreateTestJobsAsync();
            JobFilterDto filter = new()
            {
                Status = JobStatus.Pending,
                Priority = JobPriority.High
            };
            PaginationParameters pagination = new() { PageNumber = 1, PageSize = 10 };

            // Act
            PagedResult<Job> result = await _jobRepository.GetJobsAsync(filter, pagination);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();
            result.Items.Should().OnlyContain(j => j.Status == JobStatus.Pending && j.Priority == JobPriority.High);
            result.TotalCount.Should().BeGreaterThan(0);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
        }

        /// <summary>
        ///     Tests that GetJobsAsync with pagination returns correct page.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [DatabaseFact]
        public async Task GetJobsAsync_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            await CreateTestJobsAsync();
            PaginationParameters pagination = new() { PageNumber = 1, PageSize = 2 };
            SortParameters sort = new() { SortBy = "CreatedAt", SortDirection = SortDirection.Descending };

            // Act
            PagedResult<Job> result = await _jobRepository.GetJobsAsync(null, pagination, sort);

            // Assert
            result.Should().NotBeNull();
            result.Items.Count().Should().BeLessThanOrEqualTo(2);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(2);
            result.TotalCount.Should().BeGreaterThan(0);
        }

        /// <summary>
        ///     Tests that GetJobsAsync with sorting returns correctly ordered results.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [DatabaseFact]
        public async Task GetJobsAsync_WithSorting_ReturnsOrderedResults()
        {
            // Arrange
            await CreateTestJobsAsync();
            SortParameters sort = new() { SortBy = "Amount", SortDirection = SortDirection.Ascending };
            PaginationParameters pagination = new() { PageNumber = 1, PageSize = 10 };

            // Act
            PagedResult<Job> result = await _jobRepository.GetJobsAsync(null, pagination, sort);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();

            List<decimal> amounts = result.Items.Select(j => j.Pricing.TotalAmount).ToList();
            amounts.Should().BeInAscendingOrder();
        }

        /// <summary>
        ///     Tests that GetJobsAsync with search term returns matching results.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [DatabaseFact]
        public async Task GetJobsAsync_WithSearchTerm_ReturnsMatchingResults()
        {
            // Arrange
            await CreateTestJobsAsync();
            JobFilterDto filter = new() { SearchTerm = "Test" };
            PaginationParameters pagination = new() { PageNumber = 1, PageSize = 10 };

            // Act
            PagedResult<Job> result = await _jobRepository.GetJobsAsync(filter, pagination);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();
            result.Items.Should().OnlyContain(j =>
                j.Title.Contains("Test", StringComparison.OrdinalIgnoreCase) ||
                j.PickupLocation.Address.Contains("Test", StringComparison.OrdinalIgnoreCase) ||
                j.DeliveryLocation.Address.Contains("Test", StringComparison.OrdinalIgnoreCase) ||
                j.SpecialInstructions.Contains("Test", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///     Tests that GetJobsAsync with date range filter returns correct results.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [DatabaseFact]
        public async Task GetJobsAsync_WithDateRangeFilter_ReturnsCorrectResults()
        {
            // Arrange
            await CreateTestJobsAsync();
            JobFilterDto filter = new()
            {
                CreatedAfter = DateTime.UtcNow.AddDays(-1),
                CreatedBefore = DateTime.UtcNow.AddDays(1)
            };
            PaginationParameters pagination = new() { PageNumber = 1, PageSize = 10 };

            // Act
            PagedResult<Job> result = await _jobRepository.GetJobsAsync(filter, pagination);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();
            result.Items.Should().OnlyContain(j =>
                j.CreatedAt >= filter.CreatedAfter &&
                j.CreatedAt <= filter.CreatedBefore);
        }

        /// <summary>
        ///     Tests that GetJobsByStatusAsync returns jobs with correct status.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [DatabaseFact]
        public async Task GetJobsByStatusAsync_ReturnsJobsWithCorrectStatus()
        {
            // Arrange
            await CreateTestJobsAsync();
            JobStatus status = JobStatus.Pending;

            // Act
            List<Job> result = (await _jobRepository.GetJobsByStatusAsync(status)).ToList();

            // Assert
            result.Should().NotBeNull();
            result.Should().OnlyContain(j => j.Status == status);
        }

        /// <summary>
        ///     Tests that GetJobsByDriverAsync returns jobs for correct driver.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [DatabaseFact]
        public async Task GetJobsByDriverAsync_ReturnsJobsForCorrectDriver()
        {
            // Arrange
            await CreateTestJobsAsync();
            int driverId = 1;

            // Act
            List<Job> result = (await _jobRepository.GetJobsByDriverAsync(driverId)).ToList();

            // Assert
            result.Should().NotBeNull();
            result.Should().OnlyContain(j => j.AssignedDriverId == driverId);
        }

        /// <summary>
        ///     Tests that GetJobsByCustomerAsync returns jobs for correct customer.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [DatabaseFact]
        public async Task GetJobsByCustomerAsync_ReturnsJobsForCorrectCustomer()
        {
            // Arrange
            await CreateTestJobsAsync();
            string customerId = "CUST001";

            // Act
            List<Job> result = (await _jobRepository.GetJobsByCustomerAsync(customerId)).ToList();

            // Assert
            result.Should().NotBeNull();
            result.Should().OnlyContain(j => j.CustomerId == customerId);
        }

        /// <summary>
        ///     Tests that GetOverdueJobsAsync returns only overdue jobs.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [DatabaseFact]
        public async Task GetOverdueJobsAsync_ReturnsOnlyOverdueJobs()
        {
            // Arrange
            await CreateOverdueTestJobAsync();

            // Act
            List<Job> result = (await _jobRepository.GetOverdueJobsAsync()).ToList();

            // Assert
            result.Should().NotBeNull();
            result.Should().OnlyContain(j =>
                j.EstimatedDeliveryTime < DateTime.UtcNow &&
                j.Status != JobStatus.Received &&
                j.Status != JobStatus.Pending);
        }

        /// <summary>
        ///     Tests that GetJobCountAsync returns correct count.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [DatabaseFact]
        public async Task GetJobCountAsync_ReturnsCorrectCount()
        {
            // Arrange
            await CreateTestJobsAsync();
            JobFilterDto filter = new() { Status = JobStatus.Pending };

            // Act
            int count = await _jobRepository.GetJobCountAsync(filter);

            // Assert
            count.Should().BeGreaterThan(0);
        }

        /// <summary>
        ///     Tests that filtering by multiple statuses works correctly.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [DatabaseFact]
        public async Task GetJobsAsync_WithStatusFilter_ReturnsCorrectResults()
        {
            // Arrange
            await CreateTestJobsAsync();
            JobFilterDto filter = new()
            {
                StatusList = [JobStatus.Pending, JobStatus.Assigned]
            };
            PaginationParameters pagination = new() { PageNumber = 1, PageSize = 10 };

            // Act
            PagedResult<Job> result = await _jobRepository.GetJobsAsync(filter, pagination);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();
            result.Items.Should().OnlyContain(j =>
                j.Status == JobStatus.Pending || j.Status == JobStatus.Assigned);
        }

        /// <summary>
        ///     Tests that filtering by amount range works correctly.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        [DatabaseFact]
        public async Task GetJobsAsync_WithAmountRange_ReturnsCorrectResults()
        {
            // Arrange
            await CreateTestJobsAsync();
            JobFilterDto filter = new()
            {
                MinAmount = 100m,
                MaxAmount = 500m
            };
            PaginationParameters pagination = new() { PageNumber = 1, PageSize = 10 };

            // Act
            PagedResult<Job> result = await _jobRepository.GetJobsAsync(filter, pagination);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().OnlyContain(j =>
                j.Pricing.TotalAmount >= 100m && j.Pricing.TotalAmount <= 500m);
        }

        /// <summary>
        ///     Creates test jobs for testing purposes.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task CreateTestJobsAsync()
        {
            List<Job> jobs =
            [
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    CustomerId = "CUST001",
                    Title = "Test Job 1",
                    PickupLocation = new Location { Address = "123 Test St" },
                    DeliveryLocation = new Location { Address = "456 Test Ave" },
                    Status = JobStatus.Pending,
                    Priority = JobPriority.High,
                    Amount = 250.00m,
                    EstimatedDeliveryTime = DateTime.UtcNow.AddHours(4),
                    ScheduledPickupTime = DateTime.UtcNow.AddHours(1),
                    AssignedDriverId = 1,
                    SpecialInstructions = "Handle with care",
                    CreatedAt = DateTime.UtcNow,
                    Pricing = new PricingDetails { TotalAmount = 250.00m }
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    CustomerId = "CUST002",
                    Title = "Test Job 2",
                    PickupLocation = new Location { Address = "789 Test Blvd" },
                    DeliveryLocation = new Location { Address = "321 Test Rd" },
                    Status = JobStatus.Assigned,
                    Priority = JobPriority.Medium,
                    Amount = 150.00m,
                    EstimatedDeliveryTime = DateTime.UtcNow.AddHours(6),
                    ScheduledPickupTime = DateTime.UtcNow.AddHours(2),
                    AssignedDriverId = 2,
                    SpecialInstructions = "Fragile items",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-30),
                    Pricing = new PricingDetails { TotalAmount = 150.00m }
                },
                new()
                {
                    Id = Guid.NewGuid().ToString(),
                    CustomerId = "CUST001",
                    Title = "Test Job 3",
                    PickupLocation = new Location { Address = "555 Test Way" },
                    DeliveryLocation = new Location { Address = "777 Test Ln" },
                    Status = JobStatus.Pending,
                    Priority = JobPriority.High,
                    Amount = 350.00m,
                    EstimatedDeliveryTime = DateTime.UtcNow.AddHours(8),
                    ScheduledPickupTime = DateTime.UtcNow.AddHours(3),
                    AssignedDriverId = null,
                    SpecialInstructions = "Rush delivery",
                    CreatedAt = DateTime.UtcNow.AddMinutes(-60),
                    Pricing = new PricingDetails { TotalAmount = 350.00m }
                }
            ];

            foreach (Job job in jobs)
            {
                Job createdJob = await _jobRepository.CreateJobAsync(job);
                _createdJobIds.Add(createdJob.Id);
            }
        }

        /// <summary>
        ///     Creates an overdue test job for testing purposes.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task CreateOverdueTestJobAsync()
        {
            Job job = new()
            {
                Id = Guid.NewGuid().ToString(),
                CustomerId = "CUST003",
                Title = "Overdue Test Job",
                PickupLocation = new Location { Address = "999 Overdue St" },
                DeliveryLocation = new Location { Address = "888 Late Ave" },
                Status = JobStatus.EnRoute,
                Priority = JobPriority.High,
                Amount = 400.00m,
                EstimatedDeliveryTime = DateTime.UtcNow.AddHours(-2),
                ScheduledPickupTime = DateTime.UtcNow.AddHours(-4),
                AssignedDriverId = 3,
                SpecialInstructions = "Overdue delivery",
                CreatedAt = DateTime.UtcNow.AddHours(-5),
                Pricing = new PricingDetails { TotalAmount = 400.00m }
            };

            Job createdJob = await _jobRepository.CreateJobAsync(job);
            _createdJobIds.Add(createdJob.Id);
        }
    }
}
