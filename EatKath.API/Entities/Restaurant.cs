namespace EatKath.API.Entities
{
    public class Restaurant : BaseEntity
    {
        
        public int OwnerId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public int AreaId { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Website { get; set; } = string.Empty;

        public string LogoUrl { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

       

        // Navigation Properties
        public User Owner { get; set; } = null!;

        public Area Area { get; set; } = null!;

        public ICollection<RestaurantImage> Images { get; set; } = new List<RestaurantImage>();

        public ICollection<RestaurantOpeningHour> OpeningHours { get; set; } = new List<RestaurantOpeningHour>();

        public ICollection<RestaurantCuisine> RestaurantCuisines { get; set; } = new List<RestaurantCuisine>();

        public ICollection<RestaurantDiningType> RestaurantDiningTypes { get; set; } = new List<RestaurantDiningType>();

        public ICollection<MenuCategory> MenuCategories { get; set; } = new List<MenuCategory>();

        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

        public ICollection<Deal> Deals { get; set; } = new List<Deal>();

        public ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();

   
        public string? CoverImageUrl { get; set; }

        public string? MenuPdfUrl { get; set; }
    }
}