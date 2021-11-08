using System;
using System.Net;
using System.Runtime.Serialization;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    [Serializable]
    public class TusFileNotFoundException : TusException
    {
        public TusFileNotFoundException() : base(HttpStatusCode.NotFound)
        {
        }

        public TusFileNotFoundException(string message) : base(message, HttpStatusCode.NotFound)
        {
        }
    }
}