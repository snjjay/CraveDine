namespace EatKath.API.DTOs.RestaurantImage
{
    public class UpdateRestaurantImageDto
    {
        public string ImageUrl { get; set; } = string.Empty;

        public bool IsLogo { get; set; }

        public int DisplayOrder { get; set; }
    }
}