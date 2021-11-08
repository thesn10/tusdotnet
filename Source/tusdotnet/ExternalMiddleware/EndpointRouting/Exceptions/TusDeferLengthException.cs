using System;
using System.Runtime.Serialization;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusDeferLengthException : TusException
    {
        public TusDeferLengthException()
        {
        }

        public TusDeferLengthException(string message) : base(message)
        {
        }
    }
}