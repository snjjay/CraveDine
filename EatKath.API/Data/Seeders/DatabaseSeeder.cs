using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EatKath.API.Entities;

namespace EatKath.API.Data.Seeders
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Ensure database exists
            await context.Database.MigrateAsync();

            // Seed data in order
            //await RoleSeeder.SeedAsync(context);
            await RoleSeeder.SeedAsync(context);
            await UserSeeder.SeedAsync(context);
            await AreaSeeder.SeedAsync(context);
            await CuisineSeeder.SeedAsync(context);
            await DiningTypeSeeder.SeedAsync(context);
            await RestaurantSeeder.SeedAsync(context);
            await DemoDataSeeder.SeedAsync(context);
            // await SeedCuisines(context);
            // await SeedDiningTypes(context);
            // await SeedRestaurant(context);
            // await SeedMenu(context);
            // await SeedDeals(context);
        }


    }
}