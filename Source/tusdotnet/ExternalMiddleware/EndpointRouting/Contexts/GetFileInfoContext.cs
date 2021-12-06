namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Context for the GetFileInfo action
    /// </summary>
    public class GetFileInfoContext
    {
        /// <summary>
        /// The file id of the request
        /// </summary>
        public string FileId { get; internal set; }
    }
}