#if endpointrouting

using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading.Tasks;
using tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers;
using tusdotnet.Models.Concatenation;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusWriteStatusResult : IWriteResult
    {
        public TusWriteStatusResult(WriteResult writeResult)
            : this(writeResult.IsComplete, writeResult.UploadOffset, writeResult.ClientDisconnectedDuringRead, 
                   writeResult.FileExpires, writeResult.FileConcat)
        {
        }

        public TusWriteStatusResult(bool isComplete, long uploadOffset, bool clientDisconnectedDuringRead, DateTimeOffset? fileExpires = null, FileConcat? fileConcat = null)
        {
            IsComplete = isComplete;
            UploadOffset = uploadOffset;
            ClientDisconnectedDuringRead = clientDisconnectedDuringRead;
            FileExpires = fileExpires;
            FileConcat = fileConcat;
        }

        public bool IsComplete { get; set; }
        public long UploadOffset { get; internal set; }
        public bool ClientDisconnectedDuringRead { get; internal set; }

        // Expiration Extension
        public DateTimeOffset? FileExpires { get; set; }

        // Concatenation Extension
        public FileConcat? FileConcat { get; set; }

        public bool IsSuccessResult => true;

        public Task Execute(TusContext context)
        {
            if (ClientDisconnectedDuringRead)
            {
                // ?
                //return new TusBadRequestResult().Execute(context);
                return Task.CompletedTask;
            }

            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NoContent;

            RequestHandler.SetTusResumableHeader(context.HttpContext);
            RequestHandler.SetCommonHeaders(context.HttpContext, FileExpires, UploadOffset);

            return Task.CompletedTask;
        }
    }
}
#endif
