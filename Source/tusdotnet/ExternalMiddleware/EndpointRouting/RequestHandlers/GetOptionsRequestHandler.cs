#if endpointrouting
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        internal override RequestRequirement[] Requires => new RequestRequirement[]
        {

        };

        internal GetOptionsRequestHandler(HttpContext context, TusControllerBase controller, TusExtensionInfo extensionInfo, ITusEndpointOptions options)
            : base(context, controller, extensionInfo, options)
        {

        }

        internal override async Task<IActionResult> Invoke()
        {
            SetTusResumableHeader();

            _context.Response.Headers.Add(HeaderConstants.TusVersion, HeaderConstants.TusResumableValue);

            var maximumAllowedSize = _options.MaxAllowedUploadSizeInBytes;

            if (maximumAllowedSize.HasValue)
            {
                _context.Response.Headers.Add(HeaderConstants.TusMaxSize, maximumAllowedSize.Value.ToString());
            }

            if (_extensionInfo.SupportedExtensions.Any())
            {
                _context.Response.Headers.Add(HeaderConstants.TusExtension, string.Join(",", _extensionInfo.SupportedExtensions.ToList()));
            }

            if (_extensionInfo.SupportedChecksumAlgorithms.Count > 0)
            {
                _context.Response.Headers.Add(HeaderConstants.TusChecksumAlgorithm, string.Join(",", _extensionInfo.SupportedChecksumAlgorithms));
            }

            return new NoContentResult();
        }
    }
}
#endif
