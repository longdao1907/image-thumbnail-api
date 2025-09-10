using ImageAPI.Core.Application.Interfaces;
using ImageAPI.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImageAPI.Infrastructure.Persistence.Repositories
{
    public class ImageMetadataRepository : IImageMetadataRepository
    {
        private readonly AppDbContext _context;

        public ImageMetadataRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ImageMetadata> AddAsync(ImageMetadata metadata)
        {
            metadata.UploadDate = metadata.UploadDate.ToUniversalTime();
            await _context.ImageMetadata.AddAsync(metadata);
            await _context.SaveChangesAsync();
            return metadata;
        }

        public async Task<ImageMetadata?> GetByIdAsync(Guid id)
        {
            return await _context.ImageMetadata.FindAsync(id);
        }

        public async Task<IEnumerable<ImageMetadata>> GetByUserIdAsync(string userId)
        {
            return await _context.ImageMetadata.
                Where(i => i.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<ImageMetadata>> GetImages()
        {
            return await _context.ImageMetadata.ToListAsync();
            
        }

        public async Task UpdateAsync(ImageMetadata metadata)
        {
            _context.ImageMetadata.Update(metadata);
            await _context.SaveChangesAsync();
        }

        
    }
}
