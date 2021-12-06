using System.Collections.Generic;
using System.Net;
using tusdotnet.Constants;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Exception thrown if there is a checksum error
    /// </summary>
    public class TusChecksumException : TusException
    {
        /// <summary>
		/// Initializes a new instance of the <see cref="TusChecksumException"/> class.
		/// </summary>
        public TusChecksumException()
        {
        }

        /// <summary>
		/// Initializes a new instance of the <see cref="TusChecksumException"/> class.
		/// </summary>
        public TusChecksumException(HttpStatusCode statusCode, string message) : base(message, statusCode)
        {
        }

        internal static void ThrowCouldNotParseHeader()
        {
            throw new TusChecksumException(HttpStatusCode.BadRequest, $"Could not parse {HeaderConstants.UploadChecksum} header");
        }

        internal static void ThrowChecksumNotMatching()
        {
            throw new TusChecksumException((HttpStatusCode)460, "Header Upload-Checksum does not match the checksum of the file");
        }

        internal static void ThrowUnsupportedAlgorithm(IEnumerable<string> checksumAlgorithms)
        {
            throw new TusChecksumException(HttpStatusCode.BadRequest, $"Unsupported checksum algorithm. Supported algorithms are: {string.Join(",", checksumAlgorithms)}");
        }
    }
}