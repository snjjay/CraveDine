using EatKath.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace EatKath.API.Data.Seeders
{
    public static class RestaurantOpeningHourSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (context.RestaurantOpeningHours.Any())
                return;

            var restaurants = await context.Restaurants.ToListAsync();

            var openingHours = new List<RestaurantOpeningHour>();

            foreach (var restaurant in restaurants)
            {
                bool isBakery =
                    restaurant.Name.Contains("Bakery") ||
                    restaurant.Name.Contains("Bread") ||
                    restaurant.Name.Contains("Bake") ||
                    restaurant.Name.Contains("Cake");

                bool isCafe =
                    restaurant.Name.Contains("Cafe") ||
                    restaurant.Name.Contains("Coffee") ||
                    restaurant.Name.Contains("Java") ||
                    restaurant.Name.Contains("Bean");

                for (int day = 0; day < 7; day++)
                {
                    var openingHour = new RestaurantOpeningHour
                    {
                        RestaurantId = restaurant.Id,
                        DayOfWeek = (DayOfWeek)day,
                        IsClosed = false
                    };

                    if (isBakery)
                    {
                        openingHour.OpenTime = new TimeOnly(6, 0);
                        openingHour.CloseTime = new TimeOnly(19, 0);
                    }
                    else if (isCafe)
                    {
                        if (day == 0 || day == 6)
                        {
                            openingHour.OpenTime = new TimeOnly(8, 0);
                            openingHour.CloseTime = new TimeOnly(21, 0);
                        }
                        else
                        {
                            openingHour.OpenTime = new TimeOnly(7, 0);
                            openingHour.CloseTime = new TimeOnly(20, 0);
                        }
                    }
                    else
                    {
                        switch ((DayOfWeek)day)
                        {
                            case DayOfWeek.Friday:
                            case DayOfWeek.Saturday:
                                openingHour.OpenTime = new TimeOnly(10, 0);
                                openingHour.CloseTime = new TimeOnly(22, 0);
                                break;

                            case DayOfWeek.Sunday:
                                openingHour.OpenTime = new TimeOnly(11, 0);
                                openingHour.CloseTime = new TimeOnly(20, 0);
                                break;

                            default:
                                openingHour.OpenTime = new TimeOnly(10, 0);
                                openingHour.CloseTime = new TimeOnly(21, 0);
                                break;
                        }
                    }

                    openingHours.Add(openingHour);
                }
            }

            context.RestaurantOpeningHours.AddRange(openingHours);

            await context.SaveChangesAsync();
        }
    }
}