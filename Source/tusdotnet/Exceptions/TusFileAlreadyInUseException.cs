using System.Net;

namespace tusdotnet.Exceptions
{
    /// <summary>
    /// Exception thrown if the file is currently used by another request
    /// </summary>
    public class TusFileAlreadyInUseException : TusException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TusFileAlreadyInUseException"/> class.
        /// </summary>
        public TusFileAlreadyInUseException(string fileId) : base($"File {fileId} is currently being updated. Please try again later", HttpStatusCode.Conflict)
        {
        }
    }
}