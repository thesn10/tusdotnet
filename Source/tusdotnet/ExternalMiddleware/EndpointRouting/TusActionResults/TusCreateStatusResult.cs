using System;
using System.Net;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers;
using tusdotnet.Helpers;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// An <see cref="ITusActionResult"/> that when executed will produce a create status response.
    /// </summary>
    public class TusCreateStatusResult : ICreateResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TusCreateStatusResult"/> class.
        /// </summary>
        public TusCreateStatusResult(CreateResult result)
            : this(result.FileId, result.FileExpires)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TusCreateStatusResult"/> class.
        /// </summary>
        public TusCreateStatusResult(string fileId, DateTimeOffset? expires = null)
        {
            FileId = fileId;
            FileExpires = expires;
        }

        /// <summary>
        /// File id to construct the file location url from
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// Value to set the Upload-Expires Header
        /// </summary>
        public DateTimeOffset? FileExpires { get; set; }

        /// <summary>
        /// Value to set the Upload-Offset Header
        /// </summary>
        public long? UploadOffset { get; set; }

        /// <summary>
        /// Location url of the created file
        /// </summary>
        public string? Location { get; set; }

        /// <inheritdoc />
        public bool IsSuccessResult => true;

        /// <inheritdoc />
        public Task Execute(TusContext context)
        {
            RequestHandler.SetTusResumableHeader(context.HttpContext);
            RequestHandler.SetCommonHeaders(context.HttpContext, FileExpires, UploadOffset);

            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;

            if (Location != null)
            {
                context.HttpContext.Response.Headers[HeaderConstants.Location] = Location;
            }
            else
            {
                context.HttpContext.Response.Headers[HeaderConstants.Location] = context.RoutingHelper.GenerateFilePath(FileId);
            }

            return TaskHelper.Completed;
        }
    }
}