
using EatKath.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Data.Seeders
{
    public static class RestaurantSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.Restaurants.Any())
                return;

            var owner1 = await context.Users.FirstAsync(x => x.Email == "owner@eatkath.com");
            var owner2 = await context.Users.FirstAsync(x => x.Email == "owner2@eatkath.com");

            var areas = await context.Areas.ToDictionaryAsync(a => a.Name, a => a.Id);

            var businesses = new (string Name, string Type, string Area)[]
            {
                ("Everest Momo House","Restaurant","Kathmandu"),
                ("Fire & Ice","Restaurant","Kathmandu"),
                ("Thakali Kitchen","Restaurant","Kathmandu"),
                ("Burger House","Restaurant","Lalitpur"),
                ("Pizza Hub","Restaurant","Lalitpur"),
                ("Himalayan Grill","Restaurant","Bhaktapur"),
                ("Spice Garden","Restaurant","Bhaktapur"),
                ("Momo Express","Restaurant","Kirtipur"),
                ("Fusion Bites","Restaurant","Thamel"),
                ("Royal Tandoor","Restaurant","Kathmandu"),
                ("Mountain View Restaurant","Restaurant","Lalitpur"),
                ("Urban Fork","Restaurant","Bhaktapur"),
                ("The Hungry Yak","Restaurant","Kathmandu"),
                ("Nepal Flavours","Restaurant","Kirtipur"),
                ("Asian Delight","Restaurant","Baneshwor"),
                ("Sakura Sushi","Restaurant","Kathmandu"),
                ("Italian Corner","Restaurant","Lalitpur"),
                ("Mexican Fiesta","Restaurant","Bhaktapur"),
                ("BBQ Nation","Restaurant","Kathmandu"),
                ("Riverside Dine","Restaurant","Koteshwor"),
                ("Taste of Nepal","Restaurant","Lalitpur"),
                ("Golden Spoon","Restaurant","Bhaktapur"),
                ("City Grill","Restaurant","Kathmandu"),
                ("Food Factory","Restaurant","Kirtipur"),
                ("The Dining Room","Restaurant","Jawalakhel"),
                ("Chef's Table","Restaurant","Kathmandu"),
                ("Family Kitchen","Restaurant","Lalitpur"),
                ("Fresh Feast","Restaurant","Bhaktapur"),
                ("Skyline Restaurant","Restaurant","Kathmandu"),
                ("Garden Terrace","Restaurant","Lazimpat"),

                ("European Bakery","Bakery","Kathmandu"),
                ("Sweet Crumbs","Bakery","Lalitpur"),
                ("Bread Basket","Bakery","Bhaktapur"),
                ("Bake House","Bakery","Kathmandu"),
                ("Daily Bread","Bakery","Lazimpat"),
                ("Golden Oven","Bakery","Kirtipur"),
                ("Cake World","Bakery","Kathmandu"),
                ("Butter & Flour","Bakery","Lalitpur"),
                ("Fresh Loaf","Bakery","Bhaktapur"),
                ("Sugar Bloom","Bakery","Kathmandu"),

                ("Coffee Talk","Cafe","Kathmandu"),
                ("Java House","Cafe","Lalitpur"),
                ("Cafe Aroma","Cafe","Bhaktapur"),
                ("Brew Corner","Cafe","Kathmandu"),
                ("Bean Station","Cafe","Lalitpur"),
                ("Mountain Cafe","Cafe","Kirtipur"),
                ("Morning Mug","Cafe","Kathmandu"),
                ("Urban Beans","Cafe","Lalitpur"),
                ("Roast & Sip","Cafe","Bhaktapur"),
                ("Central Cafe","Cafe","Kathmandu")
            };

            var restaurants = new List<Restaurant>();

            for (int i = 0; i < businesses.Length; i++)
            {
                var b = businesses[i];

                restaurants.Add(new Restaurant
                {
                    OwnerId = i < 25 ? owner1.Id : owner2.Id,
                    Name = b.Name,
                    Description = $"{b.Type} serving quality food and excellent customer service.",
                    Address = $"{100 + i} Main Street, {b.Area}",
                    AreaId = areas[b.Area],
                    PhoneNumber = $"+977980000{(1000 + i)}",
                    Email = $"info{i + 1}@eatkathdemo.com",
                    Website = $"https://www.eatkathdemo{i + 1}.com",

                    LogoUrl = $"https://picsum.photos/seed/logo{i + 1}/300/300",
                    CoverImageUrl = $"https://picsum.photos/seed/cover{i + 1}/1200/600",
                    MenuPdfUrl = null,

                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            context.Restaurants.AddRange(restaurants);
            await context.SaveChangesAsync();
        }
    }
}
