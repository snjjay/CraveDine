using Microsoft.Extensions.Hosting;

namespace EatKath.API.Data.Demo
{
    // Safety checks for the development-only demo dataset. The demo seeder
    // refuses to run unless BOTH are true:
    //   1. the app runs in the Development environment, and
    //   2. the database connection is a local SQL Server LocalDB instance.
    // An owner password must also be supplied through configuration
    // (DemoSeed:OwnerPassword), so no demo credential lives in source code.
    public static class DemoSeedGuard
    {
        public const string PasswordConfigKey = "DemoSeed:OwnerPassword";

        public static void EnsureAllowed(IHostEnvironment environment, string? connectionString)
        {
            if (!environment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    $"Demo seeding is only allowed in the Development environment (current: '{environment.EnvironmentName}').");
            }

            if (!IsLocalDb(connectionString))
            {
                throw new InvalidOperationException(
                    "Demo seeding is only allowed against a local SQL Server LocalDB database ((localdb)\\...).");
            }
        }

        public static string RequirePassword(string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    $"Set the demo owner password in configuration '{PasswordConfigKey}' " +
                    "(e.g. environment variable DemoSeed__OwnerPassword) before running seed-demo.");
            }

            return password;
        }

        // "Server=(localdb)\MSSQLLocalDB;..." / "Data Source=(LocalDB)\..."
        public static bool IsLocalDb(string? connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return false;

            var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                var pair = part.Split('=', 2);

                if (pair.Length != 2)
                    continue;

                var key = pair[0].Trim().ToLowerInvariant();

                if (key is "server" or "data source" or "datasource" or "addr" or "address")
                    return pair[1].Trim().StartsWith("(localdb)\\", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}
