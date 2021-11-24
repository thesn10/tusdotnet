using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using tusdotnet.Constants;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    [Serializable]
    public class TusChecksumException : TusException
    {
        public TusChecksumException()
        {
        }

        public TusChecksumException(HttpStatusCode statusCode, string message) : base(message, statusCode)
        {
        }

        public static void ThrowCouldNotParseHeader()
        {
            throw new TusChecksumException(HttpStatusCode.BadRequest, $"Could not parse {HeaderConstants.UploadChecksum} header");
        }

        public static void ThrowChecksumNotMatching()
        {
            throw new TusChecksumException((HttpStatusCode)460, "Header Upload-Checksum does not match the checksum of the file");
        }

        public static void ThrowUnsupportedAlgorithm(IEnumerable<string> checksumAlgorithms)
        {
            throw new TusChecksumException(HttpStatusCode.BadRequest, $"Unsupported checksum algorithm. Supported algorithms are: {string.Join(",", checksumAlgorithms)}");
        }
    }
}