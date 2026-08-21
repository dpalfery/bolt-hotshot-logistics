using HotshotLogistics.Domain.Entities;

namespace HotshotLogistics.Tests.Utils.TestHelpers
{
    /// <summary>
    ///     Test builder to create concrete Customer instances for unit tests.
    ///     Centralizes defaults so tests only change fields they care about.
    /// </summary>
    public class CustomerBuilder
    {
        private readonly Customer _customer;

        private CustomerBuilder()
        {
            _customer = new Customer
            {
                Id = Guid.NewGuid().ToString(),
                CompanyName = "Test Company",
                IsActive = true,
                CreditLimit = 10000m,
                Contacts = [],
                BillingAddress = new Address(),
                CreatedAt = DateTime.UtcNow
            };
        }

        public static CustomerBuilder New()
        {
            return new CustomerBuilder();
        }

        public CustomerBuilder WithId(string id)
        {
            _customer.Id = id;
            return this;
        }

        public CustomerBuilder WithCompanyName(string name)
        {
            _customer.CompanyName = name;
            return this;
        }

        public CustomerBuilder WithIsActive(bool isActive)
        {
            _customer.IsActive = isActive;
            return this;
        }

        public CustomerBuilder WithCreditLimit(decimal limit)
        {
            _customer.CreditLimit = limit;
            return this;
        }

        public CustomerBuilder WithContact(Contact contact)
        {
            _customer.Contacts.Add(contact);
            return this;
        }

        public CustomerBuilder WithContacts(IEnumerable<Contact> contacts)
        {
            _customer.Contacts = new List<Contact>(contacts);
            return this;
        }

        public CustomerBuilder WithBillingAddress(Address address)
        {
            _customer.BillingAddress = address;
            return this;
        }

        public Customer Build()
        {
            return _customer;
        }
    }
}
