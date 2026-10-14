using Microsoft.AspNetCore.Http;

namespace EatKath.API.DTOs.SurpriseBag
{
    // multipart/form-data body of POST api/SurpriseBag/{id}/image.
    public class UploadSurpriseBagImageDto
    {
        public IFormFile File { get; set; } = null!;
    }
}
