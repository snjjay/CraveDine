namespace EatKath.API.DTOs.Restaurant
{
    public class UpdateRestaurantDto
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public int AreaId { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Website { get; set; } = string.Empty;

        // Only applied when supplied (non-blank). Logos are normally
        // managed through the dedicated logo upload/delete endpoints,
        // so an ordinary profile save never erases the stored logo.
        public string LogoUrl { get; set; } = string.Empty;

        public string CurrencyCode { get; set; } = "NPR";

        public bool IsActive { get; set; }

        // Optional. Omitted (null) = keep the current cuisines.
        // Supplied = replace them with this set (at least one).
        public List<int>? CuisineIds { get; set; }
    }
}