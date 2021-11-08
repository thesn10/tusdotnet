using System;
using System.Runtime.Serialization;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    [Serializable]
    public class TusInvalidOffsetException : TusException
    {
        public TusInvalidOffsetException()
        {
        }

        public TusInvalidOffsetException(string message) : base(message)
        {
        }
    }
}