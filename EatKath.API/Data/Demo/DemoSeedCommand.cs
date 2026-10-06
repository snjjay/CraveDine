using System.Text;
using EatKath.API.Data.Seeders;
using EatKath.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Data.Demo
{
    // `seed-demo` command (development only):
    //
    //   cd EatKath.API
    //   $env:ASPNETCORE_ENVIRONMENT = "Development"
    //   $env:DemoSeed__OwnerPassword = "<local demo password>"
    //   dotnet run --no-launch-profile -- seed-demo
    //
    // Refuses to run outside Development or against anything other than
    // LocalDB. Applies pending migrations and adds missing reference cuisines
    // (both also happen at normal startup), then creates the demo dataset.
    // Writes a git-ignored local report with the demo owner logins.
    public static class DemoSeedCommand
    {
        public const string ReportFileName = "demo-seed-report.local.md";

        public static async Task<int> RunAsync(WebApplication app)
        {
            try
            {
                var environment = app.Environment;
                var configuration = app.Configuration;

                DemoSeedGuard.EnsureAllowed(environment, configuration.GetConnectionString("DefaultConnection"));

                var password = DemoSeedGuard.RequirePassword(configuration[DemoSeedGuard.PasswordConfigKey]);

                var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
                var images = DemoImageLibrary.Load(Path.Combine(webRoot, "uploads", "demo"));

                using var scope = app.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                await context.Database.MigrateAsync();
                await CuisineSeeder.SeedAsync(context);

                var today = DateOnly.FromDateTime(TimeProvider.System.GetLocalNow().DateTime);
                var seeder = new DemoDataSeeder(context, new PasswordHasher<User>());
                var result = await seeder.SeedAsync(password, images, today);

                Console.WriteLine();
                Console.WriteLine("CraveDine demo seed complete");
                Console.WriteLine($"  Restaurants created: {result.RestaurantsCreated} (already present: {result.RestaurantsAlreadyPresent})");
                Console.WriteLine($"  Owners created:      {result.OwnersCreated}");
                Console.WriteLine($"  Cuisine links:       {result.CuisineLinks}");
                Console.WriteLine($"  Opening hours:       {result.OpeningHours}");
                Console.WriteLine($"  Menu categories:     {result.MenuCategories}");
                Console.WriteLine($"  Menu items:          {result.MenuItems}");
                Console.WriteLine($"  Deals:               {result.Deals}");
                Console.WriteLine($"  Images:              {result.Images}");

                var reportPath = Path.Combine(environment.ContentRootPath, ReportFileName);
                await File.WriteAllTextAsync(reportPath, BuildReport(result, password));
                Console.WriteLine($"  Local credentials report: {reportPath} (git-ignored)");

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Demo seed refused/failed: {ex.Message}");
                return 1;
            }
        }

        private static string BuildReport(DemoSeedResult result, string password)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# CraveDine demo seed report (LOCAL DEVELOPMENT ONLY)");
            sb.AppendLine();
            sb.AppendLine("Git-ignored. Never commit or share. Demo owners exist only in the local LocalDB database.");
            sb.AppendLine();
            sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine();
            sb.AppendLine($"Shared demo owner password: `{password}`");
            sb.AppendLine();
            sb.AppendLine("| # | Owner login | Demo restaurant | Area |");
            sb.AppendLine("|---|---|---|---|");

            foreach (var e in result.Entries)
                sb.AppendLine($"| {e.Index} | {e.OwnerEmail} | {e.RestaurantName} | {e.Area} |");

            return sb.ToString();
        }
    }
}
