using ImageAPI.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Runtime.CompilerServices;

namespace ImageAPI.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ImageMetadata> ImageMetadata { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ImageMetadata>().HasData(new ImageMetadata
            {
                Id = Guid.Parse("E5A8A1C8-9C6B-4F7D-8D5A-2E1C9B8E4F6A"),
                FileName = "Test_file_name_1",
                FileSize = 15,
                ContentType = string.Empty,
                UploadDate = new DateTime(2025, 9, 6, 16, 52, 45, 728, DateTimeKind.Utc),
                Status = "Completed",
                UserId = string.Empty,
                GcsObjectName = string.Empty,
                ThumbnailUrl = string.Empty

            });

            modelBuilder.Entity<ImageMetadata>().HasData(new ImageMetadata
            {
                Id = Guid.Parse("A0B1C2D3-E4F5-4A6B-8C9D-0E1F2A3B4C5D"),
                FileName = "Test_file_name_2",
                FileSize = 15,
                ContentType = string.Empty,
                UploadDate = new DateTime(2025, 9, 6, 16, 52, 45, 728, DateTimeKind.Utc),
                Status = "Completed",
                UserId = string.Empty,
                GcsObjectName = string.Empty,
                ThumbnailUrl = string.Empty

            });
        }
    }
   
}
