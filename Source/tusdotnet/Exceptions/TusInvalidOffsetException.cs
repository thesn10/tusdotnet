using System.Net;

namespace tusdotnet.Exceptions
{
    /// <summary>
    /// Exception thrown if the provided offset is invalid
    /// </summary>
    public class TusInvalidOffsetException : TusException
    {
        /// <summary>
		/// Initializes a new instance of the <see cref="TusInvalidOffsetException"/> class.
		/// </summary>
        public TusInvalidOffsetException()
        {
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="TusInvalidOffsetException"/> class.
		/// </summary>
        public TusInvalidOffsetException(string message) : base(message, HttpStatusCode.Conflict)
        {
        }
    }
}