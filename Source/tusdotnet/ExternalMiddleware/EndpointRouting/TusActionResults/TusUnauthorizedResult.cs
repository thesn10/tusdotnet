using System.Net;
using System.Threading.Tasks;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusUnauthorizedResult : ISimpleResult, IFileInfoResult, IWriteResult, ICreateResult
    {
        public bool IsSuccessResult => false;

        public Task Execute(TusContext context)
        {
            return new TusStatusCodeResult(HttpStatusCode.Unauthorized).Execute(context);
        }
    }
}