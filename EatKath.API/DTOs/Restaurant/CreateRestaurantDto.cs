namespace EatKath.API.DTOs.Restaurant
{
    public class CreateRestaurantDto
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

        public string CurrencyCode { get; set; } = "NPR";

        public bool IsActive { get; set; } = true;

        // Cuisines the restaurant serves (existing Cuisine ids).
        // At least one is required.
        public List<int> CuisineIds { get; set; } = new();
    }
}