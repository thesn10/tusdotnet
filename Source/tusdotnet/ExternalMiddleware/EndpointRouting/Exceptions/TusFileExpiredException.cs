using System;
using System.Net;
using System.Runtime.Serialization;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    [Serializable]
    public class TusFileExpiredException : TusException
    {
        public TusFileExpiredException() : base(HttpStatusCode.NotFound)
        {
        }

        public TusFileExpiredException(string message) : base(message, HttpStatusCode.NotFound)
        {
        }
    }
}