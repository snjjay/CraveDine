using EatKath.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Data.Seeders
{
    public static class RestaurantDiningTypeSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.RestaurantDiningTypes.Any())
                return;

            var diningTypes = await context.DiningTypes.ToDictionaryAsync(d => d.Name, d => d.Id);
            var restaurants = await context.Restaurants.ToListAsync();

            var mappings = new List<RestaurantDiningType>();

            foreach (var r in restaurants)
            {
                var types = new List<string>();

                if (r.Name.Contains("Bakery") || r.Name.Contains("Bread") || r.Name.Contains("Bake") || r.Name.Contains("Cake"))
                    types.Add("Takeaway");
                else if (r.Name.Contains("Cafe") || r.Name.Contains("Coffee") || r.Name.Contains("Java") || r.Name.Contains("Bean"))
                    types.Add("Cafe");
                else if (r.Name.Contains("Burger") || r.Name.Contains("Pizza") || r.Name.Contains("Express"))
                {
                    types.Add("Fast Food");
                    types.Add("Takeaway");
                }
                else
                {
                    types.Add("Casual Dining");
                    types.Add("Family Restaurant");
                }

                foreach (var t in types.Distinct())
                {
                    if (diningTypes.TryGetValue(t, out var id))
                    {
                        mappings.Add(new RestaurantDiningType
                        {
                            RestaurantId = r.Id,
                            DiningTypeId = id
                        });
                    }
                }
            }

            context.RestaurantDiningTypes.AddRange(mappings);
            await context.SaveChangesAsync();
        }
    }
}