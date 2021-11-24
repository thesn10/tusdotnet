#if endpointrouting
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.ExternalMiddleware.EndpointRouting.Validation;
using tusdotnet.Models;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers
{
    internal abstract class RequestHandler
    {
        protected readonly TusContext _context;
        protected readonly TusControllerBase _controller;

        protected HttpContext HttpContext => _context.HttpContext;
        protected TusExtensionInfo ExtensionInfo => _context.ExtensionInfo;
        protected ITusEndpointOptions Options => _context.Options;
        protected string UrlPath => _context.UrlPath;

        internal abstract RequestRequirement[] Requires { get; }

        internal RequestHandler(TusContext context, TusControllerBase controller)
        {
            _context = context;
            _controller = controller;
        }

        internal abstract Task<ITusActionResult> Invoke();

        internal static void SetCommonHeaders(HttpContext context, DateTimeOffset? expires, long? uploadOffset)
        {
            if (expires != null)
            {
                context.Response.Headers.Add(HeaderConstants.UploadExpires, expires.Value.ToString("R"));
            }

            if (uploadOffset != null)
            {
                context.Response.Headers.Add(HeaderConstants.UploadOffset, uploadOffset.Value.ToString());
            }
        }

        internal static void SetTusResumableHeader(HttpContext context)
        {
            context.Response.Headers.Add(HeaderConstants.TusResumable, HeaderConstants.TusResumableValue);
        }

        internal static void SetCacheNoStoreHeader(HttpContext context)
        {
            context.Response.Headers.Add(HeaderConstants.CacheControl, HeaderConstants.NoStore);
        }

        internal static RequestHandler GetInstance(IntentType intentType, TusContext context, TusControllerBase controller, string fileId)
        {
            switch (intentType)
            {
                case IntentType.CreateFile:
                    return new CreateRequestHandler(context, controller);
                case IntentType.WriteFile:
                    return new WriteRequestHandler(context, controller, fileId);
                case IntentType.DeleteFile:
                    return new DeleteRequestHandler(context, controller, fileId);
                case IntentType.GetFileInfo:
                    return new GetFileInfoRequestHandler(context, controller, fileId);
                case IntentType.GetOptions:
                    return new GetOptionsRequestHandler(context, controller, fileId);
                case IntentType.ConcatenateFiles:
                    return new ConcatenateRequestHandler(context, controller);
                default:
                    return null;
            }
        }
    }
}
#endif