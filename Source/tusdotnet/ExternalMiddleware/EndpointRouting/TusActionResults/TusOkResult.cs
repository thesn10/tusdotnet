using Microsoft.AspNetCore.Mvc;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusOkResult : ISimpleResult
    {
        public bool IsSuccessResult => true;

        public IActionResult Translate()
        {
            return new OkResult();
        }
    }
}