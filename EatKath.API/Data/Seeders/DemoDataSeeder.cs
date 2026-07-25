using EatKath.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Data.Seeders
{
    public static class DemoDataSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            var restaurant = await context.Restaurants
                .FirstOrDefaultAsync(x => x.Name == "Fewa Momo House");

            if (restaurant == null)
                throw new Exception("Fewa Momo House not found.");

            // ============================
            // Restaurant Cuisines
            // ============================
            if (!context.RestaurantCuisines.Any())
            {
                var nepaliCuisine = await context.Cuisines.FirstAsync(x => x.Name == "Nepali");
                var tibetanCuisine = await context.Cuisines.FirstAsync(x => x.Name == "Tibetan");

                context.RestaurantCuisines.AddRange(
                    new RestaurantCuisine
                    {
                        RestaurantId = restaurant.Id,
                        CuisineId = nepaliCuisine.Id
                    },
                    new RestaurantCuisine
                    {
                        RestaurantId = restaurant.Id,
                        CuisineId = tibetanCuisine.Id
                    });

                await context.SaveChangesAsync();
            }

            // ============================
            // Restaurant Dining Type
            // ============================
            if (!context.RestaurantDiningTypes.Any())
            {
                var casualDining = await context.DiningTypes
                    .FirstAsync(x => x.Name == "Casual Dining");

                context.RestaurantDiningTypes.Add(
                    new RestaurantDiningType
                    {
                        RestaurantId = restaurant.Id,
                        DiningTypeId = casualDining.Id
                    });

                await context.SaveChangesAsync();
            }

            // ============================
            // Opening Hours
            // ============================
            if (!context.RestaurantOpeningHours.Any())
            {
                for (int i = 0; i < 7; i++)
                {
                    context.RestaurantOpeningHours.Add(
                        new RestaurantOpeningHour
                        {
                            RestaurantId = restaurant.Id,
                            DayOfWeek = (DayOfWeek)i,
                            OpenTime = new TimeOnly(10, 0),
                            CloseTime = new TimeOnly(21, 0),
                            IsClosed = false
                        });
                }

                await context.SaveChangesAsync();
            }

            // ============================
            // Menu Categories
            // ============================
            if (!context.MenuCategories.Any())
            {
                context.MenuCategories.AddRange(
                    new MenuCategory
                    {
                        RestaurantId = restaurant.Id,
                        Name = "Momos",
                        DisplayOrder = 1,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new MenuCategory
                    {
                        RestaurantId = restaurant.Id,
                        Name = "Drinks",
                        DisplayOrder = 2,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });

                await context.SaveChangesAsync();
            }

            // ============================
            // Menu Items
            // ============================
            if (!context.MenuItems.Any())
            {
                var momoCategory = await context.MenuCategories
                    .FirstAsync(x => x.Name == "Momos");

                var drinksCategory = await context.MenuCategories
                    .FirstAsync(x => x.Name == "Drinks");

                context.MenuItems.AddRange(
                    new MenuItem
                    {
                        RestaurantId = restaurant.Id,
                        MenuCategoryId = momoCategory.Id,
                        Name = "Chicken Momo",
                        Description = "Steamed chicken momo",
                        Price = 14.99m,
                        IsFeatured = true,
                        IsAvailable = true
                    },
                    new MenuItem
                    {
                        RestaurantId = restaurant.Id,
                        MenuCategoryId = momoCategory.Id,
                        Name = "Buff Momo",
                        Description = "Traditional buff momo",
                        Price = 15.99m,
                        IsFeatured = false,
                        IsAvailable = true
                    },
                    new MenuItem
                    {
                        RestaurantId = restaurant.Id,
                        MenuCategoryId = drinksCategory.Id,
                        Name = "Coke",
                        Description = "330ml Coca Cola",
                        Price = 3.50m,
                        IsFeatured = false,
                        IsAvailable = true
                    },
                    new MenuItem
                    {
                        RestaurantId = restaurant.Id,
                        MenuCategoryId = drinksCategory.Id,
                        Name = "Lassi",
                        Description = "Sweet yoghurt drink",
                        Price = 4.50m,
                        IsFeatured = true,
                        IsAvailable = true
                    });

                await context.SaveChangesAsync();
            }

            // ============================
            // Deals
            // ============================
            if (!context.Deals.Any())
            {
                context.Deals.AddRange(
                    new Deal
                    {
                        RestaurantId = restaurant.Id,
                        Title = "Chicken Momo Combo",
                        Description = "Chicken Momo + Coke",
                        OriginalPrice = 18.99m,
                        DiscountedPrice = 13.99m,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddMonths(3),
                        IsActive = true
                    },
                    new Deal
                    {
                        RestaurantId = restaurant.Id,
                        Title = "Buff Momo Combo",
                        Description = "Buff Momo + Coke",
                        OriginalPrice = 19.99m,
                        DiscountedPrice = 14.99m,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddMonths(3),
                        IsActive = true
                    },
                    new Deal
                    {
                        RestaurantId = restaurant.Id,
                        Title = "Lunch Special",
                        Description = "20% off all Momos",
                        OriginalPrice = 16.99m,
                        DiscountedPrice = 12.99m,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddMonths(3),
                        IsActive = true
                    },
                    new Deal
                    {
                        RestaurantId = restaurant.Id,
                        Title = "Family Pack",
                        Description = "4 Plates of Momos",
                        OriginalPrice = 59.99m,
                        DiscountedPrice = 44.99m,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddMonths(3),
                        IsActive = true
                    },
                    new Deal
                    {
                        RestaurantId = restaurant.Id,
                        Title = "Student Deal",
                        Description = "Student Discount",
                        OriginalPrice = 15.99m,
                        DiscountedPrice = 10.99m,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddMonths(3),
                        IsActive = true
                    });

                await context.SaveChangesAsync();
            }
        }
    }
}