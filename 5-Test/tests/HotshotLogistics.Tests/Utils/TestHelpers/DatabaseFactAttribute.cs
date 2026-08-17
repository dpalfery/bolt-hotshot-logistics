using System.Runtime.CompilerServices;

namespace Xunit;

/// <summary>
/// A fact that runs only when a SQL Server connection is configured for tests.
/// Otherwise the test is skipped instead of failing.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class DatabaseFactAttribute : FactAttribute
{
    public DatabaseFactAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = 0)
        : base(sourceFilePath, sourceLineNumber)
    {
        if (!HotshotLogistics.Tests.TestDatabaseHelper.IsConfigured)
        {
            Skip = "Requires SQL Server. Set user secret ConnectionStrings:DefaultConnection, or CONNECTIONSTRINGS__DEFAULTCONNECTION.";
        }
    }
}
