using EatKath.API.Entities;

namespace EatKath.API.Data.Seeders
{
    public static class CuisineSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.Cuisines.Any())
                return;

            var cuisines = new List<Cuisine>
            {
                new() { Name = "Nepali" },
                new() { Name = "Indian" },
                new() { Name = "Chinese" },
                new() { Name = "Japanese" },
                new() { Name = "Korean" },
                new() { Name = "Thai" },
                new() { Name = "Italian" },
                new() { Name = "Mexican" },
                new() { Name = "American" },
                new() { Name = "Continental" },
                new() { Name = "Tibetan" },
                new() { Name = "Vietnamese" },
                new() { Name = "Mediterranean" },
                new() { Name = "Fast Food" },
                new() { Name = "Bakery" },
                new() { Name = "Cafe" },
                new() { Name = "Seafood" },
                new() { Name = "BBQ" },
                new() { Name = "Desserts" },
                new() { Name = "Vegan" }
            };

            context.Cuisines.AddRange(cuisines);

            await context.SaveChangesAsync();
        }
    }
}