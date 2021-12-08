using System.Net;

namespace tusdotnet.Exceptions
{
    /// <summary>
    /// Exception thrown if the concatenation of files fails
    /// </summary>
    public class TusInvalidConcatException : TusException
    {
        /// <summary>
		/// Initializes a new instance of the <see cref="TusInvalidConcatException"/> class.
		/// </summary>
        public TusInvalidConcatException()
        {
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="TusInvalidConcatException"/> class.
		/// </summary>
        public TusInvalidConcatException(string message, HttpStatusCode status = HttpStatusCode.BadRequest) : base(message, status)
        {
        }
    }
}