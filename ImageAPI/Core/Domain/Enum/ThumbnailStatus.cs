namespace ImageAPI.Core.Domain.Enum
{
    /// <summary>
    /// Represents the status of the thumbnail generation process.
    /// </summary>
    public enum ThumbnailStatus
    {
        /// <summary>
        /// The original image has been uploaded, but thumbnail generation has not started.
        /// </summary>
        Pending,

        /// <summary>
        /// The thumbnail is currently being generated.
        /// </summary>
        Processing,

        /// <summary>
        /// The thumbnail was successfully generated.
        /// </summary>
        Completed,

        /// <summary>
        /// An error occurred during thumbnail generation.
        /// </summary>
        Failed
    }
}
