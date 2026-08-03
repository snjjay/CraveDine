namespace EatKath.API.DTOs.Restaurant
{
    public class RestaurantDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Website { get; set; } = string.Empty;

        public string LogoUrl { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public int AreaId { get; set; }

        public string AreaName { get; set; } = string.Empty;

        public decimal? BestDiscount { get; set; }

        public int ActiveDeals { get; set; }

        public bool IsFavorite { get; set; }
    }
}