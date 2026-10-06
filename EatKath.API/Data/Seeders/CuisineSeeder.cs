using EatKath.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Data.Seeders
{
    // Cuisine reference data (runs on every startup, all environments).
    // Adds any cuisine from the catalogue that isn't in the database yet,
    // matched by name (case-insensitive). Never renames or deletes existing
    // cuisines, so it is safe to run repeatedly and on existing databases.
    public static class CuisineSeeder
    {
        public static readonly string[] Catalogue =
        {
            // Original catalogue
            "Nepali", "Indian", "Chinese", "Japanese", "Korean", "Thai",
            "Italian", "Mexican", "American", "Continental", "Tibetan",
            "Vietnamese", "Mediterranean", "Fast Food", "Bakery", "Cafe",
            "Seafood", "BBQ", "Desserts", "Vegan",

            // Kathmandu/Nepal launch additions
            "Newari", "Thakali", "Himalayan", "Momo & Dumplings",
            "North Indian", "South Indian", "Mughlai", "Pakistani",
            "Indo-Chinese", "Middle Eastern", "Turkish", "French",
            "Pizza", "Burgers"
        };

        public static async Task SeedAsync(ApplicationDbContext context)
        {
            var existing = (await context.Cuisines
                    .Select(c => c.Name)
                    .ToListAsync())
                .Select(name => name.Trim().ToLowerInvariant())
                .ToHashSet();

            var missing = Catalogue
                .Where(name => !existing.Contains(name.ToLowerInvariant()))
                .Select(name => new Cuisine { Name = name })
                .ToList();

            if (missing.Count == 0)
                return;

            context.Cuisines.AddRange(missing);

            await context.SaveChangesAsync();
        }
    }
}
