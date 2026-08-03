using Microsoft.AspNetCore.Http;

namespace EatKath.API.DTOs.RestaurantImage
{
    public class UploadRestaurantImageDto
    {
        public int RestaurantId { get; set; }

        public IFormFile File { get; set; } = null!;
    }
}