using EatKath.API.Entities;

namespace EatKath.API.Data.Seeders
{
    public static class RestaurantImageSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.RestaurantImages.Any())
                return;

            var restaurants = context.Restaurants.ToList();

            var images = new List<RestaurantImage>();

            foreach (var restaurant in restaurants)
            {
                images.Add(new RestaurantImage
                {
                    RestaurantId = restaurant.Id,
                    ImageUrl = $"/uploads/restaurants/{restaurant.Id}/gallery1.jpg",
                    Caption = "Dining Area",
                    DisplayOrder = 1,
                    IsPrimary = true
                });

                images.Add(new RestaurantImage
                {
                    RestaurantId = restaurant.Id,
                    ImageUrl = $"/uploads/restaurants/{restaurant.Id}/gallery2.jpg",
                    Caption = "Outdoor Seating",
                    DisplayOrder = 2,
                    IsPrimary = false
                });

                images.Add(new RestaurantImage
                {
                    RestaurantId = restaurant.Id,
                    ImageUrl = $"/uploads/restaurants/{restaurant.Id}/gallery3.jpg",
                    Caption = "Food",
                    DisplayOrder = 3,
                    IsPrimary = false
                });
            }

            context.RestaurantImages.AddRange(images);
            await context.SaveChangesAsync();
        }
    }
}