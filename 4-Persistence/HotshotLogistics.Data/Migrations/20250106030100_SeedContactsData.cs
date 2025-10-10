#pragma warning disable SA1649
using FluentMigrator;

namespace HotshotLogistics.Data.Migrations;

/// <summary>
/// Seeds initial contacts data for existing customers.
/// </summary>
[Migration(20250106030100)]
public class SeedContactsData : Migration
{
    /// <summary>
    /// Applies the migration to seed contacts data.
    /// </summary>
    public override void Up()
    {
        // Seed primary contacts for the first 10 customers
        for (int i = 1; i <= 10; i++)
        {
            var customerId = $"cust-{i:D3}";

            Insert.IntoTable("Contacts")
                .Row(new
                {
                    CustomerId = customerId,
                    Name = $"Primary Contact {i}",
                    Email = $"contact{i:D3}@seedtest.com",
                    Phone = $"555-30{i:D2}",
                    Title = "Operations Manager",
                    IsPrimary = true,
                    CreatedAt = DateTime.UtcNow,
                });

            // Add a secondary contact for every 3rd customer
            if (i % 3 == 0)
            {
                Insert.IntoTable("Contacts")
                    .Row(new
                    {
                        CustomerId = customerId,
                        Name = $"Secondary Contact {i}",
                        Email = $"secondary{i:D3}@seedtest.com",
                        Phone = $"555-31{i:D2}",
                        Title = "Account Manager",
                        IsPrimary = false,
                        CreatedAt = DateTime.UtcNow,
                    });
            }
        }
    }

    /// <summary>
    /// Reverts the migration by removing seeded contacts data.
    /// </summary>
    public override void Down()
    {
        Delete.FromTable("Contacts").AllRows();
    }
}
