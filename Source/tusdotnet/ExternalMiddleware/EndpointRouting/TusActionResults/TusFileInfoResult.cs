using System.Net;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers;
using tusdotnet.Helpers;
using tusdotnet.Models.Concatenation;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// An <see cref="ITusActionResult"/> that when executed will produce a tus file info response.
    /// </summary>
    public class TusFileInfoResult : IFileInfoResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TusFileInfoResult"/> class.
        /// </summary>
        public TusFileInfoResult(GetFileInfoResult result)
            : this(result.UploadMetadata, result.UploadLength, result.UploadOffset, result.FileConcatenation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TusFileInfoResult"/> class.
        /// </summary>
        public TusFileInfoResult(string uploadMetadata, long? uploadLength, long uploadOffset, FileConcat? uploadConcat = null)
        {
            UploadMetadata = uploadMetadata;
            UploadLength = uploadLength;
            UploadOffset = uploadOffset;
            UploadConcat = uploadConcat;
        }

        /// <summary>
        /// Value to set the Upload-Metadata Header
        /// </summary>
        public string UploadMetadata { get; set; }

        /// <summary>
        /// Value to set the Upload-Length Header
        /// </summary>
        public long? UploadLength { get; set; }

        /// <summary>
        /// Value to set the Upload-Offset Header
        /// </summary>
        public long? UploadOffset { get; set; }

        /// <summary>
        /// Value to set the Upload-Concat Header
        /// </summary>
        public FileConcat? UploadConcat { get; set; }

        /// <summary>
        /// Set to false if you want to generate the file urls yourself (using <see cref="FileConcatFinal.Files"/>)
        /// </summary>
        public bool GenerateUploadConcatFileUrls { get; set; } = true;

        /// <inheritdoc />
        public bool IsSuccessResult => true;

        /// <inheritdoc />
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
                if (GenerateUploadConcatFileUrls && UploadConcat is FileConcatFinal final)
                {
                    final.AddUrlPathToFiles(context.RoutingHelper);
                }
                context.HttpContext.Response.Headers.Add(HeaderConstants.UploadConcat, UploadConcat.GetHeader());
            }

            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
            return TaskHelper.Completed;
        }
    }
}
