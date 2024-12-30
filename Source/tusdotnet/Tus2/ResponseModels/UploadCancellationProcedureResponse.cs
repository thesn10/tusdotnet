using tusdotnet.Routing;
using System.Net;
using System.Threading.Tasks;

namespace tusdotnet.Tus2
{
    public class UploadCancellationProcedureResponse : Tus2BaseResponse
    {
        public override Task Execute(TusContext context)
        {
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NoContent;
            return Task.CompletedTask;
        }
    }
}