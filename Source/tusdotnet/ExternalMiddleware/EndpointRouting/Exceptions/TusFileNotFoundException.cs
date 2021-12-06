using System.Net;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
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