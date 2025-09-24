// <copyright file="DriverRepository.cs" company="PlaceholderCompany">
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
    /// Repository for managing Driver entities using native ADO.NET.
    /// </summary>
    internal class DriverRepository : BaseRepository<Driver>, IDriverRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DriverRepository"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        public DriverRepository(IConfiguration configuration)
            : base(configuration)
        {
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IDriver>> GetDriversAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM Drivers WHERE IsActive = 1 ORDER BY LastName, FirstName";
            var drivers = await ExecuteQueryAsync(sql);
            return drivers.Cast<IDriver>();
        }

        /// <inheritdoc/>
        public async Task<IDriver?> GetDriverByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM Drivers WHERE Id = @Id";
            var parameters = new[] { new SqlParameter("@Id", SqlDbType.Int) { Value = id } };
            var drivers = await ExecuteQueryAsync(sql, parameters);
            return drivers.FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<IDriver> CreateDriverAsync(IDriver driver, CancellationToken cancellationToken = default)
        {
            var domainDriver = (Driver)driver;
            domainDriver.CreatedAt = DateTime.UtcNow;
            return await AddAsync(domainDriver);
        }

        /// <inheritdoc/>
        public async Task<IDriver> UpdateDriverAsync(IDriver driver, CancellationToken cancellationToken = default)
        {
            var domainDriver = (Driver)driver;
            domainDriver.UpdatedAt = DateTime.UtcNow;
            return await UpdateAsync(domainDriver);
        }

        /// <inheritdoc/>
        public async Task DeleteDriverAsync(int id, CancellationToken cancellationToken = default)
        {
            // Soft delete - mark as inactive
            const string sql = "UPDATE Drivers SET IsActive = 0, UpdatedAt = @UpdatedAt WHERE Id = @Id";
            var parameters = new[]
            {
                new SqlParameter("@Id", SqlDbType.Int) { Value = id },
                new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = DateTime.UtcNow },
            };
            await ExecuteNonQueryAsync(sql, parameters);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IDriver>> GetActiveDriversAsync(CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM Drivers WHERE IsActive = 1 ORDER BY LastName, FirstName";
            var drivers = await ExecuteQueryAsync(sql);
            return drivers.Cast<IDriver>();
        }

        /// <inheritdoc/>
        public async Task<IDriver?> GetDriverByLicenseNumberAsync(string licenseNumber, CancellationToken cancellationToken = default)
        {
            const string sql = "SELECT * FROM Drivers WHERE LicenseNumber = @LicenseNumber";
            var parameters = new[] { new SqlParameter("@LicenseNumber", SqlDbType.NVarChar) { Value = licenseNumber } };
            var drivers = await ExecuteQueryAsync(sql, parameters);
            return drivers.FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<IDriver>> GetDriversByStatusAsync(DriverStatus status, CancellationToken cancellationToken = default)
        {
            // Note: CurrentStatus is not stored in the database, returning all active drivers
            const string sql = "SELECT * FROM Drivers WHERE IsActive = 1 ORDER BY LastName, FirstName";
            var drivers = await ExecuteQueryAsync(sql);
            return drivers.Cast<IDriver>();
        }

        // Explicit interface implementations to bridge concrete/interface types
        async Task<IDriver?> IDriverRepository.GetByIdAsync(object id)
        {
            return await GetByIdAsync(id);
        }

        async Task<IEnumerable<IDriver>> IDriverRepository.GetAllAsync()
        {
            return (await GetAllAsync()).Cast<IDriver>();
        }

        async Task<IDriver> IDriverRepository.AddAsync(IDriver entity)
        {
            var driver = entity as Driver ?? throw new ArgumentException("Entity must be Driver", nameof(entity));
            return await AddAsync(driver);
        }

        async Task<IDriver> IDriverRepository.UpdateAsync(IDriver entity)
        {
            var driver = entity as Driver ?? throw new ArgumentException("Entity must be Driver", nameof(entity));
            return await UpdateAsync(driver);
        }

        async Task<bool> IDriverRepository.DeleteAsync(object id)
        {
            return await DeleteAsync(id);
        }

        async Task<bool> IDriverRepository.ExistsAsync(object id)
        {
            return await ExistsAsync(id);
        }

        /// <inheritdoc/>
        protected override string GetTableName() => "Drivers";

        /// <inheritdoc/>
        protected override string GetPrimaryKeyColumnName() => "Id";

        /// <inheritdoc/>
        protected override Driver MapReaderToEntity(SqlDataReader reader)
        {
            return new Driver
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                PersonalInfo = new PersonalInfo
                {
                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    PhoneNumber = reader.GetString(reader.GetOrdinal("PhoneNumber")),
                },
                License = new LicenseInfo
                {
                    LicenseNumber = reader.GetString(reader.GetOrdinal("LicenseNumber")),
                    LicenseExpiryDate = reader.GetDateTime(reader.GetOrdinal("LicenseExpiryDate")),
                },
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
            };
        }

        /// <inheritdoc/>
        protected override SqlParameter[] GetInsertParameters(Driver entity)
        {
            return new[]
            {
                new SqlParameter("@FirstName", SqlDbType.NVarChar) { Value = entity.PersonalInfo.FirstName },
                new SqlParameter("@LastName", SqlDbType.NVarChar) { Value = entity.PersonalInfo.LastName },
                new SqlParameter("@Email", SqlDbType.NVarChar) { Value = entity.PersonalInfo.Email },
                new SqlParameter("@PhoneNumber", SqlDbType.NVarChar) { Value = entity.PersonalInfo.PhoneNumber },
                new SqlParameter("@LicenseNumber", SqlDbType.NVarChar) { Value = entity.License.LicenseNumber },
                new SqlParameter("@LicenseExpiryDate", SqlDbType.DateTime2) { Value = entity.License.LicenseExpiryDate },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = entity.IsActive },
            };
        }

        /// <inheritdoc/>
        protected override SqlParameter[] GetUpdateParameters(Driver entity)
        {
            return new[]
            {
                new SqlParameter("@Id", SqlDbType.Int) { Value = entity.Id },
                new SqlParameter("@FirstName", SqlDbType.NVarChar) { Value = entity.PersonalInfo.FirstName },
                new SqlParameter("@LastName", SqlDbType.NVarChar) { Value = entity.PersonalInfo.LastName },
                new SqlParameter("@Email", SqlDbType.NVarChar) { Value = entity.PersonalInfo.Email },
                new SqlParameter("@PhoneNumber", SqlDbType.NVarChar) { Value = entity.PersonalInfo.PhoneNumber },
                new SqlParameter("@LicenseNumber", SqlDbType.NVarChar) { Value = entity.License.LicenseNumber },
                new SqlParameter("@LicenseExpiryDate", SqlDbType.DateTime2) { Value = entity.License.LicenseExpiryDate },
                new SqlParameter("@IsActive", SqlDbType.Bit) { Value = entity.IsActive },
            };
        }
    }
}
