using System.Net;
using System.Threading.Tasks;

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

        public Task Execute(TusContext context)
        {
            return new TusStatusCodeResult(HttpStatusCode.BadRequest, Message).Execute(context);
        }
    }
}
