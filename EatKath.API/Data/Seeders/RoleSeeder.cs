using EatKath.API.Entities;

namespace EatKath.API.Data.Seeders
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.Roles.Any())
                return;

            context.Roles.AddRange(
                new Role
                {
                    Name = "Admin"
                },
                new Role
                {
                    Name = "Owner"
                },
                new Role
                {
                    Name = "Customer"
                });

            await context.SaveChangesAsync();
        }
    }
}