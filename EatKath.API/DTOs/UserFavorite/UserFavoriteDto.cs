namespace EatKath.API.DTOs.UserFavorite
{
    public class UserFavoriteDto
    {
        public int UserId { get; set; }

        public int RestaurantId { get; set; }

        public string RestaurantName { get; set; } = string.Empty;

        public string LogoUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}