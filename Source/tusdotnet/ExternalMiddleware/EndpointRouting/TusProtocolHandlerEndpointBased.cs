#if endpointrouting

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.Extensions;
using tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers;
using tusdotnet.ExternalMiddleware.EndpointRouting.Validation;
using tusdotnet.Models;
using tusdotnet.Routing;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    internal class TusProtocolHandlerEndpointBased<TController> : TusProtocolHandlerEndpointBased
        where TController : TusControllerBase
    {
        internal TusProtocolHandlerEndpointBased(ITusEndpointOptions options)
            : base(options)
        {
        }

        internal new Task Invoke(HttpContext context)
        {
            Controller = context.RequestServices.GetRequiredService<TController>();

            StorageClientProvider = context.RequestServices.GetRequiredService<ITusStorageClientProvider>();

            RoutingHelperFactory = context.RequestServices.GetRequiredService<ITusRoutingHelperFactory>();

            return base.Invoke(context);
        }
    }

    internal class TusProtocolHandlerEndpointBased<TController, TControllerOptions> : TusProtocolHandlerEndpointBased
        where TController : TusControllerBase, IControllerWithOptions<TControllerOptions>
    {
        private readonly TControllerOptions _controllerOptions;

        internal TusProtocolHandlerEndpointBased(ITusEndpointOptions options, TControllerOptions controllerOptions) 
            : base(options)
        {
            _controllerOptions = controllerOptions;
        }

        internal new Task Invoke(HttpContext context)
        {
            Controller = context.RequestServices.GetRequiredService<TController>();

            if (Controller is IControllerWithOptions<TControllerOptions> controllerWithOptions)
            {
                controllerWithOptions.Options = _controllerOptions;
            }

            StorageClientProvider = context.RequestServices.GetRequiredService<ITusStorageClientProvider>();

            RoutingHelperFactory = context.RequestServices.GetRequiredService<ITusRoutingHelperFactory>();

            return base.Invoke(context);
        }
    }

    internal class TusProtocolHandlerEndpointBased
    {
        private readonly ITusEndpointOptions _options;

        internal TusProtocolHandlerEndpointBased(ITusEndpointOptions options)
        {
            _options = options;
        }

        public ITusRoutingHelperFactory RoutingHelperFactory { get; set; }
        public ITusStorageClientProvider StorageClientProvider { get; set; }
        public TusControllerBase Controller { get; set; }

        public RequestDelegate Next { get; set; }

        internal async Task Invoke(HttpContext context)
        {
            ApplyControllerAttributeOptions(Controller.GetType());

            // the routing helper handles url generation for endpoint routing (or url path routing)
            var routingHelper = RoutingHelperFactory.Get(context);

            var tusContext = new TusContext()
            {
                HttpContext = context,
                Options = _options,
                RoutingHelper = routingHelper,
            };

            // Inject services into the controller
            Controller.TusContext = tusContext;
            Controller.StorageClientProvider = StorageClientProvider;
            Controller.StorageClient = await StorageClientProvider.GetOrNull(_options.StorageProfile);

            tusContext.ExtensionInfo = await Controller.GetOptions();
            Controller.TusContext.ExtensionInfo = tusContext.ExtensionInfo;

            var intentType = IntentAnalyzer.DetermineIntent(tusContext);
            if (intentType == IntentType.NotApplicable)
            {
                if (Next != null)
                {
                    await Next(context);
                    return;
                }
                else
                {
                    context.Response.StatusCode = 404;
                    return;
                }
            }

            var versionResult = await VerifyTusVersionIfApplicable(context, intentType);

            if (!versionResult.IsSuccessResult)
            {
                await versionResult.Execute(tusContext);
                return;
            }

            var fileId = routingHelper.GetFileId();

            RequestHandler requestHandler = RequestHandler.GetInstance(intentType, tusContext, Controller, fileId);
            RequestValidator requestValidator = new RequestValidator(requestHandler.Requires);

            var authorizeContext = new AuthorizeContext()
            {
                IntentType = intentType,
                ControllerMethod = AuthorizeContext.GetControllerActionMethodInfo(intentType, Controller),
                FileId = fileId,
                RequestHandler = requestHandler,
            };

            var authorizeResult = await Controller.Authorize(authorizeContext);

            if (!authorizeResult.IsSuccessResult)
            {
                await authorizeResult.Execute(tusContext);
                return;
            }

            var result = await requestValidator.Validate(tusContext);

            if (result.IsSuccessResult)
            {
                result = await requestHandler.Invoke();
            }

            await result.Execute(tusContext);
        }

        private void ApplyControllerAttributeOptions(Type controllerType)
        {
            TusStorageProfileAttribute storageProfileAttr = controllerType.GetCustomAttribute<TusStorageProfileAttribute>();
            if (storageProfileAttr != null)
            {
                _options.StorageProfile = storageProfileAttr.ProfileName ?? "default";
            }

            TusMetadataParsingAttribute metadatParsingAttr = controllerType.GetCustomAttribute<TusMetadataParsingAttribute>();
            if (metadatParsingAttr != null)
            {
                _options.MetadataParsingStrategy = metadatParsingAttr.Strategy;
            }

            TusMaxUploadSizeAttribute maxUploadAttr = controllerType.GetCustomAttribute<TusMaxUploadSizeAttribute>();
            if (maxUploadAttr != null)
            {
                _options.MaxAllowedUploadSizeInBytes = maxUploadAttr.MaxUploadSizeInBytes;
            }
        }

        private static Task<ITusActionResult> VerifyTusVersionIfApplicable(HttpContext context, IntentType intent)
        {
            // Options does not require a correct tus resumable header.
            if (intent == IntentType.GetOptions)
                return Task.FromResult<ITusActionResult>(new TusOkResult());

            var tusResumableHeader = context.Request.GetHeader(HeaderConstants.TusResumable);

            if (tusResumableHeader == HeaderConstants.TusResumableValue)
                return Task.FromResult<ITusActionResult>(new TusOkResult());

            context.Response.Headers.Add(HeaderConstants.TusVersion, HeaderConstants.TusResumableValue);

            return Task.FromResult<ITusActionResult>(new TusStatusCodeResult(HttpStatusCode.PreconditionFailed, 
                $"Tus version {tusResumableHeader} is not supported. Supported versions: {HeaderConstants.TusResumableValue}"));
        }
    }
}

#endif