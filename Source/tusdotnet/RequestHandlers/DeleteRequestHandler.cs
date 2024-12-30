using System.Threading.Tasks;
using tusdotnet.Controllers;
using tusdotnet.Exceptions;
using tusdotnet.RequestHandlers.Validation;
using tusdotnet.Routing;

namespace tusdotnet.RequestHandlers
{
    /* 
    * When receiving a DELETE request for an existing upload the Server SHOULD free associated resources and MUST 
    * respond with the 204 No Content status confirming that the upload was terminated. 
    * For all future requests to this URL the Server SHOULD respond with the 404 Not Found or 410 Gone status.
    */

    internal class DeleteRequestHandler : RequestHandler
    {
        private readonly string _fileId;

        public override RequestRequirement[] Requires => new RequestRequirement[] { };

        internal DeleteRequestHandler(TusContext context, TusControllerBase controller)
            : base(context, controller)
        {
            _fileId = context.RoutingHelper.GetFileId();
        }

        public override async Task<ITusActionResult> Invoke()
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
                return new TusBaseResult(ex.StatusCode, ex.Message);
            }
        }
    }
}
