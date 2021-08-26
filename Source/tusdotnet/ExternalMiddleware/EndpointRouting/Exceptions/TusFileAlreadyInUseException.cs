#if endpointrouting

using System;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusFileAlreadyInUseException : Exception
    {
        public TusFileAlreadyInUseException(string fileId) : base($"File {fileId} is currently being updated. Please try again later")
        {
        }
    }
}
#endif