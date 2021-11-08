#if endpointrouting
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.ExternalMiddleware.EndpointRouting.Validation;
using tusdotnet.Models;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers
{
    internal abstract class RequestHandler
    {
        protected readonly HttpContext _context;
        protected readonly TusControllerBase _controller;
        protected readonly TusExtensionInfo _extensionInfo;
        protected readonly ITusEndpointOptions _options;

        internal abstract RequestRequirement[] Requires { get; }

        internal RequestHandler(HttpContext context, TusControllerBase controller, TusExtensionInfo extensionInfo, ITusEndpointOptions options)
        {
            _context = context;
            _controller = controller;
            _extensionInfo = extensionInfo;
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

        internal static RequestHandler GetInstance(IntentType intentType, HttpContext context, TusControllerBase controller, 
            TusExtensionInfo extensionInfo, ITusEndpointOptions options)
        {
            switch (intentType)
            {
                case IntentType.CreateFile:
                    return new CreateRequestHandler(context, controller, extensionInfo, options);
                case IntentType.WriteFile:
                    return new WriteRequestHandler(context, controller, extensionInfo, options);
                case IntentType.DeleteFile:
                    return new DeleteRequestHandler(context, controller, extensionInfo, options);
                case IntentType.GetFileInfo:
                    return new GetFileInfoRequestHandler(context, controller, extensionInfo, options);
                case IntentType.GetOptions:
                    return new GetOptionsRequestHandler(context, controller, extensionInfo, options);
                default:
                    return null;
            }
        }
    }
}
#endif