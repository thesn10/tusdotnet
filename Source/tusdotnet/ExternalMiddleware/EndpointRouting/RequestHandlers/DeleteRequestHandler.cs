#if endpointrouting
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;
using tusdotnet.ExternalMiddleware.EndpointRouting.Validation;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers
{
    /* 
    * When receiving a DELETE request for an existing upload the Server SHOULD free associated resources and MUST 
    * respond with the 204 No Content status confirming that the upload was terminated. 
    * For all future requests to this URL the Server SHOULD respond with the 404 Not Found or 410 Gone status.
    */

    internal class DeleteRequestHandler : RequestHandler
    {
        internal override RequestRequirement[] Requires => new RequestRequirement[]
        {

        };

        internal DeleteRequestHandler(HttpContext context, TusControllerBase controller, TusExtensionInfo extensionInfo, ITusEndpointOptions options)
            : base(context, controller, extensionInfo, options)
        {

        }

        internal override async Task<IActionResult> Invoke()
        {
            if (!await _controller.AuthorizeForAction(nameof(_controller.GetFileInfo)))
                return new ForbidResult();

            var fileId = (string)_context.GetRouteValue("TusFileId");

            SetTusResumableHeader();

            var deleteContext = new DeleteContext()
            {
                FileId = fileId,
            };

            IDeleteResult deleteResult;
            try
            {
                deleteResult = await _controller.Delete(deleteContext);
            }
            catch (TusException ex)
            {
                return new ObjectResult(ex.Message)
                {
                    StatusCode = (int)ex.StatusCode
                };
            }

            if (deleteResult is TusBadRequest fail) return new BadRequestObjectResult(fail.Message);
            if (deleteResult is TusForbidden) return new ForbidResult();
            

            var deleteOk = deleteResult as TusDeleteOk;

            if (deleteOk == null)
            {
                throw new InvalidOperationException($"Unknown action result: {deleteResult.GetType().FullName}");
            }

            return new NoContentResult();
        }
    }
}
#endif
