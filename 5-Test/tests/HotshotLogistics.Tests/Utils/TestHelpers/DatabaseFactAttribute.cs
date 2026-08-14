namespace Xunit;

/// <summary>
/// A fact that runs only when a SQL Server connection is configured for tests.
/// Otherwise the test is skipped instead of failing.
/// </summary>
public sealed class DatabaseFactAttribute : FactAttribute
{
    public DatabaseFactAttribute()
    {
        if (!HotshotLogistics.Tests.TestDatabaseHelper.IsConfigured)
        {
            Skip = "Requires SQL Server. Set user secret ConnectionStrings:DefaultConnection, or CONNECTIONSTRINGS__DEFAULTCONNECTION.";
        }
    }
}
