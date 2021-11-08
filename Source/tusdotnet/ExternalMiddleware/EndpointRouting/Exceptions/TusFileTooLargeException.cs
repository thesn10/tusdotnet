using System;
using System.Net;
using System.Runtime.Serialization;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    [Serializable]
    public class TusFileTooLargeException : TusException
    {
        public TusFileTooLargeException() : base(HttpStatusCode.RequestEntityTooLarge)
        {
        }

        public TusFileTooLargeException(string message) : base(message, HttpStatusCode.RequestEntityTooLarge)
        {
        }
    }
}