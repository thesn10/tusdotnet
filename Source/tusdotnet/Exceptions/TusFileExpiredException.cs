using System.Net;

namespace tusdotnet.Exceptions
{
    /// <summary>
    /// Exception thrown if there is an attempt to update a expired file
    /// </summary>
    public class TusFileExpiredException : TusException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TusFileExpiredException"/> class.
        /// </summary>
        public TusFileExpiredException() : base(HttpStatusCode.NotFound)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TusFileExpiredException"/> class.
        /// </summary>
        public TusFileExpiredException(string message) : base(message, HttpStatusCode.NotFound)
        {
        }
    }
}