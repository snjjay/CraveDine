namespace EatKath.API.DTOs.MenuItem
{
    public class UpdateMenuItemDto
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public bool IsFeatured { get; set; }

        public bool IsAvailable { get; set; }
    }
}