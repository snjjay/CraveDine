using EatKath.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Data.Seeders
{
    public static class MenuCategorySeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.MenuCategories.Any())
                return;

            var restaurants = await context.Restaurants.ToListAsync();
            var categories = new List<MenuCategory>();

            foreach (var r in restaurants)
            {
                string[] names = r.Name.Contains("Bakery")
                    ? new[] { "Bread", "Pastries", "Cakes", "Beverages" }
                    : r.Name.Contains("Cafe") || r.Name.Contains("Coffee") || r.Name.Contains("Java")
                        ? new[] { "Coffee", "Tea", "Snacks", "Desserts" }
                        : new[] { "Appetizers", "Main Course", "Desserts", "Beverages" };

                int order = 1;
                foreach (var n in names)
                {
                    categories.Add(new MenuCategory
                    {
                        RestaurantId = r.Id,
                        Name = n,
                        DisplayOrder = order++,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            context.MenuCategories.AddRange(categories);
            await context.SaveChangesAsync();
        }
    }
}