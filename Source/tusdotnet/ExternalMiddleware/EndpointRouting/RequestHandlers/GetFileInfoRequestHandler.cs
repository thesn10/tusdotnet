#if endpointrouting
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.ExternalMiddleware.EndpointRouting.Validation;
using tusdotnet.Models.Concatenation;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers
{
    /*
    * The Server MUST always include the Upload-Offset header in the response for a HEAD request, 
    * even if the offset is 0, or the upload is already considered completed. If the size of the upload is known, 
    * the Server MUST include the Upload-Length header in the response. 
    * If the resource is not found, the Server SHOULD return either the 404 Not Found, 410 Gone or 403 Forbidden 
    * status without the Upload-Offset header.
    * The Server MUST prevent the client and/or proxies from caching the response by adding the 
    * Cache-Control: no-store header to the response.
    * 
    * If an upload contains additional metadata, responses to HEAD requests MUST include the Upload-Metadata header 
    * and its value as specified by the Client during the creation.
    * 
    * The response to a HEAD request for a final upload SHOULD NOT contain the Upload-Offset header unless the 
    * concatenation has been successfully finished. After successful concatenation, the Upload-Offset and Upload-Length 
    * MUST be set and their values MUST be equal. The value of the Upload-Offset header before concatenation is not 
    * defined for a final upload. The response to a HEAD request for a partial upload MUST contain the Upload-Offset header.
    * The Upload-Length header MUST be included if the length of the final resource can be calculated at the time of the request. 
    * Response to HEAD request against partial or final upload MUST include the Upload-Concat header and its value as received 
    * in the upload creation request.
    */

    internal class GetFileInfoRequestHandler : RequestHandler
    {
        internal override RequestRequirement[] Requires => new RequestRequirement[]
        {

        };

        internal GetFileInfoRequestHandler(HttpContext context, TusControllerBase controller, TusExtensionInfo extensionInfo, ITusEndpointOptions options)
            : base(context, controller, extensionInfo, options)
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

            IInfoResult getInfoResult;
            try
            {
                getInfoResult = await _controller.GetFileInfo(getInfoContext);
            }
            catch (TusException ex)
            {
                return new ObjectResult(ex.Message)
                {
                    StatusCode = (int)ex.StatusCode
                };
            }

            if (getInfoResult is TusBadRequest fail) return new BadRequestObjectResult(fail.Message);
            if (getInfoResult is TusForbidden) return new ForbidResult();

            var getInfoOk = getInfoResult as TusInfoOk;

            if (getInfoOk == null)
            {
                throw new InvalidOperationException($"Unknown action result: {getInfoResult.GetType().FullName}");
            }

            if (!string.IsNullOrEmpty(getInfoOk.UploadMetadata))
            {
                _context.Response.Headers.Add(HeaderConstants.UploadMetadata, getInfoOk.UploadMetadata);
            }

            if (getInfoOk.UploadLength != null)
            {
                _context.Response.Headers.Add(HeaderConstants.UploadLength, getInfoOk.UploadLength.Value.ToString());
            }
            else if (_extensionInfo.SupportedExtensions.CreationDeferLength)
            {
                _context.Response.Headers.Add(HeaderConstants.UploadDeferLength, "1");
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
