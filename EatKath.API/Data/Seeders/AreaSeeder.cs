using EatKath.API.Entities;

namespace EatKath.API.Data.Seeders
{
    public static class AreaSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.Areas.Any())
                return;

            var areas = new List<Area>
            {
                new() { Name = "Kathmandu" },
                new() { Name = "Lalitpur" },
                new() { Name = "Bhaktapur" },
                new() { Name = "Thamel" },
                new() { Name = "New Baneshwor" },
                new() { Name = "Baneshwor" },
                new() { Name = "Kalanki" },
                new() { Name = "Koteshwor" },
                new() { Name = "Boudha" },
                new() { Name = "Lazimpat" },
                new() { Name = "Chabahil" },
                new() { Name = "Maharajgunj" },
                new() { Name = "Balaju" },
                new() { Name = "Kirtipur" },
                new() { Name = "Jawalakhel" },
                new() { Name = "Putalisadak" },
                new() { Name = "Teku" },
                new() { Name = "Tripureshwor" },
                new() { Name = "Gongabu" },
                new() { Name = "Sinamangal" }
            };

            context.Areas.AddRange(areas);

            await context.SaveChangesAsync();
        }
    }
}