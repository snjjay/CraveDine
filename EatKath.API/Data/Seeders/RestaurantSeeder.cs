using EatKath.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Data.Seeders
{
    public static class RestaurantSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.Restaurants.Any())
                return;

            var owner = await context.Users
                .FirstAsync(x => x.Email == "owner@eatkath.com");

            var area = await context.Areas
                .FirstAsync(x => x.Name == "Kathmandu");

            var restaurant = new Restaurant
            {
                OwnerId = owner.Id,
                Name = "Everest Momo House",
                Description = "Authentic Nepali restaurant serving momo, chow mein, thukpa and traditional Nepali dishes.",
                Address = "123 Thamel Street, Kathmandu",
                AreaId = area.Id,
                PhoneNumber = "+9779800000000",
                Email = "owner@everestmomo.com",
                Website = "https://everestmomo.com",
                LogoUrl = "",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Restaurants.Add(restaurant);

            await context.SaveChangesAsync();
        }
    }
}