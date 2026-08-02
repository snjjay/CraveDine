using EatKath.API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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
            await RestaurantOpeningHourSeeder.SeedAsync(context);
            await RestaurantCuisineSeeder.SeedAsync(context);
            await RestaurantDiningTypeSeeder.SeedAsync(context);
            await DealSeeder.SeedAsync(context);
            await MenuCategorySeeder.SeedAsync(context);
            await MenuItemSeeder.SeedAsync(context);
            await UserFavoriteSeeder.SeedAsync(context);
            await RedemptionSeeder.SeedAsync(context);

            //// Optional
            //await RestaurantImageSeeder.SeedAsync(context);

            // await SeedCuisines(context);
            // await SeedDiningTypes(context);
            // await SeedRestaurant(context);
            // await SeedMenu(context);
            // await SeedDeals(context);
        }


    }
}