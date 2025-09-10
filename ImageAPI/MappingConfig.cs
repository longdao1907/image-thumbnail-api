using AutoMapper;
using ImageAPI.Core.Application.DTOs;
using ImageAPI.Core.Domain.Entities;

namespace ImageAPI
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig = new MapperConfiguration(config =>
            {
                config.CreateMap<ImageMetadataDto, ImageMetadata>();

                config.CreateMap<ImageMetadata, ImageMetadataDto>();
            });
            return mappingConfig;
        }
    }
}
