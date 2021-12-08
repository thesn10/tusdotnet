using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.Controllers;
using tusdotnet.Exceptions;
using tusdotnet.Extensions;
using tusdotnet.Models;
using tusdotnet.Models.Concatenation;
using tusdotnet.RequestHandlers.Validation;
using tusdotnet.Routing;

namespace tusdotnet.RequestHandlers
{
    /*
    * The Server SHOULD accept PATCH requests against any upload URL and apply the bytes 
    * contained in the message at the given offset specified by the Upload-Offset header. 
    * All PATCH requests MUST use Content-Type: application/offset+octet-stream.
    * The Upload-Offset header’s value MUST be equal to the current offset of the resource. 
    * In order to achieve parallel upload the Concatenation extension MAY be used. 
    * If the offsets do not match, the Server MUST respond with the 409 Conflict status without modifying the upload resource.
    * The Client SHOULD send all the remaining bytes of an upload in a single PATCH request, 
    * but MAY also use multiple small requests successively for scenarios where 
    * this is desirable, for example, if the Checksum extension is used.
    * The Server MUST acknowledge successful PATCH requests with the 204 No Content status. 
    * It MUST include the Upload-Offset header containing the new offset. 
    * The new offset MUST be the sum of the offset before the PATCH request and the number of bytes received and 
    * processed or stored during the current PATCH request.
    * Both, Client and Server, SHOULD attempt to detect and handle network errors predictably. 
    * They MAY do so by checking for read/write socket errors, as well as setting read/write timeouts. 
    * A timeout SHOULD be handled by closing the underlying connection.
    * The Server SHOULD always attempt to store as much of the received data as possible.
    * 
    * The Server MUST respond with the 403 Forbidden status to PATCH requests against a final upload URL and 
    * MUST NOT modify the final or its partial uploads.
    * 
    * [Upload-Expires] This header MUST be included in every PATCH response if the upload is going to expire. 
    */
    internal class WriteRequestHandler : RequestHandler
    {
        internal override RequestRequirement[] Requires => new RequestRequirement[]
        {
            new ContentType(),
            _isCreationWithUpload ? null : new UploadOffset(),
            new UploadChecksumHeader(GetChecksumFromContext(HttpContext, false))
        };

        private readonly string _fileId;
        private readonly bool _isCreationWithUpload;

        internal WriteRequestHandler(TusContext context, TusControllerBase controller, string fileId, bool isCreationWithUpload = false)
            : base(context, controller)
        {
            _fileId = fileId;
            _isCreationWithUpload = isCreationWithUpload;
        }

        internal override async Task<ITusActionResult> Invoke()
        {
            long? uploadLength = null;
            if (!_isCreationWithUpload && HttpContext.Request.Headers.ContainsKey(HeaderConstants.UploadLength))
            {
                uploadLength = long.Parse(HttpContext.Request.Headers[HeaderConstants.UploadLength].First());
            }

            long uploadOffset;
            if (HttpContext.Request.Headers.ContainsKey(HeaderConstants.UploadOffset))
            {
                uploadOffset = long.Parse(HttpContext.Request.Headers[HeaderConstants.UploadOffset].First());
            }
            else
            {
                // assert _isCreationWithUpload == true

                uploadOffset = 0;
            }

            var writeContext = new WriteContext
            {
                FileId = _fileId,
                // Callback to support trailing checksum headers
                GetChecksumProvidedByClient = () => Task.FromResult(GetChecksumFromContext(HttpContext)),
                RequestStream = HttpContext.Request.Body,
#if pipelines
                RequestReader = HttpContext.Request.BodyReader,
#endif
                UploadOffset = uploadOffset,
                UploadLength = uploadLength,
                IsCreationWithUpload = _isCreationWithUpload,
            };

            IWriteResult writeResult = null;
            try
            {
                writeResult = await _controller.Write(writeContext);
            }
            catch (TusException ex)
            {
                return new TusStatusCodeResult(ex.StatusCode, ex.Message)
                {
                    Exception = ex,
                };
            }

            if (writeResult is TusWriteStatusResult writeOk)
            {
                var isEmptyFile = writeContext.UploadLength == 0;
                var isPartialFile = writeOk.FileConcatenation is FileConcatPartial;

                if (writeOk.IsComplete && !isPartialFile && !isEmptyFile)
                {
                    ISimpleResult completedResult;
                    try
                    {
                        completedResult = await _controller.FileCompleted(new() { FileId = writeContext.FileId });
                    }
                    catch (TusException ex)
                    {
                        return new TusStatusCodeResult(ex.StatusCode, ex.Message);
                    }

                    if (!completedResult.IsSuccessResult)
                        return completedResult;

                    return writeOk;
                }
            }

            return writeResult;
        }

        private Checksum GetChecksumFromContext(HttpContext context, bool canBeTrailing = true)
        {
            var header = context.Request.Headers[HeaderConstants.UploadChecksum].FirstOrDefault();

            if (header != null)
            {
                return new Checksum(header);
            }

#if trailingheaders
            if (canBeTrailing && context.Request.HasDeclaredTrailingUploadChecksumHeader())
            {
                header = context.Request.GetTrailingUploadChecksumHeader();

                if (header != null)
                {
                    return new Checksum(header);
                }
            }
#endif

            return null;
        }
    }
}
