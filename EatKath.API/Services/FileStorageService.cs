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

        private static readonly string[] AllowedImageExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        public async Task<string> SaveImageAsync(
            IFormFile file,
            string folder,
            string fileName)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!AllowedImageExtensions.Contains(extension))
                throw new BusinessRuleException("Only JPG, PNG and WEBP images are allowed.");

            return await SaveFileAsync(file, folder, fileName);
        }

        // Saves an image under a new unique name ("image-<guid>.<ext>"), so
        // a replacement never reuses the previous URL (no stale browser
        // cache). Besides the extension, checks the size and that the
        // content really starts like a JPG, PNG or WEBP file.
        public async Task<string> SaveValidatedImageAsync(
            IFormFile file,
            string folder,
            long maxBytes)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (!AllowedImageExtensions.Contains(extension))
                throw new BusinessRuleException("Only JPG, PNG and WEBP images are allowed.");

            if (file.Length == 0)
                throw new BusinessRuleException("The selected image is empty.");

            if (file.Length > maxBytes)
                throw new BusinessRuleException($"Images must be {maxBytes / (1024 * 1024)} MB or smaller.");

            if (!await HasImageSignatureAsync(file))
                throw new BusinessRuleException("The selected file is not a valid JPG, PNG or WEBP image.");

            return await SaveFileAsync(file, folder, $"image-{Guid.NewGuid():N}");
        }

        // JPG: FF D8 FF. PNG: 89 50 4E 47 0D 0A 1A 0A.
        // WEBP: "RIFF" <4-byte size> "WEBP".
        private static async Task<bool> HasImageSignatureAsync(IFormFile file)
        {
            var header = new byte[12];
            var read = 0;

            await using (var stream = file.OpenReadStream())
            {
                while (read < header.Length)
                {
                    var count = await stream.ReadAsync(header.AsMemory(read));

                    if (count == 0)
                        break;

                    read += count;
                }
            }

            var isJpeg = read >= 3 &&
                header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;

            var isPng = read >= 8 &&
                header.AsSpan(0, 8).SequenceEqual(
                    new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

            var isWebp = read >= 12 &&
                header.AsSpan(0, 4).SequenceEqual("RIFF"u8) &&
                header.AsSpan(8, 4).SequenceEqual("WEBP"u8);

            return isJpeg || isPng || isWebp;
        }

        public async Task<string> SavePdfAsync(
            IFormFile file,
            string folder,
            string fileName)
        {
            var extension = Path.GetExtension(file.FileName).ToLower();

            if (extension != ".pdf")
                throw new BusinessRuleException("Only PDF files are allowed.");

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