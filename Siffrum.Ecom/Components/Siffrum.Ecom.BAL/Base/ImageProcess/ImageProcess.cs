using Amazon.S3;
using Amazon.S3.Model;
using AutoMapper;
using Siffrum.Ecom.BAL.Foundation.Base;
using Siffrum.Ecom.Config.Configuration;
using Siffrum.Ecom.DAL.Context;

namespace Siffrum.Ecom.BAL.Base.ImageProcess
{
    public class ImageProcess : SiffrumBalBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly S3Settings _s3;
        private readonly IAmazonS3 _s3Client;
        private readonly bool _useS3;

        public ImageProcess(IMapper mapper, ApiDbContext context, IWebHostEnvironment env, APIConfiguration config)
            : base(mapper, context)
        {
            _env = env;
            _s3 = config.S3Settings ?? new S3Settings();
            _useS3 = !string.IsNullOrEmpty(_s3.AccessKey) && !string.IsNullOrEmpty(_s3.SecretKey);

            if (_useS3)
            {
                _s3Client = new AmazonS3Client(
                    _s3.AccessKey,
                    _s3.SecretKey,
                    Amazon.RegionEndpoint.GetBySystemName(_s3.Region));
            }
        }

        public async Task<string?> SaveFromBase64(
            string base64String,
            string imageExtension = "jpg",
            string imagePath = @"content/loginusers/profile")
        {
            if (string.IsNullOrWhiteSpace(base64String))
                return null;

            if (imagePath.StartsWith("wwwroot", StringComparison.OrdinalIgnoreCase))
            {
                imagePath = imagePath.Substring("wwwroot".Length).TrimStart('/', '\\');
            }

            imageExtension = imageExtension?.Trim().Replace(".", "").ToLower();
            byte[] fileBytes = Convert.FromBase64String(base64String);

            int maxSize = (imageExtension == "mp4" || imageExtension == "svg" || imageExtension == "mp3" || imageExtension == "m4a")
                ? 10 * 1024 * 1024
                : 2 * 1024 * 1024;

            if (fileBytes.Length > maxSize)
            {
                throw new Exception($"File size exceeds {maxSize / (1024 * 1024)} MB limit.");
            }

            string fileName = $"{Guid.NewGuid()}.{imageExtension}";

            // ── S3 upload ──
            if (_useS3)
            {
                var s3Prefix = ResolveS3Prefix(imagePath, imageExtension);
                var s3Key = $"{s3Prefix}{fileName}";

                var contentType = ResolveContentType(imageExtension);

                using var stream = new MemoryStream(fileBytes);
                var putReq = new PutObjectRequest
                {
                    BucketName = _s3.BucketName,
                    Key = s3Key,
                    InputStream = stream,
                    ContentType = contentType,
                };

                await _s3Client.PutObjectAsync(putReq);

                return $"https://{_s3.BucketName}.s3.{_s3.Region}.amazonaws.com/{s3Key}";
            }

            // ── Local fallback (dev) ──
            var rootPath = Directory.Exists("/var/www/speedyEcom")
                ? "/var/www/speedyEcom"
                : _env.WebRootPath;

            var folderPath = Path.Combine(rootPath, imagePath);
            Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);
            try
            {
                await File.WriteAllBytesAsync(filePath, fileBytes);
                return Path.Combine(imagePath, fileName).Replace("\\", "/");
            }
            catch
            {
                if (File.Exists(filePath)) File.Delete(filePath);
                throw;
            }
        }

        public async Task<string?> ConvertToBase64(string filePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                    return null;

                // If it's already an S3/HTTP URL, return it directly
                if (filePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                    || filePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    return filePath;
                }

                // Local file fallback (for old data or dev mode)
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

        public record ImageResult(string? NetworkUrl, string? Base64);

        public async Task<ImageResult> ResolveImage(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return new ImageResult(null, null);

            var result = await ConvertToBase64(filePath);
            if (result == null)
                return new ImageResult(null, null);

            if (result.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || result.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return new ImageResult(result, null);
            }

            return new ImageResult(null, result);
        }

        private static string ResolveS3Prefix(string imagePath, string ext)
        {
            // Audio files → instruction-audio/
            if (imagePath.Contains("audio", StringComparison.OrdinalIgnoreCase)
                || ext == "mp3" || ext == "m4a" || ext == "wav" || ext == "aac")
            {
                return "instruction-audio/";
            }

            // Everything else → uploads/<subfolder>/
            // Strip "content/" prefix to get clean subfolder: "content/products" → "products"
            var sub = imagePath
                .Replace("content/", "", StringComparison.OrdinalIgnoreCase)
                .Replace("content\\", "", StringComparison.OrdinalIgnoreCase)
                .Trim('/', '\\');

            if (string.IsNullOrEmpty(sub))
                return "uploads/";

            return $"uploads/{sub}/";
        }

        private static string ResolveContentType(string ext) => ext switch
        {
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            "gif" => "image/gif",
            "svg" => "image/svg+xml",
            "webp" => "image/webp",
            "mp4" => "video/mp4",
            "mp3" => "audio/mpeg",
            "m4a" => "audio/mp4",
            "wav" => "audio/wav",
            "aac" => "audio/aac",
            _ => "application/octet-stream"
        };
    }
}
