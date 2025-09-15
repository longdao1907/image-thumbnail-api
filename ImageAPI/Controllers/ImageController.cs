using AutoMapper;
using ImageAPI.Core.Application.DTOs;
using ImageAPI.Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ImageAPI.Controllers
{
    [ApiController]
    [Route("api/Image")]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;
        private ResponseDto _response;
        private IMapper _mapper;

        public ImageController(IImageService imageService, IMapper mapper)
        {
            _imageService = imageService;
            _response = new ResponseDto();
            _mapper = mapper;
        }

        [HttpPost]
        [Route("upload-request")]
        [Authorize]
        public async Task<ResponseDto> Post([FromForm] ImageMetadataDto request)
        {
            try
            {
                var _obj = await _imageService.AddImageAsync(request, request.UserId ?? string.Empty);
                _response.Result = _mapper.Map<ImageMetadataDto>(_obj);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;

        }

        [HttpGet]
        [Route("get-images-by-user/{userId}")]
        [Authorize]
        public async Task<ResponseDto> GetUserImages(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _response.IsSuccess = false;
                    _response.Message = "Unauthorized Access.";
                    return _response;
                }

                var objList = await _imageService.GetImagesForUserAsync(userId);
                _response.Result = _mapper.Map<IEnumerable<ImageMetadataDto>>(objList);

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;


        }

        [HttpGet]
        [Route("get-images")]
        [Authorize]
        public async Task<ResponseDto> GetImages()
        {
            try
            {
                var objList = await _imageService.GetImagesAsync();
                _response.Result = _mapper.Map<IEnumerable<ImageMetadataDto>>(objList);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpPut]
        [Route("update-image")]
        [Authorize(Policy = "ServiceOnly")]
        public async Task<ResponseDto> Put(UpdateThumbnailImageDto request)
        {
            try
            {
                await _imageService.UpdateImageAsync(request);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.Message;
            }
            return _response;
        }

        [HttpGet]
        [Route("download-thumbnail/{imageId}")]
        [Authorize]
        public async Task<IActionResult> DownloadThumbnail(Guid imageId)
        {
            var destinationStream = new MemoryStream();
            try
            {
                var downloadResult = await _imageService.DownloadThumbnailAsync(imageId, destinationStream);
                return File(downloadResult.ThumbnailStream, downloadResult.ContentType, $"{imageId}_thumbnail.jpg");
            }
            catch (Exception ex)
            {
                throw new Exception("Error downloading thumbnail: " + ex.Message);
            }
        }
    }
}
