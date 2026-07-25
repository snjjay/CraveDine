using EatKath.API.Entities;

namespace EatKath.API.Data.Seeders
{
    public static class DiningTypeSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.DiningTypes.Any())
                return;

            var diningTypes = new List<DiningType>
            {
                new() { Name = "Casual Dining" },
                new() { Name = "Fine Dining" },
                new() { Name = "Cafe" },
                new() { Name = "Buffet" },
                new() { Name = "Family Restaurant" },
                new() { Name = "Fast Food" },
                new() { Name = "Takeaway" },
                new() { Name = "Delivery Only" },
                new() { Name = "Food Court" },
                new() { Name = "Rooftop Dining" }
            };

            context.DiningTypes.AddRange(diningTypes);

            await context.SaveChangesAsync();
        }
    }
}