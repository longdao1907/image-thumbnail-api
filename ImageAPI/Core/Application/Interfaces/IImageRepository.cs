using ImageAPI.Core.Domain.Entities;

namespace ImageAPI.Core.Application.Interfaces
{
    public interface IImageMetadataRepository
    {
        Task<ImageMetadata> AddAsync(ImageMetadata metadata);
        Task<ImageMetadata?> GetByIdAsync(Guid id);
        Task<IEnumerable<ImageMetadata>> GetByUserIdAsync(string userId);
        Task<IEnumerable<ImageMetadata>> GetImages();
        Task UpdateAsync(ImageMetadata metadata);
    }
}
