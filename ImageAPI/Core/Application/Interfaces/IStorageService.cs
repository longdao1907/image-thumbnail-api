namespace ImageAPI.Core.Application.Interfaces
{
    public interface IStorageService
    { 
       /// <summary>
       /// Generates a time-limited, signed URL that grants a client permission to upload a file directly to cloud storage.
       /// </summary>
       /// <param name="objectName">The unique name of the object in the storage bucket.</param>
       /// <param name="contentType">The MIME type of the file to be uploaded.</param>
       /// <returns>A pre-signed URL for a PUT request.</returns>
        Task<string> GenerateUploadUrlAsync(string objectName, string contentType);

        // New: actually uploads a stream to GCS and returns the public URL (or media link)
        Task<string> UploadFileAsync(string objectName, Stream source, string contentType);
    }
}
