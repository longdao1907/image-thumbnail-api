using Google.Apis.Auth.OAuth2;
using Google.Cloud.SecretManager.V1;
using Google.Cloud.Storage.V1;
using ImageAPI.Core.Application.Interfaces;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System.Diagnostics;

namespace ImageAPI.Infrastructure.Services
{
    public class GcsStorageService : IStorageService
    {
        private readonly UrlSigner _urlSigner;
        private readonly string _bucketName;
         private readonly string _folderOriginalImage;
        private readonly StorageClient _storageClient;

        public GcsStorageService(IConfiguration configuration)
        {
            _bucketName = configuration["Gcp:BucketName"] ?? throw new ArgumentNullException("GCP BucketName not configured.");
            _folderOriginalImage = configuration["Gcp:OriginalImageFolder"] ?? throw new ArgumentNullException("GCP Folder Image not configured.");
            //Get Application Default Credentials

            // Configure EF Core with PostgreSQL
            string projectId = configuration.GetSection("Gcp").GetValue<string>("ProjectID") ?? throw new ArgumentNullException("Gcp Project ID not configured.");
            string sACredentialsKey = configuration.GetSection("Gcp").GetValue<string>("SACredentialsKey") ?? throw new ArgumentNullException("Gcp SACredentialsKey not configured.");
            string secretVersion = configuration.GetSection("Gcp").GetValue<string>("SecretVersion") ?? throw new ArgumentNullException("Gcp Secret Version not configured.");

            //Init Secret Manager Client
            SecretManagerServiceClient client =  SecretManagerServiceClient.Create();

            //get the secret value for database connection
            SecretVersionName secretVersionName = new(projectId, sACredentialsKey, secretVersion);
            AccessSecretVersionResponse result =  client.AccessSecretVersion(secretVersionName);
            string credJson = result.Payload.Data.ToStringUtf8();

            var credentials = GoogleCredential.FromJson(credJson);

            
            // This implicitly uses Application Default Credentials when running on Google Cloud.
            // For local development, ensure you have authenticated via 'gcloud auth application-default login'.
            _urlSigner =  UrlSigner.FromCredential(credentials);
            _storageClient = StorageClient.Create(credentials);
        }



        public async Task<string> GenerateUploadUrlAsync(string objectName, string contentType)
        {
            // The client will use this URL with an HTTP PUT request.
            var options = UrlSigner.Options.FromDuration(TimeSpan.FromMinutes(15));
            var request = UrlSigner.RequestTemplate
                .FromBucket(_bucketName).WithObjectName(objectName).WithHttpMethod(HttpMethod.Put)
                .WithContentHeaders(new Dictionary<string, IEnumerable<string>>
                {
                { "Content-Type", new[] { contentType } }
                });

            return await _urlSigner.SignAsync(request, options);
        }

        public async Task<string> UploadFileAsync(string objectName, Stream source, string contentType)
        {
            objectName = _folderOriginalImage + objectName;
            var uploaded = await _storageClient.UploadObjectAsync(
               _bucketName,
               objectName,
               contentType,
               source);

            // Public URL pattern (works with PublicRead)
            return $"https://storage.googleapis.com/{_bucketName}/{objectName}";
            // Or: return uploaded.MediaLink; (requires auth sometimes)
        }
    }
}
