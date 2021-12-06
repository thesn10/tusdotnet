using System.Net;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Exception thrown if the file is too large
    /// </summary>
    public class TusFileTooLargeException : TusException
    {
        /// <summary>
		/// Initializes a new instance of the <see cref="TusFileTooLargeException"/> class.
		/// </summary>
        public TusFileTooLargeException() : base(HttpStatusCode.RequestEntityTooLarge)
        {
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="TusFileTooLargeException"/> class.
		/// </summary>
        public TusFileTooLargeException(string message) : base(message, HttpStatusCode.RequestEntityTooLarge)
        {
        }
    }
}