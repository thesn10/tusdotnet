using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;
using tusdotnet.Controllers;
using tusdotnet.Routing;

namespace tusdotnet.Tus2
{
    public class Tus2BaseResponse : ITusActionResult
    {
        public HttpStatusCode Status { get; set; }

        public string ErrorMessage { get; set; }

        public bool DisconnectClient { get; set; }

        public bool IsSuccessResult => (int)Status >= 200 && (int)Status <= 299;

        protected bool NoCache { get; set; }

        public virtual async Task Execute(TusContext tusContext)
        {
            if (DisconnectClient)
            {
                tusContext.HttpContext.Abort();
                return;
            }

            if (NoCache)
            {
                tusContext.HttpContext.SetHeader("Cache-Control", "no-cache");
            }

            if (!IsSuccessResult)
            {
                await tusContext.HttpContext.Error(Status, ErrorMessage);
                return;
            }

            tusContext.HttpContext.Response.StatusCode = (int)Status;
        }
    }
}
