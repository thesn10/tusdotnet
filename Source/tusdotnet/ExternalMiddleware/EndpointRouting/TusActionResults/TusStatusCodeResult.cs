using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;
using tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers;
using tusdotnet.Helpers;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// An <see cref="ITusActionResult"/> that when executed will produce a response with the specified http status code.
    /// </summary>
    public class TusStatusCodeResult : ISimpleResult, IFileInfoResult, IWriteResult, ICreateResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TusStatusCodeResult"/> class.
        /// </summary>
        public TusStatusCodeResult(HttpStatusCode statusCode, string message = null)
        {
            StatusCode = statusCode;
            Message = message;
            CacheNoStore = statusCode == HttpStatusCode.NotFound;

            // Do not leak information on the tus endpoint during authorization.
            TusResumableHeader = statusCode != HttpStatusCode.Unauthorized;
        }

        /// <summary>
        /// The http status code of the response
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Message to write into the request body if needed
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Set cache no store header
        /// </summary>
        public bool CacheNoStore { get; set; }

        /// <summary>
        /// Set the Tus-Resumable header
        /// </summary>
        public bool TusResumableHeader { get; set; }

        /// <summary>
        /// If applicable, the exception that caused this result
        /// </summary>
        public TusException? Exception { get; set; }

        /// <inheritdoc />
        public bool IsSuccessResult => (int)StatusCode >= 200 && (int)StatusCode <= 299;

        /// <inheritdoc />
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

            return TaskHelper.Completed;
        }
    }
}
