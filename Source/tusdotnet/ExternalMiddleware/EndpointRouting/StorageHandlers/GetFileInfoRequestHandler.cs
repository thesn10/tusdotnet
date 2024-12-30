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
using tusdotnet.ExternalMiddleware.EndpointRouting.Validation;
using tusdotnet.Models;
using tusdotnet.Models.Concatenation;
using tusdotnet.Parsers;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers
{
    internal class GetFileInfoRequestHandler : RequestHandler
    {
        internal override RequestRequirement[] Requires => new RequestRequirement[]
        {

        };

        internal GetFileInfoRequestHandler(HttpContext context, TusControllerBase controller, ITusEndpointOptions options)
            : base(context, controller, options)
        {

        }

        internal override async Task<IActionResult> Invoke()
        {
            if (!await _controller.AuthorizeForAction(nameof(_controller.GetFileInfo)))
                return new ForbidResult();

            var fileId = (string)_context.GetRouteValue("TusFileId");

            SetTusResumableHeader();
            SetCacheNoStoreHeader();

            var getInfoContext = new GetFileInfoContext()
            {
                FileId = fileId,
            };

            ITusInfoActionResult getInfoResult;
            try
            {
                getInfoResult = await _controller.GetFileInfo(getInfoContext, _context.RequestAborted);
            }
            catch (TusStoreException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }

            if (getInfoResult is TusFail fail)
            {
                return new BadRequestObjectResult(fail.Error);
            }

            if (getInfoResult is TusForbidden forbidden)
            {
                return new ForbidResult();
            }

            var getInfoOk = getInfoResult as TusInfoOk;

            if (getInfoOk == null)
            {
                throw new InvalidOperationException($"Unknown action result: {getInfoResult.GetType().FullName}");
            }

            if (!string.IsNullOrEmpty(getInfoOk.UploadMetadata))
            {
                _context.Response.Headers.Add(HeaderConstants.UploadMetadata, getInfoOk.UploadMetadata);
            }

            if (getInfoOk.UploadDeferLength)
            {
                _context.Response.Headers.Add(HeaderConstants.UploadDeferLength, "1");
            }
            else if (getInfoOk.UploadLength != null)
            {
                _context.Response.Headers.Add(HeaderConstants.UploadLength, getInfoOk.UploadLength.Value.ToString());
            }

            var addUploadOffset = true;
            if (getInfoOk.UploadConcat != null)
            {
                // Only add Upload-Offset to final files if they are complete.
                if (getInfoOk.UploadConcat is FileConcatFinal && getInfoOk.UploadLength != getInfoOk.UploadOffset)
                {
                    addUploadOffset = false;
                }
            }

            if (addUploadOffset)
            {
                _context.Response.Headers.Add(HeaderConstants.UploadOffset, getInfoOk.UploadOffset.ToString());
            }

            if (getInfoOk.UploadConcat != null)
            {
                (getInfoOk.UploadConcat as FileConcatFinal)?.AddUrlPathToFiles(_context.Request.GetDisplayUrl());
                _context.Response.Headers.Add(HeaderConstants.UploadConcat, getInfoOk.UploadConcat.GetHeader());
            }

            return new NoContentResult();
        }
    }
}
#endif
