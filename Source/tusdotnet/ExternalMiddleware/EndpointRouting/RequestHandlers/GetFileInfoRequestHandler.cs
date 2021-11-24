#if endpointrouting
using System.Threading.Tasks;
using tusdotnet.ExternalMiddleware.EndpointRouting.Validation;

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
        private readonly string _fileId;

        internal override RequestRequirement[] Requires => new RequestRequirement[] { };

        internal GetFileInfoRequestHandler(TusContext context, TusControllerBase controller, string fileId)
            : base(context, controller)
        {
            _fileId = fileId;
        }

        internal override async Task<ITusActionResult> Invoke()
        {
            var getInfoContext = new GetFileInfoContext()
            {
                FileId = _fileId,
            };

            try
            {
                return await _controller.GetFileInfo(getInfoContext);
            }
            catch (TusException ex)
            {
                return new TusStatusCodeResult(ex.StatusCode, ex.Message);
            }
        }
    }
}
#endif
