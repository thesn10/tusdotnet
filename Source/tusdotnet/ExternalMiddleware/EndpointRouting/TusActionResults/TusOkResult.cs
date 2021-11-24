using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusOkResult : ISimpleResult
    {
        public bool IsSuccessResult => true;

        public Task Execute(TusContext context)
        {
            return new TusStatusCodeResult(HttpStatusCode.NoContent).Execute(context);
        }
    }
}