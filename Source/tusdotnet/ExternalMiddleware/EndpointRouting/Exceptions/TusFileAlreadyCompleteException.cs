using System;
using System.Runtime.Serialization;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    [Serializable]
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