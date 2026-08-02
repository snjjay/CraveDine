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

            var users = new List<User>
            {
                // ==========================
                // Admin
                // ==========================
                new User
                {
                    FirstName = "System",
                    LastName = "Administrator",
                    Email = "admin@eatkath.com",
                    PhoneNumber = "0400000000",
                    RoleId = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                // ==========================
                // Owners
                // ==========================
                new User
                {
                    FirstName = "John",
                    LastName = "Smith",
                    Email = "owner@eatkath.com",
                    PhoneNumber = "0412345678",
                    RoleId = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                new User
                {
                    FirstName = "Jack",
                    LastName = "Springster",
                    Email = "owner2@eatkath.com",
                    PhoneNumber = "0453454543",
                    RoleId = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                new User
                {
                    FirstName = "Sarah",
                    LastName = "Brown",
                    Email = "owner3@eatkath.com",
                    PhoneNumber = "0455555555",
                    RoleId = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                new User
                {
                    FirstName = "Michael",
                    LastName = "Wilson",
                    Email = "owner4@eatkath.com",
                    PhoneNumber = "0466666666",
                    RoleId = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                // ==========================
                // Customers
                // ==========================
                new User
                {
                    FirstName = "Emma",
                    LastName = "Johnson",
                    Email = "emma.johnson@eatkath.com",
                    PhoneNumber = "0400000001",
                    RoleId = 3,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                new User
                {
                    FirstName = "David",
                    LastName = "Miller",
                    Email = "david.miller@eatkath.com",
                    PhoneNumber = "0400000002",
                    RoleId = 3,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                new User
                {
                    FirstName = "Olivia",
                    LastName = "Taylor",
                    Email = "olivia.taylor@eatkath.com",
                    PhoneNumber = "0400000003",
                    RoleId = 3,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                new User
                {
                    FirstName = "James",
                    LastName = "Anderson",
                    Email = "james.anderson@eatkath.com",
                    PhoneNumber = "0400000004",
                    RoleId = 3,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                new User
                {
                    FirstName = "Sophia",
                    LastName = "Thomas",
                    Email = "sophia.thomas@eatkath.com",
                    PhoneNumber = "0400000005",
                    RoleId = 3,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            foreach (var user in users)
            {
                user.PasswordHash = hasher.HashPassword(user, "Password77");
            }

            context.Users.AddRange(users);

            await context.SaveChangesAsync();
        }
    }
}