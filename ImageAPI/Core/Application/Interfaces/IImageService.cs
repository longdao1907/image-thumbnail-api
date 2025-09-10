using ImageAPI.Core.Application.DTOs;

namespace ImageAPI.Core.Application.Interfaces
{
    public interface IImageService
    {
        Task<ImageMetadataDto> AddImageAsync(ImageMetadataDto request,string userId);
        Task<IEnumerable<ImageMetadataDto>> GetImagesForUserAsync(string userId);
        Task<IEnumerable<ImageMetadataDto>> GetImagesAsync();

        Task UpdateImageAsync(UpdateThumbnailImageDto updateThumbnailImageDto);

    }
}
