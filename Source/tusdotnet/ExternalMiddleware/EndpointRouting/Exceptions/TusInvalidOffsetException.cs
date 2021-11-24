using System.Net;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusInvalidOffsetException : TusException
    {
        public TusInvalidOffsetException()
        {
        }

        public TusInvalidOffsetException(string message) : base(message, HttpStatusCode.Conflict)
        {
        }
    }
}