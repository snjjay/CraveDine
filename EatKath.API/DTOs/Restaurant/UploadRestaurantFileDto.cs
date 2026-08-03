using Microsoft.AspNetCore.Http;

namespace EatKath.API.DTOs.Restaurant
{
    public class UploadRestaurantFileDto
    {
        public IFormFile File { get; set; } = null!;
    }
}