using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using tusdotnet.Controllers;
using tusdotnet.Models;
using tusdotnet.RequestHandlers.Tus2;
using tusdotnet.RequestHandlers.Validation;
using tusdotnet.Routing;

namespace tusdotnet.RequestHandlers
{
    internal abstract class RequestHandlerV2 : IRequestHandler
    {
        protected readonly TusContext _context;
        protected readonly Tus2ControllerBase _controller;

        protected HttpContext HttpContext => _context.HttpContext;
        protected FeatureSupportContext FeatureSupportContext => _context.FeatureSupportContext;
        protected ITusEndpointOptions EndpointOptions => _context.EndpointOptions;
        protected ITusRoutingHelper RoutingHelper => _context.RoutingHelper;

        public abstract RequestRequirement[] Requires { get; }

        internal RequestHandlerV2(TusContext context, Tus2ControllerBase controller)
        {
            _context = context;
            _controller = controller;
        }

        public abstract Task<ITusActionResult> Invoke();

        internal static RequestHandlerV2 GetInstance(IntentType intentType, TusContext context, Tus2ControllerBase controller)
        {
            switch (intentType)
            {
                case IntentType.V2UploadCreationProcedure:
                    return new UploadCreationRequestHandler(context, controller);
                case IntentType.V2OffsetRetrievingProcedure:
                    return new RetrieveOffsetRequestHandler(context, controller);
                case IntentType.V2UploadAppendingProcedure:
                    //return new DeleteRequestHandler(context, controller);
                case IntentType.V2UploadCancellationProcedure:
                    //return new DeleteRequestHandler(context, controller);
                default:
                    return null;
            }
        }
    }
}