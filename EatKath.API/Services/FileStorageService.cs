using Microsoft.AspNetCore.Hosting;

namespace EatKath.API.Services
{
    public class FileStorageService
    {
        private readonly IWebHostEnvironment _environment;

        public FileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveImageAsync(
            IFormFile file,
            string folder,
            string fileName)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            if (!allowedExtensions.Contains(extension))
                throw new Exception("Only JPG, PNG and WEBP images are allowed.");

            return await SaveFileAsync(file, folder, fileName);
        }

        public async Task<string> SavePdfAsync(
            IFormFile file,
            string folder,
            string fileName)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (extension != ".pdf")
                throw new Exception("Only PDF files are allowed.");

            return await SaveFileAsync(file, folder, fileName);
        }

        public async Task DeleteFileAsync(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return;

            var path = Path.Combine(
                _environment.WebRootPath,
                relativePath.TrimStart('/').Replace("/", "\\"));

            if (File.Exists(path))
                File.Delete(path);

            await Task.CompletedTask;
        }

        private async Task<string> SaveFileAsync(
            IFormFile file,
            string folder,
            string fileName)
        {
            var uploadFolder = Path.Combine(
                _environment.WebRootPath,
                folder);

            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var extension = Path.GetExtension(file.FileName);

            var fullPath = Path.Combine(uploadFolder, $"{fileName}{extension}");

            using var stream = new FileStream(fullPath, FileMode.Create);

            await file.CopyToAsync(stream);

            return "/" + folder.Replace("\\", "/") + "/" + fileName + extension;
        }
    }
}