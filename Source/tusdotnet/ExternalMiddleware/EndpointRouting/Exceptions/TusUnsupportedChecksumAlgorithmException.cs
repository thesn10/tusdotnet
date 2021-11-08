using System;
using System.Runtime.Serialization;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    [Serializable]
    public class TusUnsupportedChecksumAlgorithmException : Exception
    {
        public TusUnsupportedChecksumAlgorithmException()
        {
        }

        public TusUnsupportedChecksumAlgorithmException(string message) : base(message)
        {
        }
    }
}