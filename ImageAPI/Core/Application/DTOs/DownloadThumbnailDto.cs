using System;

namespace ImageAPI.Core.Application.DTOs;

public class DownloadThumbnailDto
{
  public Guid ImageId { get; set; }
  public Stream ThumbnailStream { get; set; } = new MemoryStream();
  public string ContentType { get; set; } = string.Empty;
  public long? FileSize { get; set; }

}
