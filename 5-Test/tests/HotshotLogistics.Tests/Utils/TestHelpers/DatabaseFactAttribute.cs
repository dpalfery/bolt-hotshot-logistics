using System.Runtime.CompilerServices;

namespace HotshotLogistics.Tests.Utils.TestHelpers
{
    /// <summary>
    ///     A fact that runs only when a SQL Server connection is configured for tests.
    ///     Otherwise the test is skipped instead of failing.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class DatabaseFactAttribute : FactAttribute
    {
        public DatabaseFactAttribute(
            [CallerFilePath] string? sourceFilePath = null,
            [CallerLineNumber] int sourceLineNumber = 0)
            : base(sourceFilePath, sourceLineNumber)
        {
            if (!TestDatabaseHelper.IsConfigured)
            {
                Skip =
                    "Requires SQL Server. Set user secret ConnectionStrings:DefaultConnection, or CONNECTIONSTRINGS__DEFAULTCONNECTION.";
            }
        }
    }
}
