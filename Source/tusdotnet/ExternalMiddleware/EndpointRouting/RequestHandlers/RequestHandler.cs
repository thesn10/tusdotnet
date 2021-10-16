#if endpointrouting
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tusdotnet.Adapters;
using tusdotnet.Constants;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers
{
    internal abstract class RequestHandler
    {
        protected readonly HttpContext _context;
        protected readonly TusControllerBase _controller;
        protected readonly TusEndpointOptions _options;

        internal RequestHandler(HttpContext context, TusControllerBase controller, TusEndpointOptions options)
        {
            _context = context;
            _controller = controller;
            _options = options;
        }

        internal abstract Task<IActionResult> Invoke();


        protected void SetCreateHeaders(DateTimeOffset? expires, long? uploadOffset)
        {
            var result = new Dictionary<string, string>();
            if (expires != null)
            {
                _context.Response.Headers.Add(HeaderConstants.UploadExpires, expires.Value.ToString("R"));
            }

            if (uploadOffset != null)
            {
                _context.Response.Headers.Add(HeaderConstants.UploadOffset, uploadOffset.Value.ToString());
            }
        }

        protected void SetTusResumableHeader()
        {
            _context.Response.Headers.Add(HeaderConstants.TusResumable, HeaderConstants.TusResumableValue);
        }

        protected void SetCacheNoStoreHeader()
        {
            _context.Response.Headers.Add(HeaderConstants.CacheControl, HeaderConstants.NoStore);
        }
    }
}
#endif