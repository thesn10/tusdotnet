using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using tusdotnet.Controllers;
using tusdotnet.Models;
using tusdotnet.RequestHandlers.Validation;
using tusdotnet.Routing;

namespace tusdotnet.RequestHandlers
{
    internal abstract class RequestHandler : IRequestHandler
    {
        protected readonly TusContext _context;
        protected readonly TusControllerBase _controller;

        protected HttpContext HttpContext => _context.HttpContext;
        protected FeatureSupportContext FeatureSupportContext => _context.FeatureSupportContext;
        protected ITusEndpointOptions EndpointOptions => _context.EndpointOptions;
        protected ITusRoutingHelper RoutingHelper => _context.RoutingHelper;

        public abstract RequestRequirement[] Requires { get; }

        internal RequestHandler(TusContext context, TusControllerBase controller)
        {
            _context = context;
            _controller = controller;
        }

        public abstract Task<ITusActionResult> Invoke();

        internal static RequestHandler GetInstance(IntentType intentType, TusContext context, TusControllerBase controller)
        {
            switch (intentType)
            {
                case IntentType.CreateFile:
                    return new CreateRequestHandler(context, controller);
                case IntentType.WriteFile:
                    return new WriteRequestHandler(context, controller);
                case IntentType.DeleteFile:
                    return new DeleteRequestHandler(context, controller);
                case IntentType.GetFileInfo:
                    return new GetFileInfoRequestHandler(context, controller);
                case IntentType.GetOptions:
                    return new GetOptionsRequestHandler(context, controller);
                case IntentType.ConcatenateFiles:
                    return new ConcatenateRequestHandler(context, controller);
                default:
                    return null;
            }
        }
    }
}