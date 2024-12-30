using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using tusdotnet.Exceptions;
using tusdotnet.Helpers;
using tusdotnet.Routing;
using tusdotnet.Tus2;

namespace tusdotnet.Controllers
{
    /// <summary>
    /// An <see cref="ITusActionResult"/> that when executed will produce a response with the specified http status code.
    /// </summary>
    public class Tus2BaseResult : ITusActionResult, ITus2RetrieveOffsetResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TusBaseResult"/> class.
        /// </summary>
        public Tus2BaseResult(HttpStatusCode statusCode, string message = null)
        {
            StatusCode = statusCode;
            Message = message;

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

        public bool DisconnectClient { get; set; }

        public long? UploadOffset { get; set; }
        public bool? UploadIncomplete { get; set; }

        public bool NoCache { get; set; }

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

            /*if (TusResumableHeader)
            {
                HeaderHelper.SetTusResumableHeader(context.HttpContext);
            }*/

            if (NoCache)
            {
                HeaderHelper.SetCacheNoCacheHeader(context.HttpContext);
            }

            if (!string.IsNullOrEmpty(Message))
            {
                context.HttpContext.Response.ContentType = "text/plain";
                return context.HttpContext.Response.WriteAsync(Message);
            }

            if (DisconnectClient)
            {
                context.HttpContext.Abort();
                return TaskHelper.Completed;
            }

            if (UploadOffset != null)
            {
                context.HttpContext.SetHeader("Upload-Offset", UploadOffset.ToString());
            }

            if (UploadIncomplete != null)
            {
                context.HttpContext.SetHeader("Upload-Incomplete", UploadIncomplete.Value.ToSfBool());
            }

            return TaskHelper.Completed;
        }
    }
}

