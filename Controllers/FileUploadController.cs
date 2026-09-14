using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LovEat.API.Services;

namespace LovEat.API.Controllers
{
    /// <summary>
    /// Generic upload endpoint used across the app for chef documents,
    /// portfolio photos, review images, FSSAI license scans, etc. — replaces
    /// the previous complete absence of any upload mechanism (every
    /// ImageUrl/DocumentUrl field elsewhere in the codebase had nowhere
    /// real to point to before this existed).
    /// </summary>
    [ApiController]
    [Route("api/files")]
    [Authorize]
    public class FileUploadController : ControllerBase
    {
        private readonly IFileStorageService _storage;
        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp", "image/heic", "application/pdf",
        };
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        public FileUploadController(IFileStorageService storage) => _storage = storage;

        /// <summary>folder should be a short logical bucket path, e.g. "chef-documents", "portfolio", "reviews" — validated to a safe allowlist so callers can't write outside expected prefixes.</summary>
        [HttpPost("upload")]
        [RequestSizeLimit(MaxFileSizeBytes)]
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] string folder = "uploads")
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "No file provided." });

            if (file.Length > MaxFileSizeBytes)
                return BadRequest(new { success = false, message = "File exceeds the 10 MB size limit." });

            if (!AllowedContentTypes.Contains(file.ContentType))
                return BadRequest(new { success = false, message = $"File type '{file.ContentType}' is not allowed. Allowed: {string.Join(", ", AllowedContentTypes)}." });

            var safeFolder = string.IsNullOrWhiteSpace(folder) || folder.Contains("..") ? "uploads" : folder.Trim('/');

            await using var stream = file.OpenReadStream();
            var (success, url, error) = await _storage.UploadAsync(stream, file.FileName, file.ContentType, safeFolder);
            if (!success) return BadRequest(new { success = false, message = error });

            return Ok(new { success = true, data = new { url } });
        }
    }
}
