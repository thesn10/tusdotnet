namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Exception thrown when there is a problem with the upload length
    /// </summary>
    public class TusUploadLengthException : TusException
    {
        /// <summary>
		/// Initializes a new instance of the <see cref="TusUploadLengthException"/> class.
		/// </summary>
        public TusUploadLengthException()
        {
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="TusUploadLengthException"/> class.
		/// </summary>
        public TusUploadLengthException(string message) : base(message)
        {
        }
    }
}