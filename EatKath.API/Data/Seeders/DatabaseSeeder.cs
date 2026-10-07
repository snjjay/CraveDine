using EatKath.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace EatKath.API.Data.Seeders
{
    public static class DatabaseSeeder
    {
        // Runs on every API startup (Program.cs). Migrations and reference
        // data run in every environment; the sample accounts (shared
        // password) and fictional restaurants only in Development.
        public static async Task SeedAsync(ApplicationDbContext context, bool includeDevelopmentData)
        {
            // Ensure database exists
            await context.Database.MigrateAsync();

            await SeedReferenceDataAsync(context);

            if (includeDevelopmentData)
                await SeedDevelopmentDataAsync(context);
        }

        // Lookup data the app needs in every environment: roles (registration
        // assigns "Customer"), areas, cuisines and dining types.
        public static async Task SeedReferenceDataAsync(ApplicationDbContext context)
        {
            await RoleSeeder.SeedAsync(context);
            await AreaSeeder.SeedAsync(context);
            await CuisineSeeder.SeedAsync(context);
            await DiningTypeSeeder.SeedAsync(context);
        }

        // Development only: sample users (all with the same known password)
        // and fictional restaurants, deals, menus, favourites and
        // redemptions. Needs the reference data above. Never run in
        // Production.
        public static async Task SeedDevelopmentDataAsync(ApplicationDbContext context)
        {
            await UserSeeder.SeedAsync(context);

            await RestaurantSeeder.SeedAsync(context);
            await RestaurantOpeningHourSeeder.SeedAsync(context);
            await RestaurantCuisineSeeder.SeedAsync(context);
            await RestaurantDiningTypeSeeder.SeedAsync(context);
            await DealSeeder.SeedAsync(context);
            await MenuCategorySeeder.SeedAsync(context);
            await MenuItemSeeder.SeedAsync(context);
            await RestaurantImageSeeder.SeedAsync(context);
            await UserFavoriteSeeder.SeedAsync(context);
            await RedemptionSeeder.SeedAsync(context);
        }

        // Sample data only in Development (launchSettings, docker-compose).
        // Azure runs as Production unless ASPNETCORE_ENVIRONMENT says otherwise.
        public static bool ShouldSeedDevelopmentData(IHostEnvironment environment)
        {
            return environment.IsDevelopment();
        }
    }
}