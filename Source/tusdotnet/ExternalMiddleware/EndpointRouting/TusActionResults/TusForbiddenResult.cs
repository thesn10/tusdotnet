#if endpointrouting

using Microsoft.AspNetCore.Mvc;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusForbiddenResult : ISimpleResult, IFileInfoResult, IWriteResult, ICreateResult
    {
        public bool IsSuccessResult => false;

        public IActionResult Translate()
        {
            return new ForbidResult();
        }
    }
}

#endif