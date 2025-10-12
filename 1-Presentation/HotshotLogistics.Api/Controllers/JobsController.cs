// <copyright file="JobsController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
using System;
using System.Threading;
using System.Threading.Tasks;
using HotshotLogistics.Domain.DTOs;
using HotshotLogistics.Application.Services;
using HotshotLogistics.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HotshotLogistics.Api.Controllers
{
    /// <summary>
    /// API controller for job operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JobsController : ControllerBase
    {
        private readonly IJobService jobService;
        private readonly ILogger<JobsController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobsController"/> class.
        /// </summary>
        /// <param name="jobService">The job service.</param>
        /// <param name="logger">The logger.</param>
        public JobsController(
            IJobService jobService,
            ILogger<JobsController> logger)
        {
            this.jobService = jobService ?? throw new ArgumentNullException(nameof(jobService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets a summary of job counts by status.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A summary of job counts by status.</returns>
        [HttpGet("status-summary")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(typeof(JobStatusSummaryDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<JobStatusSummaryDto>> GetJobStatusSummary(CancellationToken cancellationToken = default)
        {
            try
            {
                var summary = await jobService.GetJobStatusSummaryAsync(cancellationToken);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while retrieving job status summary");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request.");
            }
        }
    }
}