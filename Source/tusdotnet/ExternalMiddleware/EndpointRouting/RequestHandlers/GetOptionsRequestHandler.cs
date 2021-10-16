#if endpointrouting
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
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
    internal class GetOptionsRequestHandler : RequestHandler
    {
        internal GetOptionsRequestHandler(HttpContext context, TusControllerBase controller, TusEndpointOptions options)
            : base(context, controller, options)
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

            var capabilities = await _controller.GetCapabilities();
            if (capabilities.SupportedExtensions.Count > 0)
            {
                _context.Response.Headers.Add(HeaderConstants.TusExtension, string.Join(",", capabilities.SupportedExtensions));
            }

            if (capabilities.SupportedChecksumAlgorithms.Count > 0)
            {
                _context.Response.Headers.Add(HeaderConstants.TusChecksumAlgorithm, string.Join(",", capabilities.SupportedChecksumAlgorithms));
            }

            return new NoContentResult();
        }
    }
}
#endif
