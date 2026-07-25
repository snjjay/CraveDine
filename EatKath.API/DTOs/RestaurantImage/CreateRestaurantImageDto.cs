namespace EatKath.API.DTOs.RestaurantImage
{
    public class CreateRestaurantImageDto
    {
        public int RestaurantId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public bool IsLogo { get; set; }

        public int DisplayOrder { get; set; }
    }
}