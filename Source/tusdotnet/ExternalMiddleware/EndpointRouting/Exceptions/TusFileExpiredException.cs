using System;
using System.Net;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
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