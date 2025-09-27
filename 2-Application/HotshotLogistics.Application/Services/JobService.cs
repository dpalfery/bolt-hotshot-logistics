// <copyright file="JobService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Application.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using HotshotLogistics.Contracts.Models;
    using HotshotLogistics.Contracts.Repositories;
    using HotshotLogistics.Contracts.Services;
    using HotshotLogistics.Core.Exceptions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Service for managing jobs with lifecycle management.
    /// </summary>
    public class JobService : IJobService
    {
        private readonly IJobRepository jobRepository;
        private readonly ICustomerRepository customerRepository;
        private readonly IDriverRepository driverRepository;
        private readonly INotificationService notificationService;
        private readonly IMappingService mappingService;
        private readonly ILogger<JobService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobService"/> class.
        /// </summary>
        /// <param name="jobRepository">The job repository.</param>
        /// <param name="customerRepository">The customer repository.</param>
        /// <param name="driverRepository">The driver repository.</param>
        /// <param name="notificationService">The notification service.</param>
        /// <param name="mappingService">The mapping service.</param>
        /// <param name="logger">The logger.</param>
        public JobService(
            IJobRepository jobRepository,
            ICustomerRepository customerRepository,
            IDriverRepository driverRepository,
            INotificationService notificationService,
            IMappingService mappingService,
            ILogger<JobService> logger)
        {
            this.jobRepository = jobRepository ?? throw new ArgumentNullException(nameof(jobRepository));
            this.customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            this.driverRepository = driverRepository ?? throw new ArgumentNullException(nameof(driverRepository));
            this.notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            this.mappingService = mappingService ?? throw new ArgumentNullException(nameof(mappingService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<IJob> CreateJobAsync(IJob job, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Creating job with ID: {JobId}", job.Id);

            // Validate the job
            if (!await ValidateJobAsync(job, cancellationToken))
            {
                throw new ValidationException("Job validation failed");
            }

            // Verify customer exists and is active
            var customer = await customerRepository.GetByIdAsync(job.CustomerId);
            if (customer == null || !customer.IsActive)
            {
                throw new BusinessRuleException($"Customer {job.CustomerId} not found or inactive");
            }

            // Set initial job status and timestamps
            job.Status = JobStatus.Pending;
            job.CreatedAt = DateTime.UtcNow;
            job.UpdatedAt = DateTime.UtcNow;

            // Generate unique ID if not provided
            if (string.IsNullOrWhiteSpace(job.Id))
            {
                job.Id = Guid.NewGuid().ToString();
            }

            // Calculate estimated delivery time if not provided
            if (job.EstimatedDeliveryTime == default)
            {
                job.EstimatedDeliveryTime = await CalculateEstimatedDeliveryTimeAsync(job, cancellationToken);
            }

            // Initialize tracking info
            if (job.Tracking == null)
            {
                job.Tracking = new TrackingInfo
                {
                    CurrentStatus = "Job Created",
                    IsActive = false
                };
            }

            var createdJob = await jobRepository.CreateJobAsync(job, cancellationToken);

            // Send notification for job creation
            try
            {
                await notificationService.SendNotificationAsync(
                    job.CustomerId,
                    NotificationType.JobCreated,
                    "Job Created",
                    $"Your job '{job.Title}' has been created and is pending assignment.",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send job creation notification for job {JobId}", job.Id);
            }

            logger.LogInformation("Job created successfully with ID: {JobId}", createdJob.Id);
            return createdJob;
        }

        /// <inheritdoc/>
        public Task<IJob?> GetJobByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return jobRepository.GetJobByIdAsync(id, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<IJob>> GetJobsAsync(CancellationToken cancellationToken = default)
        {
            return jobRepository.GetJobsAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public Task<PagedResult<IJob>> GetJobsAsync(
            JobFilter? filter = null,
            PaginationParameters? pagination = null,
            SortParameters? sort = null,
            CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Getting jobs with filter: {@Filter}, pagination: {@Pagination}, sort: {@Sort}", 
                filter, pagination, sort);
            
            return jobRepository.GetJobsAsync(filter, pagination, sort, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IJob?> UpdateJobAsync(string id, IJob jobDetails, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Updating job with ID: {JobId}", id);

            var existingJob = await jobRepository.GetJobByIdAsync(id, cancellationToken);
            if (existingJob == null)
            {
                logger.LogWarning("Job not found with ID: {JobId}", id);
                return null;
            }

            // Validate the updated job details
            if (!await ValidateJobAsync(jobDetails, cancellationToken))
            {
                throw new ValidationException("Job validation failed");
            }

            jobDetails.UpdatedAt = DateTime.UtcNow;
            var updatedJob = await jobRepository.UpdateJobAsync(id, jobDetails, cancellationToken);

            logger.LogInformation("Job updated successfully with ID: {JobId}", id);
            return updatedJob;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteJobAsync(string id, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Deleting job with ID: {JobId}", id);

            var job = await jobRepository.GetJobByIdAsync(id, cancellationToken);
            if (job == null)
            {
                logger.LogWarning("Job not found with ID: {JobId}", id);
                return false;
            }

            // Check if job can be deleted (only allow deletion of jobs that haven't started)
            if (job.Status != JobStatus.Pending && job.Status != JobStatus.Cancelled)
            {
                logger.LogWarning("Cannot delete job with status: {Status}", job.Status);
                throw new BusinessRuleException($"Cannot delete job with status: {job.Status}");
            }

            var result = await jobRepository.DeleteJobAsync(id, cancellationToken);

            if (result)
            {
                logger.LogInformation("Job deleted successfully with ID: {JobId}", id);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<IJob> AssignDriverAsync(string jobId, int driverId, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Assigning driver {DriverId} to job {JobId}", driverId, jobId);

            var job = await jobRepository.GetJobByIdAsync(jobId, cancellationToken);
            if (job == null)
            {
                throw new BusinessRuleException($"Job {jobId} not found");
            }

            var driver = await driverRepository.GetByIdAsync(driverId);
            if (driver == null)
            {
                throw new BusinessRuleException($"Driver {driverId} not found");
            }

            // Check if driver is available
            if (!await IsDriverAvailableAsync(driverId, job.EstimatedDeliveryTime, cancellationToken: cancellationToken))
            {
                throw new BusinessRuleException($"Driver {driverId} is not available for the job timeframe");
            }

            // Update job with driver assignment
            job.AssignedDriverId = driverId;
            job.Status = JobStatus.Assigned;
            job.UpdatedAt = DateTime.UtcNow;

            var updatedJob = await jobRepository.UpdateJobAsync(jobId, job, cancellationToken);
            if (updatedJob == null)
            {
                throw new InvalidOperationException($"Failed to update job {jobId}");
            }

            // Send notification to driver
            try
            {
                await notificationService.SendNotificationAsync(
                    driverId.ToString(),
                    NotificationType.JobAssignment,
                    "New Job Assignment",
                    $"You have been assigned to job {job.Title}",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send job assignment notification to driver {DriverId}", driverId);
            }

            logger.LogInformation("Driver {DriverId} assigned to job {JobId} successfully", driverId, jobId);
            return updatedJob;
        }

        /// <inheritdoc/>
        public async Task<IJob> UpdateJobStatusAsync(string jobId, JobStatus status, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Updating job {JobId} status to {Status}", jobId, status);

            var job = await jobRepository.GetJobByIdAsync(jobId, cancellationToken);
            if (job == null)
            {
                throw new BusinessRuleException($"Job {jobId} not found");
            }

            var previousStatus = job.Status;
            job.Status = status;
            job.UpdatedAt = DateTime.UtcNow;

            // Update specific timestamps based on status
            switch (status)
            {
                case JobStatus.EnRoute:
                    // Driver is en route to pickup
                    job.Tracking.CurrentStatus = "En route to pickup";
                    break;
                case JobStatus.InProgress:
                    // Pickup completed, en route to delivery
                    job.Tracking.CurrentStatus = "In progress - en route to delivery";
                    break;
                case JobStatus.Completed:
                    job.Tracking.CurrentStatus = "Delivered";
                    job.Tracking.IsActive = false;
                    break;
                case JobStatus.Cancelled:
                    job.Tracking.CurrentStatus = "Cancelled";
                    job.Tracking.IsActive = false;
                    break;
            }

            var updatedJob = await jobRepository.UpdateJobAsync(jobId, job, cancellationToken);
            if (updatedJob == null)
            {
                throw new InvalidOperationException($"Failed to update job {jobId}");
            }

            // Send notifications for status changes
            try
            {
                await SendStatusChangeNotificationsAsync(updatedJob, previousStatus, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to send status change notifications for job {JobId}", jobId);
            }

            logger.LogInformation("Job {JobId} status updated to {Status} successfully", jobId, status);
            return updatedJob;
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateJobAsync(IJob job, CancellationToken cancellationToken = default)
        {
            if (job == null)
            {
                logger.LogWarning("Job validation failed: job is null");
                return false;
            }

            if (string.IsNullOrWhiteSpace(job.CustomerId))
            {
                logger.LogWarning("Job validation failed: customer ID is empty");
                return false;
            }

            if (string.IsNullOrWhiteSpace(job.Title))
            {
                logger.LogWarning("Job validation failed: job title is empty");
                return false;
            }

            // Validate pickup location
            if (job.PickupLocation == null || !job.PickupLocation.IsValid())
            {
                logger.LogWarning("Job validation failed: pickup location is invalid");
                return false;
            }

            // Validate pickup location with geocoding
            if (!await ValidateLocationWithGeocodingAsync(job.PickupLocation, "pickup", cancellationToken))
            {
                logger.LogWarning("Job validation failed: pickup location geocoding validation failed");
                return false;
            }

            // Validate delivery location
            if (job.DeliveryLocation == null || !job.DeliveryLocation.IsValid())
            {
                logger.LogWarning("Job validation failed: delivery location is invalid");
                return false;
            }

            // Validate delivery location with geocoding
            if (!await ValidateLocationWithGeocodingAsync(job.DeliveryLocation, "delivery", cancellationToken))
            {
                logger.LogWarning("Job validation failed: delivery location geocoding validation failed");
                return false;
            }

            // Validate cargo details
            if (job.Cargo == null || !job.Cargo.IsValid())
            {
                logger.LogWarning("Job validation failed: cargo details are invalid");
                return false;
            }

            // Validate pricing details
            if (job.Pricing == null || !job.Pricing.IsValid())
            {
                logger.LogWarning("Job validation failed: pricing details are invalid");
                return false;
            }

            // Validate scheduled pickup time
            if (job.ScheduledPickupTime <= DateTime.UtcNow)
            {
                logger.LogWarning("Job validation failed: scheduled pickup time is in the past");
                return false;
            }

            // Validate estimated delivery time
            if (job.EstimatedDeliveryTime <= job.ScheduledPickupTime)
            {
                logger.LogWarning("Job validation failed: estimated delivery time must be after pickup time");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates a location using geocoding to ensure it's a real address.
        /// </summary>
        /// <param name="location">The location to validate.</param>
        /// <param name="locationType">The type of location (pickup/delivery) for logging.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if the location is valid, false otherwise.</returns>
        private async Task<bool> ValidateLocationWithGeocodingAsync(Location location, string locationType, CancellationToken cancellationToken)
        {
            try
            {
                // If coordinates are already provided, validate them with reverse geocoding
                if (location.HasCoordinates)
                {
                    var reverseResult = await mappingService.ReverseGeocodeAsync(location.Latitude!.Value, location.Longitude!.Value, cancellationToken);
                    if (!reverseResult.IsValid)
                    {
                        logger.LogWarning("Location validation failed: reverse geocoding failed for {LocationType} coordinates", locationType);
                        return false;
                    }
                    return true;
                }

                // Otherwise, geocode the address to get coordinates
                var geocodeResult = await mappingService.GeocodeAddressAsync(location.FullAddress, cancellationToken);
                if (!geocodeResult.IsValid)
                {
                    logger.LogWarning("Location validation failed: geocoding failed for {LocationType} address: {Address}", locationType, location.FullAddress);
                    return false;
                }

                // Update location with geocoded coordinates
                location.Latitude = geocodeResult.Latitude;
                location.Longitude = geocodeResult.Longitude;

                logger.LogInformation("Location validated and geocoded for {LocationType}: {Address} -> ({Lat}, {Lon})",
                    locationType, location.FullAddress, location.Latitude, location.Longitude);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error validating location with geocoding for {LocationType}: {Address}", locationType, location.FullAddress);
                // If geocoding fails, still allow the job but log the error
                return true;
            }
        }

        /// <summary>
        /// Calculates the estimated delivery time based on job details.
        /// </summary>
        /// <param name="job">The job to calculate delivery time for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The estimated delivery time.</returns>
        private async Task<DateTime> CalculateEstimatedDeliveryTimeAsync(IJob job, CancellationToken cancellationToken = default)
        {
            try
            {
                // Use mapping service to get accurate distance and duration
                var distanceResult = await mappingService.CalculateDistanceAsync(job.PickupLocation, job.DeliveryLocation, cancellationToken);

                double distance;
                TimeSpan travelTime;

                if (distanceResult.IsValid)
                {
                    distance = distanceResult.Distance;
                    travelTime = distanceResult.Duration;
                }
                else
                {
                    // Fallback to basic calculation if mapping service fails
                    logger.LogWarning("Mapping service failed for distance calculation, using fallback calculation");
                    distance = job.PickupLocation.DistanceTo(job.DeliveryLocation) ?? 100; // Default 100 miles if no coordinates
                    var averageSpeed = 55; // Average highway speed in mph
                    travelTime = TimeSpan.FromHours(distance / averageSpeed);
                }

                var loadingTime = TimeSpan.FromHours(1); // 1 hour for loading/unloading
                var totalTime = travelTime.Add(loadingTime);

                // Add buffer based on priority
                var bufferTime = job.Priority switch
                {
                    JobPriority.High => TimeSpan.FromMinutes(30), // 30 minutes buffer for high priority
                    JobPriority.Medium => TimeSpan.FromHours(1.0), // 1 hour buffer for medium priority
                    JobPriority.Low => TimeSpan.FromHours(2.0), // 2 hours buffer for low priority
                    _ => TimeSpan.FromHours(1.0)
                };

                var estimatedDeliveryTime = job.ScheduledPickupTime.Add(totalTime).Add(bufferTime);

                logger.LogInformation("Calculated delivery time for job: distance={Distance} miles, travelTime={TravelTime}, totalTime={TotalTime}, buffer={Buffer}, ETA={ETA}",
                    distance, travelTime, totalTime, bufferTime, estimatedDeliveryTime);

                return estimatedDeliveryTime;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error calculating estimated delivery time, using fallback calculation");

                // Fallback calculation
                var distance = job.PickupLocation.DistanceTo(job.DeliveryLocation) ?? 100;
                var averageSpeed = 55;
                var travelTimeHours = distance / averageSpeed;
                var loadingTime = 1;
                var bufferHours = job.Priority switch
                {
                    JobPriority.High => 0.5,
                    JobPriority.Medium => 1.0,
                    JobPriority.Low => 2.0,
                    _ => 1.0
                };

                return job.ScheduledPickupTime.AddHours(travelTimeHours + loadingTime + bufferHours);
            }
        }

        /// <summary>
        /// Calculates the current estimated time of arrival for a job in progress.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The estimated time of arrival.</returns>
        public async Task<DateTime?> CalculateCurrentETAAsync(string jobId, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Calculating current ETA for job {JobId}", jobId);

            var job = await jobRepository.GetJobByIdAsync(jobId, cancellationToken);
            if (job == null)
            {
                logger.LogWarning("Job not found for ETA calculation: {JobId}", jobId);
                return null;
            }

            if (job.Status == JobStatus.Completed || job.Status == JobStatus.Cancelled)
            {
                return null; // No ETA needed for completed/cancelled jobs
            }

            var currentLocation = job.Tracking?.CurrentLocation;
            if (currentLocation == null)
            {
                // No tracking data, use original estimate
                return job.EstimatedDeliveryTime;
            }

            try
            {
                DateTime eta;

                if (job.Status == JobStatus.Assigned || job.Status == JobStatus.EnRoute)
                {
                    // Driver is heading to pickup or at pickup
                    var currentLocationAsLocation = new Location
                    {
                        Latitude = currentLocation.Latitude,
                        Longitude = currentLocation.Longitude
                    };
                    var distanceResult = await mappingService.CalculateDistanceAsync(currentLocationAsLocation, job.PickupLocation, cancellationToken);
                    if (distanceResult.IsValid)
                    {
                        eta = DateTime.UtcNow.Add(distanceResult.Duration);
                    }
                    else
                    {
                        // Fallback calculation
                        var remainingDistance = currentLocation.DistanceTo(job.PickupLocation) ?? 0;
                        var averageSpeed = job.Tracking?.GetAverageSpeed() ?? 45;
                        eta = DateTime.UtcNow.AddHours((double)remainingDistance / (double)averageSpeed);
                    }

                    // Add pickup time
                    eta = eta.AddMinutes(30);

                    // Add delivery time
                    var deliveryResult = await mappingService.CalculateDistanceAsync(job.PickupLocation, job.DeliveryLocation, cancellationToken);
                    if (deliveryResult.IsValid)
                    {
                        eta = eta.Add(deliveryResult.Duration);
                    }
                    else
                    {
                        // Fallback
                        var deliveryDistance = job.PickupLocation.DistanceTo(job.DeliveryLocation) ?? 0;
                        eta = eta.AddHours(deliveryDistance / 55);
                    }

                    // Add delivery time
                    eta = eta.AddMinutes(30);
                }
                else
                {
                    // Driver is heading to delivery
                    var currentLocationAsLocation = new Location
                    {
                        Latitude = currentLocation.Latitude,
                        Longitude = currentLocation.Longitude
                    };
                    var distanceResult = await mappingService.CalculateDistanceAsync(currentLocationAsLocation, job.DeliveryLocation, cancellationToken);
                    if (distanceResult.IsValid)
                    {
                        eta = DateTime.UtcNow.Add(distanceResult.Duration);
                    }
                    else
                    {
                        // Fallback calculation
                        var remainingDistance = currentLocation.DistanceTo(job.DeliveryLocation) ?? 0;
                        var averageSpeed = job.Tracking?.GetAverageSpeed() ?? 45;
                        eta = DateTime.UtcNow.AddHours((double)remainingDistance / (double)averageSpeed);
                    }

                    // Add delivery time
                    eta = eta.AddMinutes(30);
                }

                logger.LogInformation("Calculated ETA for job {JobId}: {ETA}", jobId, eta);
                return eta;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error calculating current ETA for job {JobId}, using fallback", jobId);

                // Fallback to original calculation
                var targetLocation = job.Status == JobStatus.Assigned || job.Status == JobStatus.EnRoute
                    ? job.PickupLocation
                    : job.DeliveryLocation;

                var remainingDistance = currentLocation.DistanceTo(targetLocation) ?? 0;
                var averageSpeed = job.Tracking?.GetAverageSpeed() ?? 45;
                var eta = DateTime.UtcNow.AddHours((double)remainingDistance / (double)averageSpeed);

                if (job.Status == JobStatus.Assigned || job.Status == JobStatus.EnRoute)
                {
                    eta = eta.AddMinutes(30);
                    var deliveryDistance = job.PickupLocation.DistanceTo(job.DeliveryLocation) ?? 0;
                    eta = eta.AddHours(deliveryDistance / 55).AddMinutes(30);
                }

                return eta;
            }
        }

        /// <summary>
        /// Optimizes the route for a job using mapping service.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True if route was optimized successfully.</returns>
        public async Task<bool> OptimizeRouteAsync(string jobId, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Optimizing route for job {JobId}", jobId);

            var job = await jobRepository.GetJobByIdAsync(jobId, cancellationToken);
            if (job == null)
            {
                logger.LogWarning("Job not found for route optimization: {JobId}", jobId);
                return false;
            }

            try
            {
                // For single pickup-delivery jobs, optimize the route between pickup and delivery
                var waypoints = new List<Location> { job.PickupLocation, job.DeliveryLocation };
                var optimizationResult = await mappingService.OptimizeRouteAsync(waypoints, cancellationToken);

                if (optimizationResult.IsValid)
                {
                    // Update estimated delivery time based on optimized route
                    var optimizedETA = DateTime.UtcNow.Add(optimizationResult.TotalDuration);
                    job.EstimatedDeliveryTime = optimizedETA;
                    job.UpdatedAt = DateTime.UtcNow;
                    await jobRepository.UpdateJobAsync(jobId, job, cancellationToken);

                    logger.LogInformation("Route optimized for job {JobId}, new ETA: {ETA}, distance: {Distance} miles",
                        jobId, optimizedETA, optimizationResult.TotalDistance);
                    return true;
                }
                else
                {
                    logger.LogWarning("Route optimization failed for job {JobId}: {Error}", jobId, optimizationResult.ErrorMessage);
                    // Fallback to current ETA calculation
                    var currentETA = await CalculateCurrentETAAsync(jobId, cancellationToken);
                    if (currentETA.HasValue)
                    {
                        job.EstimatedDeliveryTime = currentETA.Value;
                        job.UpdatedAt = DateTime.UtcNow;
                        await jobRepository.UpdateJobAsync(jobId, job, cancellationToken);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to optimize route for job {JobId}", jobId);
                // Fallback to current ETA calculation
                try
                {
                    var currentETA = await CalculateCurrentETAAsync(jobId, cancellationToken);
                    if (currentETA.HasValue)
                    {
                        job.EstimatedDeliveryTime = currentETA.Value;
                        job.UpdatedAt = DateTime.UtcNow;
                        await jobRepository.UpdateJobAsync(jobId, job, cancellationToken);
                        return true;
                    }
                }
                catch (Exception fallbackEx)
                {
                    logger.LogError(fallbackEx, "Fallback ETA calculation also failed for job {JobId}", jobId);
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> IsDriverAvailableAsync(int driverId, DateTime startTime, DateTime? endTime = null, CancellationToken cancellationToken = default)
        {
            var driver = await driverRepository.GetByIdAsync(driverId);
            if (driver == null || !driver.IsActive)
            {
                return false;
            }

            // Check if driver has any conflicting jobs
            var driverJobs = await jobRepository.GetByDriverIdAsync(driverId, cancellationToken);
            var activeJobs = driverJobs.Where(j =>
                j.Status == JobStatus.Assigned ||
                j.Status == JobStatus.InProgress ||
                j.Status == JobStatus.EnRoute);

            foreach (var activeJob in activeJobs)
            {
                var jobEndTime = activeJob.ActualDeliveryTime ?? activeJob.EstimatedDeliveryTime;
                var jobStartTime = activeJob.ActualPickupTime ?? activeJob.CreatedAt;

                // Check for time overlap
                if (startTime < jobEndTime && (endTime ?? startTime.AddHours(8)) > jobStartTime)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Sends notifications for job status changes.
        /// </summary>
        /// <param name="job">The job that changed status.</param>
        /// <param name="previousStatus">The previous status.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task SendStatusChangeNotificationsAsync(IJob job, JobStatus previousStatus, CancellationToken cancellationToken)
        {
            var statusMessage = job.Status switch
            {
                JobStatus.Assigned => "Your job has been assigned to a driver",
                JobStatus.EnRoute => "Driver is en route for pickup",
                JobStatus.InProgress => "Your job is now in progress - driver is en route to delivery",
                JobStatus.Completed => "Your job has been completed successfully",
                JobStatus.Cancelled => "Your job has been cancelled",
                _ => $"Your job status has been updated to {job.Status}"
            };

            // Notify customer
            await notificationService.SendNotificationAsync(
                job.CustomerId,
                NotificationType.JobStatusUpdate,
                "Job Status Update",
                statusMessage,
                cancellationToken);

            // Notify driver if assigned
            if (job.AssignedDriverId.HasValue)
            {
                var driverMessage = job.Status switch
                {
                    JobStatus.EnRoute => "Please proceed to pickup location",
                    JobStatus.InProgress => "Pickup completed - proceed to delivery location",
                    JobStatus.Completed => "Job has been marked as completed",
                    JobStatus.Cancelled => "Job has been cancelled",
                    _ => $"Job status updated to {job.Status}"
                };

                await notificationService.SendNotificationAsync(
                    job.AssignedDriverId.Value.ToString(),
                    NotificationType.JobStatusUpdate,
                    "Job Status Update",
                    driverMessage,
                    cancellationToken);
            }
        }
    }
}
