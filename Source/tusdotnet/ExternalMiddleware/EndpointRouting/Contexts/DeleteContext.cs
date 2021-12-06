namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Context for the Delete action
    /// </summary>
    public class DeleteContext
    {
        /// <summary>
        /// The file id of the request
        /// </summary>
        public string FileId { get; set; }
    }
}