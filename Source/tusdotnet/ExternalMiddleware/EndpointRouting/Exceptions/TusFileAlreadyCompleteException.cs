using System;
using System.Runtime.Serialization;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    [Serializable]
    public class TusFileAlreadyCompleteException : TusException
    {
        public TusFileAlreadyCompleteException()
        {
        }

        public TusFileAlreadyCompleteException(string message) : base(message)
        {
        }
    }
}