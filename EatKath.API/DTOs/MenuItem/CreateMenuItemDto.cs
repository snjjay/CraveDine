namespace EatKath.API.DTOs.MenuItem
{
    public class CreateMenuItemDto
    {
        public int RestaurantId { get; set; }

        public int MenuCategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsAvailable { get; set; } = true;
    }
}