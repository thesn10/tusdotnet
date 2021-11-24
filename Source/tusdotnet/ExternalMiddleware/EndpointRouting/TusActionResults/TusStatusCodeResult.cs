#if endpointrouting

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusStatusCodeResult : ISimpleResult, IFileInfoResult, IWriteResult, ICreateResult
    {
        public TusStatusCodeResult()
        {
        }

        public TusStatusCodeResult(HttpStatusCode statusCode, string message = null)
        {
            StatusCode = statusCode;
            Message = message;
            CacheNoStore = statusCode == HttpStatusCode.NotFound;

            // Do not leak information on the tus endpoint during authorization.
            TusResumableHeader = statusCode != HttpStatusCode.Unauthorized;
        }

        public HttpStatusCode StatusCode { get; }
        public string Message { get; set; }

        public bool CacheNoStore { get; set; }
        public bool TusResumableHeader { get; set; }

        public bool IsSuccessResult => (int)StatusCode >= 200 && (int)StatusCode <= 299;

        public Task Execute(TusContext context)
        {
            context.HttpContext.Response.StatusCode = (int)StatusCode;

            if (TusResumableHeader)
            {
                RequestHandler.SetTusResumableHeader(context.HttpContext);
            }

            if (CacheNoStore)
            {
                RequestHandler.SetCacheNoStoreHeader(context.HttpContext);
            }

            if (!string.IsNullOrEmpty(Message))
            {
                context.HttpContext.Response.ContentType = "text/plain";
                return context.HttpContext.Response.WriteAsync(Message);
            }

            return Task.CompletedTask;
        }
    }
}

#endif
