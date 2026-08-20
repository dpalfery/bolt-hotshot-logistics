// <copyright file="CustomerService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HotshotLogistics.Application.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using HotshotLogistics.Contracts.Repositories;
    using HotshotLogistics.Contracts.Services;
    using HotshotLogistics.Domain.Entities;
    using HotshotLogistics.Domain.ValueObjects;

    /// <summary>
    /// Service for managing customers.
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IJobRepository _jobRepository;
        private readonly IInvoiceRepository _invoiceRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomerService"/> class.
        /// </summary>
        /// <param name="customerRepository">The customer repository.</param>
        /// <param name="jobRepository">The job repository.</param>
        /// <param name="invoiceRepository">The invoice repository.</param>
        public CustomerService(
            ICustomerRepository customerRepository,
            IJobRepository jobRepository,
            IInvoiceRepository invoiceRepository)
        {
            _customerRepository = customerRepository;
            _jobRepository = jobRepository;
            _invoiceRepository = invoiceRepository;
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Customer>> GetCustomersAsync(CancellationToken cancellationToken = default)
        {
            return _customerRepository.GetAllAsync();
        }

        /// <inheritdoc/>
        public Task<Customer?> GetCustomerByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return _customerRepository.GetByIdAsync(id);
        }

        /// <inheritdoc/>
        public Task<Customer> CreateCustomerAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            return _customerRepository.AddAsync(customer);
        }

        /// <inheritdoc/>
        public async Task<Customer?> UpdateCustomerAsync(string id, Customer customer, CancellationToken cancellationToken = default)
        {
            var existingCustomer = await _customerRepository.GetByIdAsync(id);
            if (existingCustomer == null)
            {
                return null;
            }

            customer.Id = id;
            return await _customerRepository.UpdateAsync(customer);
        }

        /// <inheritdoc/>
        public Task<bool> DeleteCustomerAsync(string id, CancellationToken cancellationToken = default)
        {
            return _customerRepository.DeleteAsync(id);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Customer>> GetActiveCustomersAsync(CancellationToken cancellationToken = default)
        {
            return _customerRepository.GetActiveCustomersAsync();
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Customer>> GetOverdueCustomersAsync(CancellationToken cancellationToken = default)
        {
            return _customerRepository.GetOverdueCustomersAsync();
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Job>> GetCustomerJobsAsync(string customerId, CancellationToken cancellationToken = default)
        {
            return _jobRepository.GetJobsByCustomerAsync(customerId, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<IEnumerable<Invoice>> GetCustomerInvoicesAsync(string customerId, CancellationToken cancellationToken = default)
        {
            return _invoiceRepository.GetByCustomerIdAsync(customerId);
        }

        /// <inheritdoc/>
        public Task<bool> UpdateCreditLimitAsync(string customerId, decimal newLimit, CancellationToken cancellationToken = default)
        {
            return _customerRepository.UpdateCreditLimitAsync(customerId, newLimit);
        }

        /// <inheritdoc/>
        public Task<bool> UpdateCreditTermsAsync(string customerId, CreditTerms creditTerms, CancellationToken cancellationToken = default)
        {
            return _customerRepository.UpdateCreditTermsAsync(customerId, creditTerms);
        }

        /// <inheritdoc/>
        public Task<bool> ValidateCustomerAsync(Customer customer, CancellationToken cancellationToken = default)
        {
            // Basic validation - can be extended
            var isValid = !string.IsNullOrWhiteSpace(customer.CompanyName) &&
                         !string.IsNullOrWhiteSpace(customer.Id) &&
                         customer.CreditLimit >= 0;
            return Task.FromResult(isValid);
        }
    }
}
