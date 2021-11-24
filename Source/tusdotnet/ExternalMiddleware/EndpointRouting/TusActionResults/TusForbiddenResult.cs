#if endpointrouting

using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusForbiddenResult : ISimpleResult, IFileInfoResult, IWriteResult, ICreateResult
    {
        public bool IsSuccessResult => false;

        public Task Execute(TusContext context)
        {
            return new TusStatusCodeResult(HttpStatusCode.Forbidden).Execute(context);
        }
    }
}

#endif