using EatKath.API.DTOs.RestaurantOpeningHour;

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

        public string CurrencyCode { get; set; } = "NPR";

        public string? CoverImageUrl { get; set; }

        public string? MenuPdfUrl { get; set; }

        public bool IsActive { get; set; }

        // Synthetic demo listing (development data); shown with a subtle
        // "Demo" label in the customer UI.
        public bool IsDemo { get; set; }

        public int AreaId { get; set; }

        public string AreaName { get; set; } = string.Empty;

        public decimal? BestDiscount { get; set; }

        public int ActiveDeals { get; set; }

        public bool IsFavorite { get; set; }

        // NEW
        public List<string> Cuisines { get; set; } = new();

        // Ids of the same cuisines, so edit forms can preselect them.
        public List<int> CuisineIds { get; set; } = new();

        public List<string> DiningTypes { get; set; } = new();

        public List<RestaurantOpeningHourDto> OpeningHours { get; set; } = new();

        // Active, not-yet-ended deals with today's availability.
        // Filled in by the restaurant list (GetAllAsync) only.
        public List<RestaurantDealSummaryDto> DealSummaries { get; set; } = new();
    }
}