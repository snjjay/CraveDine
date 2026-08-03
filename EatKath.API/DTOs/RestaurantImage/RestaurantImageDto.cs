namespace EatKath.API.DTOs.RestaurantImage
{
    public class RestaurantImageDto
    {
        public int Id { get; set; }

        public int RestaurantId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public string? Caption { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsPrimary { get; set; }
    }
}