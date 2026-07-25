namespace EatKath.API.Entities
{
    public class MenuItem
    {
        public int Id { get; set; }

        public int MenuCategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Navigation Property
        public MenuCategory MenuCategory { get; set; } = null!;
        public Restaurant Restaurant { get; set; } = null!;
        public int RestaurantId { get; set; }

    }
}