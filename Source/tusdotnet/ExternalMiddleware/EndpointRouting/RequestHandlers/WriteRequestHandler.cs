#if endpointrouting
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tusdotnet.Adapters;
using tusdotnet.Constants;
using tusdotnet.Models;
using tusdotnet.Parsers;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers
{
    internal class WriteRequestHandler : RequestHandler
    {
        private readonly string _fileId;

        internal WriteRequestHandler(HttpContext context, TusControllerBase controller, TusEndpointOptions options, string fileId = null)
            : base(context, controller, options)
        {
            // used on creation-with-upload
            _fileId = fileId;
        }

        internal override async Task<IActionResult> Invoke()
        {
            if (!await _controller.AuthorizeForAction(_context, nameof(_controller.Write)))
                return new ForbidResult();

            SetTusResumableHeader();

            long? uploadLength = null;
            if (_context.Request.Headers.ContainsKey(HeaderConstants.UploadLength))
            {
                uploadLength = long.Parse(_context.Request.Headers[HeaderConstants.UploadLength].First());
            }

            long? uploadOffset = null;
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
                UploadOffset = uploadOffset,
                UploadLength = uploadLength
            };

            ITusWriteActionResult writeResult = null;
            try
            {
                writeResult = await _controller.Write(writeContext, _context.RequestAborted);
            }
            catch (TusFileAlreadyInUseException ex)
            {
                return new ConflictObjectResult(ex.Message);
            }
            catch (TusStoreException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

            if (writeResult is TusFail fail)
            {
                return new BadRequestObjectResult(fail.Error);
            }
            else if (writeResult is TusForbidden forbidden)
            {
                return new ForbidResult();
            }

            var writeOk = writeResult as TusWriteOk;

            if (writeOk == null)
            {
                throw new InvalidOperationException($"Unknown action result: {writeResult.GetType().FullName}");
            }

            if (writeOk.ClientDisconnectedDuringRead)
            {
                return new OkResult();
            }

            if (writeOk.IsComplete && !writeContext.IsPartialFile)
            {
                await _controller.FileCompleted(new() { FileId = writeContext.FileId }, _context.RequestAborted);
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
