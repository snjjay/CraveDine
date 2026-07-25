using EatKath.API.Entities;
using Microsoft.AspNetCore.Identity;

namespace EatKath.API.Data.Seeders
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.Users.Any())
                return;

            var hasher = new PasswordHasher<User>();

            // Admin
            var admin = new User
            {
                FirstName = "System",
                LastName = "Administrator",
                Email = "admin@eatkath.com",
                PhoneNumber = "0400000000",
                RoleId = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            admin.PasswordHash = hasher.HashPassword(admin, "Password77");

            // Owner
            var owner = new User
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "owner@eatkath.com",
                PhoneNumber = "0412345678",
                RoleId = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            owner.PasswordHash = hasher.HashPassword(owner, "Password77");

            context.Users.Add(admin);
            context.Users.Add(owner);

            await context.SaveChangesAsync();
        }
    }
}