using ImageAPI.Core.Domain.Enum;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ImageAPI.Core.Domain.Entities
{
    /// <summary>
    /// Represents the metadata for a single uploaded image.
    /// </summary>
    public class ImageMetadata
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string FileName { get; set; } = string.Empty ;
        public long FileSize { get; set; }
        public string ContentType { get; set; } = string.Empty;
        [Required]
        public DateTime UploadDate { get; set; }
     
        public string? Status { get; set; }

        /// <summary>
        /// The ID of the user who uploaded the image.
        /// </summary>
        public string? UserId { get; set; } = string.Empty;

        /// <summary>
        /// The unique object name in the Google Cloud Storage bucket.
        /// </summary>
        public string GcsObjectName { get; set; } = string.Empty;

        /// <summary>
        /// Public URL to the generated thumbnail.
        /// </summary>
        public string? ThumbnailUrl { get; set; }

    }
}
