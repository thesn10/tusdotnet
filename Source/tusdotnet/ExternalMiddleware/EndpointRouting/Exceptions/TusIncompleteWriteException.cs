using System;
using System.Net;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Exception thrown if a write operation did not fully complete, but still managed to write some of the data
    /// </summary>
    public class TusIncompleteWriteException : TusException
    {
        /// <summary>
        /// Current upload offset of the file
        /// </summary>
        public long UploadOffset { get; }

        /// <summary>
		/// Initializes a new instance of the <see cref="TusIncompleteWriteException"/> class.
		/// </summary>
        public TusIncompleteWriteException(long uploadOffset, Exception innerException) 
            : base(innerException.Message, (innerException as TusException)?.StatusCode ?? HttpStatusCode.BadRequest, innerException)
        {
            UploadOffset = uploadOffset;
        }
    }
}