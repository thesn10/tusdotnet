using System;
using System.Net;
using System.Threading.Tasks;
using tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers;
using tusdotnet.Helpers;
using tusdotnet.Models.Concatenation;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// An <see cref="ITusActionResult"/> that when executed will produce a write status response.
    /// </summary>
    public class TusWriteStatusResult : IWriteResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TusWriteStatusResult"/> class.
        /// </summary>
        public TusWriteStatusResult(WriteResult writeResult)
            : this(writeResult.UploadOffset, writeResult.IsComplete, writeResult.FileExpires, writeResult.FileConcatenation)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TusWriteStatusResult"/> class.
        /// </summary>
        public TusWriteStatusResult(long uploadOffset, bool isComplete, DateTimeOffset? fileExpires = null, FileConcat? fileConcat = null)
        {
            UploadOffset = uploadOffset;
            IsComplete = isComplete;
            FileExpires = fileExpires;
            FileConcatenation = fileConcat;
        }

        /// <summary>
        /// Value to set the Upload-Offset Header
        /// </summary>
        public long UploadOffset { get; set; }

        /// <summary>
        /// True if the file upload is complete
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Value to set the Upload-Expires Header
        /// </summary>
        public DateTimeOffset? FileExpires { get; set; }

        /// <summary>
        /// File concatenation information otherwise null
        /// </summary>
        public FileConcat FileConcatenation { get; set; }

        /// <inheritdoc />
        public bool IsSuccessResult => true;

        /// <inheritdoc />
        public Task Execute(TusContext context)
        {
            context.HttpContext.Response.StatusCode = (int)HttpStatusCode.NoContent;

            RequestHandler.SetTusResumableHeader(context.HttpContext);
            RequestHandler.SetCommonHeaders(context.HttpContext, FileExpires, UploadOffset);

            return TaskHelper.Completed;
        }
    }
}
