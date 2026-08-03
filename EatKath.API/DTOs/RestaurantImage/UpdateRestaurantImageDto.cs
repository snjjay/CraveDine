namespace EatKath.API.DTOs.RestaurantImage
{
    public class UpdateRestaurantImageDto
    {
        public string ImageUrl { get; set; } = string.Empty;

        public string? Caption { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsPrimary { get; set; }
    }
}