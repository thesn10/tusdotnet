using System.Net;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusFileAlreadyInUseException : TusException
    {
        public TusFileAlreadyInUseException(string fileId) : base($"File {fileId} is currently being updated. Please try again later", HttpStatusCode.Conflict)
        {
        }
    }
}