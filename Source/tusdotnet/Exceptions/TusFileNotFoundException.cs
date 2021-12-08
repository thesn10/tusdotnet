using System.Net;

namespace tusdotnet.Exceptions
{
    /// <summary>
    /// Exception thrown if the file was not found
    /// </summary>
    public class TusFileNotFoundException : TusException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TusFileNotFoundException"/> class.
        /// </summary>
        public TusFileNotFoundException() : base(HttpStatusCode.NotFound)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TusFileNotFoundException"/> class.
        /// </summary>
        public TusFileNotFoundException(string message) : base(message, HttpStatusCode.NotFound)
        {
        }
    }
}