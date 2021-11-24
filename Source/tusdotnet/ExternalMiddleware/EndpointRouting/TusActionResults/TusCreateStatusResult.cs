#if endpointrouting

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Net;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers;
using tusdotnet.Helpers;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusCreateStatusResult : ICreateResult
    {
        public TusCreateStatusResult(CreateResult result)
            : this(result.FileId, result.FileExpires)
        {
        }

        public TusCreateStatusResult(string fileId, DateTimeOffset? expires = null)
        {
            FileId = fileId;
            Expires = expires;
        }

        public string FileId { get; set; }
        public DateTimeOffset? Expires { get; set; }

        public long? UploadOffset { get; set; }

        public bool IsSuccessResult => true;

        public Task Execute(TusContext context)
        {
            RequestHandler.SetTusResumableHeader(context.HttpContext);
            RequestHandler.SetCommonHeaders(context.HttpContext, Expires, UploadOffset);

            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;
            context.HttpContext.Response.Headers[HeaderConstants.Location] = $"{context.HttpContext.Request.Path.Value.TrimEnd('/')}/{FileId}"; // TODO

            return Task.CompletedTask;
        }
    }
}

#endif