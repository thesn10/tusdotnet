#if endpointrouting

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Net;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers;
using tusdotnet.Models.Concatenation;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusFileInfoResult : IFileInfoResult
    {
        public TusFileInfoResult(GetFileInfoResult result)
            : this(result.UploadMetadata, result.UploadLength, result.UploadOffset, result.UploadConcat)
        {
        }

        public TusFileInfoResult(string uploadMetadata, long? uploadLength, long uploadOffset, FileConcat? uploadConcat = null)
        {
            UploadMetadata = uploadMetadata;
            UploadLength = uploadLength;
            UploadOffset = uploadOffset;
            UploadConcat = uploadConcat;
        }

        public string UploadMetadata { get; set; }
        public long? UploadLength { get; set; }
        public long? UploadOffset { get; set; }
        public FileConcat? UploadConcat { get; set; }

        public bool IsSuccessResult => true;

        public Task Execute(TusContext context)
        {
            RequestHandler.SetTusResumableHeader(context.HttpContext);
            RequestHandler.SetCacheNoStoreHeader(context.HttpContext);

            if (!string.IsNullOrEmpty(UploadMetadata))
            {
                context.HttpContext.Response.Headers.Add(HeaderConstants.UploadMetadata, UploadMetadata);
            }

            if (UploadLength != null && UploadLength >= 0)
            {
                context.HttpContext.Response.Headers.Add(HeaderConstants.UploadLength, UploadLength.Value.ToString());
            }
            else if (context.ExtensionInfo.SupportedExtensions.CreationDeferLength)
            {
                context.HttpContext.Response.Headers.Add(HeaderConstants.UploadDeferLength, "1");
            }

            var addUploadOffset = true;
            if (UploadConcat != null)
            {
                // Only add Upload-Offset to final files if they are complete.
                if (UploadConcat is FileConcatFinal && UploadLength != UploadOffset)
                {
                    addUploadOffset = false;
                }
            }

            if (addUploadOffset)
            {
                context.HttpContext.Response.Headers.Add(HeaderConstants.UploadOffset, UploadOffset.ToString());
            }

            if (UploadConcat != null)
            {
                (UploadConcat as FileConcatFinal)?.AddUrlPathToFiles(context.HttpContext.Request.GetDisplayUrl());
                context.HttpContext.Response.Headers.Add(HeaderConstants.UploadConcat, UploadConcat.GetHeader());
            }

            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            return Task.CompletedTask;
        }
    }
}
#endif
