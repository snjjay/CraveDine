using EatKath.API.Entities;
using EatKath.API.Enums;
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
            // Offers (Deals)
            // ============================
            if (!context.Deals.Any())
            {
                context.Deals.AddRange(

                    new Deal
                    {
                        RestaurantId = restaurant.Id,
                        Title = "25% OFF Dine-In",
                        Description = "Enjoy 25% off your total dine-in bill.",
                        DiscountPercentage = 25,
                        OfferType = OfferType.DineIn,
                        PromoImageUrl = "",
                        TermsAndConditions = "Valid for dine-in only.",
                        StartDate = DateOnly.FromDateTime(DateTime.Today),
                        EndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(3)),
                        StartTime = new TimeOnly(18, 0),
                        EndTime = new TimeOnly(20, 0),
                        MaximumGuests = 6,
                        AdvanceRedeemMinutes = 30,
                        DailyRedemptionLimit = 20,
                        IsActive = true
                    },

                    new Deal
                    {
                        RestaurantId = restaurant.Id,
                        Title = "20% OFF Takeaway",
                        Description = "Enjoy 20% off your takeaway order.",
                        DiscountPercentage = 20,
                        OfferType = OfferType.Takeaway,
                        PromoImageUrl = "",
                        TermsAndConditions = "Valid for takeaway only.",
                        StartDate = DateOnly.FromDateTime(DateTime.Today),
                        EndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(3)),
                        StartTime = new TimeOnly(11, 0),
                        EndTime = new TimeOnly(15, 0),
                        MaximumGuests = 1,
                        AdvanceRedeemMinutes = 30,
                        DailyRedemptionLimit = 30,
                        IsActive = true
                    },

                    new Deal
                    {
                        RestaurantId = restaurant.Id,
                        Title = "15% OFF Family Dinner",
                        Description = "Perfect for family dining.",
                        DiscountPercentage = 15,
                        OfferType = OfferType.DineIn,
                        PromoImageUrl = "",
                        TermsAndConditions = "Maximum 6 guests.",
                        StartDate = DateOnly.FromDateTime(DateTime.Today),
                        EndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(3)),
                        StartTime = new TimeOnly(17, 0),
                        EndTime = new TimeOnly(21, 0),
                        MaximumGuests = 6,
                        AdvanceRedeemMinutes = 30,
                        DailyRedemptionLimit = 15,
                        IsActive = true
                    });

                await context.SaveChangesAsync();
            }
        }
    }
}