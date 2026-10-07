namespace EatKath.API.Data.Seeders
{
    public static class RestaurantImageSeeder
    {
        // Seeds no gallery images. It used to add gallery1-3.jpg paths
        // for every restaurant, but no such files exist (broken images).
        // Gallery photos come from owner uploads; the dev-only
        // `seed-demo` command adds licensed photos for its demo
        // restaurants.
        public static Task SeedAsync(ApplicationDbContext context)
        {
            return Task.CompletedTask;
        }
    }
}
