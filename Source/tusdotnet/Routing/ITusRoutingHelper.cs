namespace tusdotnet.Routing
{
    /// <summary>
    /// Provides helper methods for routing
    /// </summary>
    public interface ITusRoutingHelper
    {
        /// <summary>
        /// Generates a valid url path using the file id
        /// </summary>
        public string? GenerateFilePath(string fileId);

        /// <summary>
        /// Gets the current file id of the request
        /// </summary>
        public string? GetFileId();

        /// <summary>
        /// Parses the file id using the given url
        /// </summary>
        public string? ParseFileId(string url);

        /// <summary>
        /// Checks if the request route is valid
        /// </summary>
        public bool IsMatchingRoute();
    }
}
