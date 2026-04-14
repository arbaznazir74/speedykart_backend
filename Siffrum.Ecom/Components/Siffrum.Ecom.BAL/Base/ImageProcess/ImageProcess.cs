using AutoMapper;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.Config.Configuration;
using Siffrum.Ecom.DAL.Context;

namespace Siffrum.Ecom.BAL.Base.ImageProcess
{
    public class ImageProcess : SiffrumBalBase
    {
        private readonly IWebHostEnvironment _env;
        public ImageProcess(IMapper mapper, ApiDbContext context, IWebHostEnvironment env)
            : base(mapper, context)
        {
            _env = env;
        }

        public async Task<string?> SaveFromBase64(
            string base64String,
            string imageExtension = "jpg",
            string imagePath = @"content/loginusers/profile")
        {
            if (string.IsNullOrWhiteSpace(base64String))
                return null;

            string? filePath = null;

            if (imagePath.StartsWith("wwwroot", StringComparison.OrdinalIgnoreCase))
            {
                imagePath = imagePath.Substring("wwwroot".Length).TrimStart('/', '\\');
            }

            try
            {

                imageExtension = imageExtension?.Trim().Replace(".", "").ToLower();

                byte[] imageBytes = Convert.FromBase64String(base64String);

                int maxSize = (imageExtension == "mp4" || imageExtension == "svg")
                    ? 4 * 1024 * 1024
                    : 1 * 1024 * 1024;

                if (imageBytes.Length > maxSize)
                {
                    throw new Exception($"File size exceeds {maxSize / (1024 * 1024)} MB limit.");
                }

                string fileName = $"{Guid.NewGuid()}.{imageExtension}";
                var rootPath = Directory.Exists("/var/www/speedyEcom")
                    ? "/var/www/speedyEcom"
                    : _env.WebRootPath;

                var folderPath = Path.Combine(rootPath, imagePath);

                Directory.CreateDirectory(folderPath);

                filePath = Path.Combine(folderPath, fileName);

                await File.WriteAllBytesAsync(filePath, imageBytes);
                // Build path inside wwwroot
                /*var folderPath = Path.Combine(_env.WebRootPath, imagePath);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                filePath = Path.Combine(folderPath, fileName);

                await File.WriteAllBytesAsync(filePath, imageBytes);*/

                // Return relative path usable in URL
                return Path.Combine(imagePath, fileName).Replace("\\", "/");
                //return Path.Combine(filePath).Replace("\\", "/");
            }
            catch
            {
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                throw;
            }
        }

        public async Task<string?> ConvertToBase64(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    return null;
                if (filePath.StartsWith("wwwroot", StringComparison.OrdinalIgnoreCase))
                {
                    filePath = filePath.Substring("wwwroot".Length).TrimStart('/', '\\');
                }
                var rootPath = Directory.Exists("/var/www/speedyEcom")
                    ? "/var/www/speedyEcom"
                    : _env.WebRootPath;

                var fullPath = Path.Combine(rootPath, filePath);
                if (!File.Exists(fullPath))
                    return null;

                byte[] fileBytes = await File.ReadAllBytesAsync(fullPath);

                return Convert.ToBase64String(fileBytes);
            }
            catch
            {
                return null;
            }
        }

        /*public async Task<string?> SaveFromBase64(string base64String, string imageExtension = "jpg", string imagePath = @"wwwroot/content/loginusers/profile")
        {
            string? filePath = null;
            try
            {
                //convert bas64string to bytes
                byte[] imageBytes = Convert.FromBase64String(base64String);
                

                int maxSize = (imageExtension == "mp4")
                    ? 4 * 1024 * 1024
                    : 1 * 1024 * 1024;

                if (imageBytes.Length > maxSize)
                {
                    throw new Exception($"File size exceeds {maxSize / (1024 * 1024)} MB limit.");
                }

                string fileName = Guid.NewGuid().ToString() + "." + imageExtension;

                // Specify the folder path where resumes are stored
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), imagePath);

                // Create the folder if it doesn't exist
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Combine the folder path and file name to get the full file path
                filePath = Path.Combine(folderPath, fileName);

                // Write the bytes to the file asynchronously
                await File.WriteAllBytesAsync(filePath, imageBytes);

                // Return the relative file path
                return Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath);
            }
            catch
            {
                // If an error occurs, delete the file (if created) and return null
                if (File.Exists(filePath))
                    File.Delete(filePath);
                throw;
            }
        }


        /// <summary>
        /// Converts an image file to a base64 encoded string.
        /// </summary>
        /// <param name="filePath">The path to the image file.</param>
        /// <returns>
        /// If successful, returns the base64 encoded string; 
        /// otherwise, returns null.
        /// </returns>
        public async Task<string?> ConvertToBase64(string filePath)
        {
            try
            {
                // Read all bytes from the file asynchronously
                byte[] imageBytes = await File.ReadAllBytesAsync(filePath);

                // Convert the bytes to a base64 string
                return Convert.ToBase64String(imageBytes);
            }
            catch (Exception ex)
            {
                // Handle exceptions and return null
                //return ex.Message;
                return null;
            }
        }*/
    }
}
