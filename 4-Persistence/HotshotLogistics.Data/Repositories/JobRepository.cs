// <copyright file="JobRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using HotshotLogistics.Contracts.Models;
using HotshotLogistics.Contracts.Repositories;
using HotshotLogistics.Core.Repositories;
using HotshotLogistics.Domain.Models;
using Microsoft.Extensions.Configuration;

namespace HotshotLogistics.Data.Repositories
{
    /// <summary>
    /// Repository for managing Job entities using native ADO.NET.
    /// </summary>
    internal class JobRepository : BaseRepository<JobDto>, IJobRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JobRepository"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        public JobRepository(IConfiguration configuration) : base(configuration)
        {
        }

        /// <inheritdoc/>
        protected override string GetTableName() => "Jobs";

        /// <inheritdoc/>
        protected override string GetPrimaryKeyColumnName() => "Id";

        /// <inheritdoc/>
        protected override JobDto MapReaderToEntity(SqlDataReader reader)
        {
            return new JobDto
            {
                Id = reader.GetString(reader.GetOrdinal("Id")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                PickupAddress = reader.GetString(reader.GetOrdinal("PickupAddress")),
                DropoffAddress = reader.GetString(reader.GetOrdinal("DeliveryAddress")),
                Status = (JobStatus)reader.GetInt32(reader.GetOrdinal("Status")),
                Priority = (JobPriority)reader.GetInt32(reader.GetOrdinal("Priority")),
                Amount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                EstimatedDeliveryTimeString = reader.GetDateTime(reader.GetOrdinal("EstimatedDeliveryTime")).ToString("O"),
                AssignedDriverId = reader.IsDBNull(reader.GetOrdinal("AssignedDriverId")) ? null : reader.GetInt32(reader.GetOrdinal("AssignedDriverId")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
            };
        }

        /// <inheritdoc/>
        protected override SqlParameter[] GetInsertParameters(JobDto entity)
        {
            return new[]
            {
                new SqlParameter("@Id", SqlDbType.NVarChar) { Value = entity.Id },
                new SqlParameter("@Title", SqlDbType.NVarChar) { Value = entity.Title },
                new SqlParameter("@PickupAddress", SqlDbType.NVarChar) { Value = entity.PickupAddress },
                new SqlParameter("@DeliveryAddress", SqlDbType.NVarChar) { Value = entity.DropoffAddress },
                new SqlParameter("@Status", SqlDbType.Int) { Value = (int)entity.Status },
                new SqlParameter("@Priority", SqlDbType.Int) { Value = (int)entity.Priority },
                new SqlParameter("@TotalAmount", SqlDbType.Decimal) { Value = entity.Amount },
                new SqlParameter("@EstimatedDeliveryTime", SqlDbType.DateTime2) { Value = DateTime.Parse(entity.EstimatedDeliveryTimeString) },
                new SqlParameter("@AssignedDriverId", SqlDbType.Int) { Value = (object?)entity.AssignedDriverId ?? DBNull.Value }
            };
        }

        /// <inheritdoc/>
        protected override SqlParameter[] GetUpdateParameters(JobDto entity)
        {
            return new[]
            {
                new SqlParameter("@Id", SqlDbType.NVarChar) { Value = entity.Id },
                new SqlParameter("@Title", SqlDbType.NVarChar) { Value = entity.Title },
                new SqlParameter("@PickupAddress", SqlDbType.NVarChar) { Value = entity.PickupAddress },
                new SqlParameter("@DeliveryAddress", SqlDbType.NVarChar) { Value = entity.DropoffAddress },
                new SqlParameter("@Status", SqlDbType.Int) { Value = (int)entity.Status },
                new SqlParameter("@Priority", SqlDbType.Int) { Value = (int)entity.Priority },
                new SqlParameter("@TotalAmount", SqlDbType.Decimal) { Value = entity.Amount },
                new SqlParameter("@EstimatedDeliveryTime", SqlDbType.DateTime2) { Value = DateTime.Parse(entity.EstimatedDeliveryTimeString) },
                new SqlParameter("@AssignedDriverId", SqlDbType.Int) { Value = (object?)entity.AssignedDriverId ?? DBNull.Value }
            };
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IJob>> GetJobsAsync(CancellationToken cancellationToken = default)
        {
            return await GetAllAsync();
        }

        /// <inheritdoc/>
        public async Task<IJob?> GetJobByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await GetByIdAsync(id);
        }

        /// <inheritdoc/>
        public async Task<IJob> CreateJobAsync(IJob job, CancellationToken cancellationToken = default)
        {
            var jobDto = (JobDto)job;
            jobDto.CreatedAt = DateTime.UtcNow;
            return await AddAsync(jobDto);
        }

        /// <inheritdoc/>
        public async Task<IJob?> UpdateJobAsync(string id, IJob jobDetails, CancellationToken cancellationToken = default)
        {
            var existingJob = await GetByIdAsync(id);
            if (existingJob == null)
            {
                return null;
            }

            var jobDto = (JobDto)existingJob;
            var detailsDto = (JobDto)jobDetails;

            jobDto.Title = detailsDto.Title;
            jobDto.PickupAddress = detailsDto.PickupAddress;
            jobDto.DropoffAddress = detailsDto.DropoffAddress;
            jobDto.Status = detailsDto.Status;
            jobDto.Priority = detailsDto.Priority;
            jobDto.Amount = detailsDto.Amount;
            jobDto.EstimatedDeliveryTimeString = detailsDto.EstimatedDeliveryTimeString;
            jobDto.AssignedDriverId = detailsDto.AssignedDriverId;
            jobDto.UpdatedAt = DateTime.UtcNow;

            return await UpdateAsync(jobDto);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteJobAsync(string id, CancellationToken cancellationToken = default)
        {
            return await DeleteAsync(id);
        }
    }
}
