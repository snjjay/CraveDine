using EatKath.API.Entities;
using EatKath.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Data.Seeders
{
    public static class DealSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.Deals.Any())
                return;

            var restaurants = await context.Restaurants.ToListAsync();
            var deals = new List<Deal>();

            foreach (var r in restaurants)
            {
                deals.Add(new Deal
                {
                    RestaurantId = r.Id,
                    Title = "Lunch Special",
                    Description = "Enjoy discounted lunch.",
                    DiscountPercentage = 20 + (r.Id % 6) * 5,
                    OfferType = OfferType.DineIn,
                    PromoImageUrl = r.LogoUrl,
                    TermsAndConditions = "Valid during offer hours only.",
                    StartDate = DateOnly.FromDateTime(DateTime.Today),
                    EndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(6)),
                    StartTime = new TimeOnly(12, 0),
                    EndTime = new TimeOnly(15, 0),
                    MaximumGuests = 6,
                    DailyRedemptionLimit = 50,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });

                deals.Add(new Deal
                {
                    RestaurantId = r.Id,
                    Title = "Evening Takeaway",
                    Description = "Discount on takeaway orders.",
                    DiscountPercentage = 15 + (r.Id % 5) * 5,
                    OfferType = OfferType.Takeaway,
                    PromoImageUrl = r.LogoUrl,
                    TermsAndConditions = "Takeaway only.",
                    StartDate = DateOnly.FromDateTime(DateTime.Today),
                    EndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(6)),
                    StartTime = new TimeOnly(17, 0),
                    EndTime = new TimeOnly(20, 0),
                    MaximumGuests = 4,
                    DailyRedemptionLimit = 40,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            context.Deals.AddRange(deals);
            await context.SaveChangesAsync();
        }
    }
}