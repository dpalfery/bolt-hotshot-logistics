namespace HotshotLogistics.Data.Repositories;

using System.Data;
using System.Data.SqlClient;
using HotshotLogistics.Contracts.Models;
using HotshotLogistics.Contracts.Repositories;
using HotshotLogistics.Core.Repositories;
using HotshotLogistics.Domain.Models;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Repository for customer data access using native ADO.NET.
/// </summary>
public class CustomerRepository : BaseRepository<ICustomer>, ICustomerRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomerRepository"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    public CustomerRepository(IConfiguration configuration) : base(configuration)
    {
    }

    /// <inheritdoc/>
    protected override string GetTableName() => "Customers";

    /// <inheritdoc/>
    protected override string GetPrimaryKeyColumnName() => "Id";

    /// <inheritdoc/>
    protected override ICustomer MapReaderToEntity(SqlDataReader reader)
    {
        return new Customer
        {
            Id = reader.GetString(reader.GetOrdinal("Id")),
            CompanyName = reader.GetString(reader.GetOrdinal("CompanyName")),
            TaxId = reader.IsDBNull(reader.GetOrdinal("TaxId")) ? null : reader.GetString(reader.GetOrdinal("TaxId")),
            BillingAddress = new Address
            {
                Street = reader.GetString(reader.GetOrdinal("BillingAddress")),
                City = reader.GetString(reader.GetOrdinal("City")),
                State = reader.GetString(reader.GetOrdinal("State")),
                ZipCode = reader.GetString(reader.GetOrdinal("ZipCode")),
                Country = reader.GetString(reader.GetOrdinal("Country")),
                Latitude = reader.GetDouble(reader.GetOrdinal("Latitude")),
                Longitude = reader.GetDouble(reader.GetOrdinal("Longitude"))
            },
            CreditLimit = reader.GetDecimal(reader.GetOrdinal("CreditLimit")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
        };
    }

    /// <inheritdoc/>
    protected override SqlParameter[] GetInsertParameters(ICustomer entity)
    {
        return new[]
        {
            new SqlParameter("@Id", SqlDbType.NVarChar) { Value = entity.Id },
            new SqlParameter("@CompanyName", SqlDbType.NVarChar) { Value = entity.CompanyName },
            new SqlParameter("@TaxId", SqlDbType.NVarChar) { Value = (object?)entity.TaxId ?? DBNull.Value },
            new SqlParameter("@BillingAddress", SqlDbType.NVarChar) { Value = $"{entity.BillingAddress.Street}, {entity.BillingAddress.City}, {entity.BillingAddress.State} {entity.BillingAddress.ZipCode}" },
            new SqlParameter("@City", SqlDbType.NVarChar) { Value = entity.BillingAddress.City },
            new SqlParameter("@State", SqlDbType.NVarChar) { Value = entity.BillingAddress.State },
            new SqlParameter("@ZipCode", SqlDbType.NVarChar) { Value = entity.BillingAddress.ZipCode },
            new SqlParameter("@Country", SqlDbType.NVarChar) { Value = entity.BillingAddress.Country },
            new SqlParameter("@Latitude", SqlDbType.Decimal) { Value = entity.BillingAddress.Latitude },
            new SqlParameter("@Longitude", SqlDbType.Decimal) { Value = entity.BillingAddress.Longitude },
            new SqlParameter("@CreditLimit", SqlDbType.Decimal) { Value = entity.CreditLimit },
            new SqlParameter("@IsActive", SqlDbType.Bit) { Value = entity.IsActive }
        };
    }

    /// <inheritdoc/>
    protected override SqlParameter[] GetUpdateParameters(ICustomer entity)
    {
        return new[]
        {
            new SqlParameter("@Id", SqlDbType.NVarChar) { Value = entity.Id },
            new SqlParameter("@CompanyName", SqlDbType.NVarChar) { Value = entity.CompanyName },
            new SqlParameter("@TaxId", SqlDbType.NVarChar) { Value = (object?)entity.TaxId ?? DBNull.Value },
            new SqlParameter("@BillingAddress", SqlDbType.NVarChar) { Value = $"{entity.BillingAddress.Street}, {entity.BillingAddress.City}, {entity.BillingAddress.State} {entity.BillingAddress.ZipCode}" },
            new SqlParameter("@City", SqlDbType.NVarChar) { Value = entity.BillingAddress.City },
            new SqlParameter("@State", SqlDbType.NVarChar) { Value = entity.BillingAddress.State },
            new SqlParameter("@ZipCode", SqlDbType.NVarChar) { Value = entity.BillingAddress.ZipCode },
            new SqlParameter("@Country", SqlDbType.NVarChar) { Value = entity.BillingAddress.Country },
            new SqlParameter("@Latitude", SqlDbType.Decimal) { Value = entity.BillingAddress.Latitude },
            new SqlParameter("@Longitude", SqlDbType.Decimal) { Value = entity.BillingAddress.Longitude },
            new SqlParameter("@CreditLimit", SqlDbType.Decimal) { Value = entity.CreditLimit },
            new SqlParameter("@IsActive", SqlDbType.Bit) { Value = entity.IsActive }
        };
    }

    /// <inheritdoc/>
    public async Task<ICustomer?> GetByTaxIdAsync(string taxId)
    {
        const string sql = "SELECT * FROM Customers WHERE TaxId = @TaxId";

        var parameters = new[] { new SqlParameter("@TaxId", SqlDbType.NVarChar) { Value = taxId } };
        var customers = await ExecuteQueryAsync(sql, parameters);

        return customers.Cast<ICustomer>().FirstOrDefault();
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ICustomer>> GetByCreditLimitRangeAsync(decimal minLimit, decimal maxLimit)
    {
        const string sql = "SELECT * FROM Customers WHERE CreditLimit BETWEEN @MinLimit AND @MaxLimit AND IsActive = 1";

        var parameters = new[]
        {
            new SqlParameter("@MinLimit", SqlDbType.Decimal) { Value = minLimit },
            new SqlParameter("@MaxLimit", SqlDbType.Decimal) { Value = maxLimit }
        };

        return await ExecuteQueryAsync(sql, parameters);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ICustomer>> GetActiveCustomersAsync()
    {
        const string sql = "SELECT * FROM Customers WHERE IsActive = 1 ORDER BY CompanyName";

        return await ExecuteQueryAsync(sql);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ICustomer>> GetOverdueCustomersAsync()
    {
        const string sql = @"
            SELECT DISTINCT c.*
            FROM Customers c
            INNER JOIN Invoices i ON c.Id = i.CustomerId
            WHERE i.Status IN (4, 5) AND i.DueDate < @CurrentDate
            ORDER BY c.CompanyName";

        var parameters = new[] { new SqlParameter("@CurrentDate", SqlDbType.DateTime2) { Value = DateTime.UtcNow } };

        return await ExecuteQueryAsync(sql, parameters);
    }

    /// <inheritdoc/>
    public async Task<decimal> GetTotalCreditLimitAsync()
    {
        const string sql = "SELECT SUM(CreditLimit) FROM Customers WHERE IsActive = 1";

        var result = await ExecuteScalarAsync<decimal?>(sql);
        return result ?? 0;
    }

    /// <inheritdoc/>
    public async Task<int> GetCustomerCountAsync()
    {
        const string sql = "SELECT COUNT(1) FROM Customers WHERE IsActive = 1";

        return await ExecuteScalarAsync<int>(sql);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateCreditLimitAsync(string customerId, decimal newLimit)
    {
        const string sql = "UPDATE Customers SET CreditLimit = @CreditLimit, UpdatedAt = @UpdatedAt WHERE Id = @CustomerId";

        var parameters = new[]
        {
            new SqlParameter("@CustomerId", SqlDbType.NVarChar) { Value = customerId },
            new SqlParameter("@CreditLimit", SqlDbType.Decimal) { Value = newLimit },
            new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = DateTime.UtcNow }
        };

        var rowsAffected = await ExecuteNonQueryAsync(sql, parameters);
        return rowsAffected > 0;
    }

    /// <inheritdoc/>
    public async Task<bool> DeactivateCustomerAsync(string customerId)
    {
        const string sql = "UPDATE Customers SET IsActive = 0, UpdatedAt = @UpdatedAt WHERE Id = @CustomerId";

        var parameters = new[]
        {
            new SqlParameter("@CustomerId", SqlDbType.NVarChar) { Value = customerId },
            new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = DateTime.UtcNow }
        };

        var rowsAffected = await ExecuteNonQueryAsync(sql, parameters);
        return rowsAffected > 0;
    }

    /// <inheritdoc/>
    public async Task<bool> ReactivateCustomerAsync(string customerId)
    {
        const string sql = "UPDATE Customers SET IsActive = 1, UpdatedAt = @UpdatedAt WHERE Id = @CustomerId";

        var parameters = new[]
        {
            new SqlParameter("@CustomerId", SqlDbType.NVarChar) { Value = customerId },
            new SqlParameter("@UpdatedAt", SqlDbType.DateTime2) { Value = DateTime.UtcNow }
        };

        var rowsAffected = await ExecuteNonQueryAsync(sql, parameters);
        return rowsAffected > 0;
    }
}