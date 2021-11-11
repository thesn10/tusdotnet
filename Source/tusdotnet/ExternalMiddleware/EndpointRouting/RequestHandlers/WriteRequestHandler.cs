#if endpointrouting
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.ExternalMiddleware.EndpointRouting.Validation;
using tusdotnet.ExternalMiddleware.EndpointRouting.Validation.Requirements;
using tusdotnet.Models;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers
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
            new UploadOffset(),
        };

        private readonly string _fileId;

        internal WriteRequestHandler(HttpContext context, TusControllerBase controller, TusExtensionInfo extensionInfo, ITusEndpointOptions options, string fileId = null)
            : base(context, controller, extensionInfo, options)
        {
            // used on creation-with-upload
            _fileId = fileId;
        }

        internal override async Task<IActionResult> Invoke()
        {
            var authorizeContext = new AuthorizeContext()
            {
                IntentType = IntentType.GetFileInfo,
                ControllerMethod = ((Func<WriteContext, Task<IWriteResult>>)_controller.Write).Method,
            };

            var authorizeResult = await _controller.Authorize(authorizeContext);

            if (!authorizeResult.IsSuccessResult)
            {
                return authorizeResult.Translate();
            }

            SetTusResumableHeader();

            long? uploadLength = null;
            if (_context.Request.Headers.ContainsKey(HeaderConstants.UploadLength))
            {
                // creation-defer-length
                uploadLength = long.Parse(_context.Request.Headers[HeaderConstants.UploadLength].First());
            }

            long uploadOffset;
            if (_context.Request.Headers.ContainsKey(HeaderConstants.UploadOffset))
            {
                uploadOffset = long.Parse(_context.Request.Headers[HeaderConstants.UploadOffset].First());
            }
            else
            {
                // creation-with-upload
                uploadOffset = 0;
            }

            var writeContext = new WriteContext
            {
                FileId = _fileId ?? (string)_context.GetRouteValue("TusFileId"),
                // Callback to later support trailing checksum headers
                GetChecksumProvidedByClient = () => GetChecksumFromContext(_context),
                RequestStream = _context.Request.Body,
                RequestReader = _context.Request.BodyReader,
                UploadOffset = uploadOffset,
                UploadLength = uploadLength
            };

            IWriteResult writeResult = null;
            try
            {
                writeResult = await _controller.Write(writeContext);
            }
            catch (TusException ex)
            {
                return new ObjectResult(ex.Message)
                {
                    StatusCode = (int)ex.StatusCode
                };
            }

            if (writeResult is TusBadRequestResult fail) return fail.Translate();
            if (writeResult is TusForbiddenResult forbid) return forbid.Translate();

            var writeOk = writeResult as TusWriteStatusResult;
            if (writeOk == null)
            {
                throw new InvalidOperationException($"Unknown action result: {writeResult.GetType().FullName}");
            }

            if (writeOk.ClientDisconnectedDuringRead)
            {
                // ?
                return new BadRequestResult();
            }

            if (writeOk.IsComplete && !writeContext.IsPartialFile)
            {
                await _controller.FileCompleted(new() { FileId = writeContext.FileId });
            }

            SetCreateHeaders(writeOk.FileExpires, writeOk.UploadOffset);
            return new NoContentResult();
        }

        private Checksum GetChecksumFromContext(HttpContext context)
        {
            var header = context.Request.Headers[HeaderConstants.UploadChecksum].FirstOrDefault();

            return header != null ? new Checksum(header) : null;
        }
    }
}
#endif
