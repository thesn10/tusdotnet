#if endpointrouting

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusFail : ITusCompletedActionResult, ITusInfoActionResult, ITusWriteActionResult, ITusCreateActionResult
    {
        public TusFail(string error)
        {
            Error = error;
        }
        public string Error { get; set; }
    }
}

#endif
