#if endpointrouting

using Microsoft.AspNetCore.Mvc;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusBadRequestResult : ISimpleResult, IFileInfoResult, IWriteResult, ICreateResult
    {
        public TusBadRequestResult()
        {
        }

        public TusBadRequestResult(string message)
        {
            Message = message;
        }
        public string Message { get; set; }

        public bool IsSuccessResult => false;

        public IActionResult Translate()
        {
            return new BadRequestObjectResult(Message);
        }
    }
}

#endif
