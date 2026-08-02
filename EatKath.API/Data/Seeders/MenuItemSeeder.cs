using EatKath.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Data.Seeders
{
    public static class MenuItemSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.MenuItems.Any())
                return;

            var categories = await context.MenuCategories
                .Include(c => c.Restaurant)
                .ToListAsync();

            var items = new List<MenuItem>();

            foreach (var c in categories)
            {
                for (int i = 1; i <= 3; i++)
                {
                    items.Add(new MenuItem
                    {
                        RestaurantId = c.RestaurantId,
                        MenuCategoryId = c.Id,
                        Name = $"{c.Name} Item {i}",
                        Description = $"Sample {c.Name.ToLower()} item.",
                        Price = 5 + (i * 3),

                        // NEW
                        ImageUrl = $"https://picsum.photos/seed/{c.RestaurantId}-{c.Id}-{i}/600/600",

                        IsFeatured = i == 1,
                        IsAvailable = true
                    });
                }
            }

            context.MenuItems.AddRange(items);
            await context.SaveChangesAsync();
        }
    }
}