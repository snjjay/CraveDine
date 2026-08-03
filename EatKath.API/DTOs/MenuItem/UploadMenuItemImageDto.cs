using Microsoft.AspNetCore.Http;

namespace EatKath.API.DTOs.MenuItem
{
    public class UploadMenuItemImageDto
    {
        public IFormFile File { get; set; } = null!;
    }
}