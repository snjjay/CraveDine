using EatKath.API.Data;
using EatKath.API.Entities;
using EatKath.API.Enums;

namespace EatKath.API.Data.Seeders;

public static class UserFavoriteSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (context.UserFavorites.Any())
            return;

        var users = context.Users.ToList();
        var restaurants = context.Restaurants.ToList();

        if (!users.Any() || !restaurants.Any())
            return;

        var favorites = new List<UserFavorite>();

        // User 1
        favorites.Add(new UserFavorite
        {
            UserId = users[0].Id,
            RestaurantId = restaurants[0].Id,
            CreatedAt = DateTime.UtcNow.AddDays(-15)
        });

        favorites.Add(new UserFavorite
        {
            UserId = users[0].Id,
            RestaurantId = restaurants[2].Id,
            CreatedAt = DateTime.UtcNow.AddDays(-12)
        });

        favorites.Add(new UserFavorite
        {
            UserId = users[0].Id,
            RestaurantId = restaurants[5].Id,
            CreatedAt = DateTime.UtcNow.AddDays(-8)
        });

        // User 2
        favorites.Add(new UserFavorite
        {
            UserId = users[1].Id,
            RestaurantId = restaurants[1].Id,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        });

        favorites.Add(new UserFavorite
        {
            UserId = users[1].Id,
            RestaurantId = restaurants[3].Id,
            CreatedAt = DateTime.UtcNow.AddDays(-6)
        });

        favorites.Add(new UserFavorite
        {
            UserId = users[1].Id,
            RestaurantId = restaurants[7].Id,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        });

        // User 3
        favorites.Add(new UserFavorite
        {
            UserId = users[2].Id,
            RestaurantId = restaurants[4].Id,
            CreatedAt = DateTime.UtcNow.AddDays(-7)
        });

        favorites.Add(new UserFavorite
        {
            UserId = users[2].Id,
            RestaurantId = restaurants[8].Id,
            CreatedAt = DateTime.UtcNow.AddDays(-3)
        });

        favorites.Add(new UserFavorite
        {
            UserId = users[2].Id,
            RestaurantId = restaurants[9].Id,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        });

        await context.UserFavorites.AddRangeAsync(favorites);
        await context.SaveChangesAsync();
    }
}