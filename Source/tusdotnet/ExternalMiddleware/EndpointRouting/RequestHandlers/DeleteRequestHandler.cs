#if endpointrouting
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
        private readonly string _fileId;

        internal override RequestRequirement[] Requires => new RequestRequirement[] { };

        internal DeleteRequestHandler(TusContext context, TusControllerBase controller, string fileId)
            : base(context, controller)
        {
            _fileId = fileId;
        }

        internal override async Task<ITusActionResult> Invoke()
        {
            var deleteContext = new DeleteContext()
            {
                FileId = _fileId,
            };

            try
            {
                return await _controller.Delete(deleteContext);
            }
            catch (TusException ex)
            {
                return new TusStatusCodeResult(ex.StatusCode, ex.Message);
            }
        }
    }
}
#endif
