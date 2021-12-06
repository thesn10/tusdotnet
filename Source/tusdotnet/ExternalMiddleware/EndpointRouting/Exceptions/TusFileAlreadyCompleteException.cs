using System;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusFileAlreadyCompleteException : TusException
    {
        public TusFileAlreadyCompleteException() : base("Upload is already complete.")
        {
        }

        public TusFileAlreadyCompleteException(string message) : base(message)
        {
        }
    }
}