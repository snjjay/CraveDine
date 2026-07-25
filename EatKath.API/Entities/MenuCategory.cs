namespace EatKath.API.Entities
{
    public class MenuCategory
    {
        public int Id { get; set; }

        public int MenuId { get; set; }

        public string Name { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        // Navigation Properties
        public MenuItem MenuItem { get; set; } = null!;

        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}