using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.ExternalMiddleware.EndpointRouting.Validation;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers
{
    /*
    * An OPTIONS request MAY be used to gather information about the Server’s current configuration. 
    * A successful response indicated by the 204 No Content status MUST contain the Tus-Version header. 
    * It MAY include the Tus-Extension and Tus-Max-Size headers.
    * The Client SHOULD NOT include the Tus-Resumable header in the request and the Server MUST discard it.
    */

    internal class GetOptionsRequestHandler : RequestHandler
    {
        private readonly string _fileId;

        internal override RequestRequirement[] Requires => new RequestRequirement[] { };

        internal GetOptionsRequestHandler(TusContext context, TusControllerBase controller, string fileId)
            : base(context, controller)
        {
            _fileId = fileId;
        }

        internal override async Task<ITusActionResult> Invoke()
        {
            HttpContext.Response.Headers.Add(HeaderConstants.TusVersion, HeaderConstants.TusResumableValue);

            var maximumAllowedSize = Options.MaxAllowedUploadSizeInBytes;

            if (maximumAllowedSize.HasValue)
            {
                HttpContext.Response.Headers.Add(HeaderConstants.TusMaxSize, maximumAllowedSize.Value.ToString());
            }

            if (ExtensionInfo.SupportedExtensions.Any())
            {
                HttpContext.Response.Headers.Add(HeaderConstants.TusExtension, string.Join(",", ExtensionInfo.SupportedExtensions.ToList()));
            }

            var supportedChecksumAlgorithms = await ExtensionInfo.GetSupportedChecksumAlgorithms(HttpContext.RequestAborted);
            if (supportedChecksumAlgorithms.Any())
            {
                HttpContext.Response.Headers.Add(HeaderConstants.TusChecksumAlgorithm, string.Join(",", supportedChecksumAlgorithms));
            }

            return new TusOkResult();
        }
    }
}
