
using EatKath.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Data.Seeders
{
    public static class RestaurantCuisineSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.RestaurantCuisines.Any())
                return;

            var cuisines = await context.Cuisines.ToDictionaryAsync(c => c.Name, c => c.Id);
            var restaurants = await context.Restaurants.ToListAsync();
            var mappings = new List<RestaurantCuisine>();

            foreach (var r in restaurants)
            {
                var names = new List<string>();

                if (r.Name.Contains("Bakery") || r.Name.Contains("Bread") || r.Name.Contains("Bake") || r.Name.Contains("Cake"))
                    names.Add("Bakery");
                else if (r.Name.Contains("Cafe") || r.Name.Contains("Coffee") || r.Name.Contains("Java") || r.Name.Contains("Bean"))
                    names.Add("Cafe");
                else if (r.Name.Contains("Pizza") || r.Name.Contains("Italian"))
                    names.Add("Italian");
                else if (r.Name.Contains("Burger"))
                {
                    names.Add("American");
                    names.Add("Fast Food");
                }
                else if (r.Name.Contains("Sushi"))
                    names.Add("Japanese");
                else if (r.Name.Contains("Mexican"))
                    names.Add("Mexican");
                else if (r.Name.Contains("BBQ"))
                    names.Add("BBQ");
                else if (r.Name.Contains("Momo") || r.Name.Contains("Everest") || r.Name.Contains("Thakali") || r.Name.Contains("Nepal"))
                {
                    names.Add("Nepali");
                    names.Add("Tibetan");
                }
                else
                    names.Add("Continental");

                foreach (var n in names.Distinct())
                    if (cuisines.TryGetValue(n, out var id))
                        mappings.Add(new RestaurantCuisine { RestaurantId = r.Id, CuisineId = id });
            }

            context.RestaurantCuisines.AddRange(mappings);
            await context.SaveChangesAsync();
        }
    }
}
