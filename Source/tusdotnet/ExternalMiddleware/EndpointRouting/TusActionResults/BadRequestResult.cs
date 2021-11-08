#if endpointrouting

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusBadRequest : ICompletedResult, IInfoResult, IWriteResult, ICreateResult, IDeleteResult
    {
        public TusBadRequest()
        {
        }

        public TusBadRequest(string message)
        {
            Message = message;
        }
        public string Message { get; set; }
    }
}

#endif
