using System;
using System.Runtime.Serialization;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    [Serializable]
    public class TusInvalidConcatException : TusException
    {
        public TusInvalidConcatException()
        {
        }

        public TusInvalidConcatException(string message) : base(message)
        {
        }
    }
}