using System.ComponentModel.DataAnnotations;

namespace ImageAPI.Core.Application.DTOs
{
    public class ImageMetadataDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }
        public string? Status { get; set; } 
        public string? ThumbnailUrl { get; set; }

        public string? UserId { get; set; } = string.Empty;

        [Required]
        public string ContentType { get; set; } = string.Empty;
        public string GcsObjectName { get; set; } = string.Empty;

        public IFormFile? OriginalImageFile { get; set; }
    }
}
