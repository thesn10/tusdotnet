using System.Collections.Generic;
using System.Net;
using tusdotnet.Constants;

namespace tusdotnet.Exceptions
{
    /// <summary>
    /// Exception thrown if there is an error related to checksums
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

        internal static TusChecksumException CouldNotParseHeader()
        {
            return new TusChecksumException(HttpStatusCode.BadRequest, $"Could not parse {HeaderConstants.UploadChecksum} header");
        }

        internal static TusChecksumException ChecksumNotMatching()
        {
            return new TusChecksumException((HttpStatusCode)460, "Header Upload-Checksum does not match the checksum of the file");
        }

        internal static TusChecksumException UnsupportedAlgorithm(IEnumerable<string> checksumAlgorithms)
        {
            return new TusChecksumException(HttpStatusCode.BadRequest, $"Unsupported checksum algorithm. Supported algorithms are: {string.Join(",", checksumAlgorithms)}");
        }
    }
}