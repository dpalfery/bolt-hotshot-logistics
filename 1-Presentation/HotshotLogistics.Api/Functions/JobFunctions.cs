// <copyright file="JobFunctions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Api.Functions
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
    using HotshotLogistics.Contracts.Models;
    using HotshotLogistics.Contracts.Services;
    using Microsoft.Azure.Functions.Worker;
    using Microsoft.Azure.Functions.Worker.Http;

    /// <summary>
    /// Azure Functions for managing jobs.
    /// </summary>
    public class JobFunctions
    {
        private readonly IJobService jobService;
        private readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="JobFunctions"/> class.
        /// </summary>
        /// <param name="jobService">The job service.</param>
        public JobFunctions(IJobService jobService)
        {
            this.jobService = jobService;
        }

        /// <summary>
        /// Gets all jobs.
        /// </summary>
        /// <param name="req">The HTTP request.</param>
        /// <returns>The HTTP response containing the jobs.</returns>
        [Function("GetJobs")]
        public async Task<HttpResponseData> GetJobs(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "jobs")] HttpRequestData req)
        {
            var jobs = await this.jobService.GetJobsAsync(CancellationToken.None);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(jobs);
            return response;
        }

        /// <summary>
        /// Gets a job by ID.
        /// </summary>
        /// <param name="req">The HTTP request.</param>
        /// <param name="id">The job ID.</param>
        /// <returns>The HTTP response containing the job.</returns>
        [Function("GetJobById")]
        public async Task<HttpResponseData> GetJobById(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "jobs/{id}")] HttpRequestData req,
            string id)
        {

            var job = await this.jobService.GetJobByIdAsync(id);
            if (job == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Job with ID {id} not found");
                return notFoundResponse;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(job);
            return response;
        }

        /// <summary>
        /// Creates a new job.
        /// </summary>
        /// <param name="req">The HTTP request.</param>
        /// <returns>The HTTP response containing the created job.</returns>
        [Function("CreateJob")]
        public async Task<HttpResponseData> CreateJob(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "jobs")] HttpRequestData req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var jobDto = JsonSerializer.Deserialize<JobDto>(requestBody, this.jsonSerializerOptions);

            if (jobDto == null) // Basic validation
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid job data provided.");
                return badRequestResponse;
            }

            // Add more specific validation as needed (e.g., required fields)


            var createdJob = await this.jobService.CreateJobAsync(jobDto);
            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(createdJob);
            return response;
        }

        /// <summary>
        /// Updates an existing job.
        /// </summary>
        /// <param name="req">The HTTP request.</param>
        /// <param name="id">The job ID.</param>
        /// <returns>The HTTP response containing the updated job.</returns>
        [Function("UpdateJob")]
        public async Task<HttpResponseData> UpdateJob(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = "jobs/{id}")] HttpRequestData req,
            string id)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var jobDto = JsonSerializer.Deserialize<JobDto>(requestBody, this.jsonSerializerOptions);
            if (jobDto == null)
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid job data provided.");
                return badRequestResponse;
            }


            var updatedJob = await this.jobService.UpdateJobAsync(id, jobDto);
            if (updatedJob == null)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Job with ID {id} not found for update.");
                return notFoundResponse;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(updatedJob);
            return response;
        }

        /// <summary>
        /// Deletes a job.
        /// </summary>
        /// <param name="req">The HTTP request.</param>
        /// <param name="id">The job ID.</param>
        /// <returns>The HTTP response indicating success or failure.</returns>
        [Function("DeleteJob")]
        public async Task<HttpResponseData> DeleteJob(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "jobs/{id}")] HttpRequestData req,
            string id)
        {

            var success = await this.jobService.DeleteJobAsync(id);
            if (!success)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync($"Job with ID {id} not found or could not be deleted.");
                return notFoundResponse;
            }

            return req.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}
