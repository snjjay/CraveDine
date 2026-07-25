namespace EatKath.API.Entities
{
    public class MenuCategory : BaseEntity
    {
        public int RestaurantId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        // Navigation Properties
        public Restaurant Restaurant { get; set; } = null!;

        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}