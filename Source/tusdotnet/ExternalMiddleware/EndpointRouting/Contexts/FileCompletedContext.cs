namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Context for the FileCompleted action
    /// </summary>
    public class FileCompletedContext
    {
        /// <summary>
        /// The file id of the request
        /// </summary>
        public string FileId { get; set; }
    }
}