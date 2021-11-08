#if endpointrouting

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Threading.Tasks;
using tusdotnet.ExternalMiddleware.Core;
using tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers;
using tusdotnet.ExternalMiddleware.EndpointRouting.Validation;
using tusdotnet.Models;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    internal class TusProtocolHandlerEndpointBased<TController> : TusProtocolHandlerEndpointBased
        where TController : TusControllerBase
    {
        internal TusProtocolHandlerEndpointBased(ITusEndpointOptions options)
            : base(options)
        {
        }

        internal Task Invoke(HttpContext context)
        {
            var controller = (TusControllerBase)context.RequestServices.GetRequiredService<TController>();
            return Invoke(context, controller);
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

        internal Task Invoke(HttpContext context)
        {
            var controller = (TusControllerBase)context.RequestServices.GetRequiredService<TController>();

            if (controller is IControllerWithOptions<TControllerOptions> controllerWithOptions)
            {
                controllerWithOptions.Options = _controllerOptions;
            }

            return Invoke(context, controller);
        }
    }

    internal abstract class TusProtocolHandlerEndpointBased
    {
        private readonly ITusEndpointOptions _options;

        internal TusProtocolHandlerEndpointBased(ITusEndpointOptions options)
        {
            _options = options;
        }


        internal async Task Invoke(HttpContext context, TusControllerBase controller)
        {
            var storageClientProvider = context.RequestServices.GetRequiredService<ITusStorageClientProvider>();
            var profileName = controller.GetType().GetCustomAttribute<TusStorageProfileAttribute>()?.ProfileName ?? "default";

            // Inject services into the controller
            controller.HttpContext = context;
            controller.StorageClientProvider = storageClientProvider;
            controller.StorageClient = await storageClientProvider.GetOrNull(profileName);

            var contextAdapter = ContextAdapterBuilder.CreateFakeContextAdapter(context, new DefaultTusConfiguration() { });
            var extensionInfo = await controller.GetOptions();
            var intentType = IntentAnalyzer.DetermineIntent(contextAdapter, extensionInfo.SupportedExtensions);

            if (intentType == IntentType.NotApplicable)
            {
                // Cannot determine intent so return not found.
                context.Response.StatusCode = 404;
                return;
            }

            RequestHandler requestHandler = RequestHandler.GetInstance(intentType, context, controller, extensionInfo, _options);
            RequestValidator requestValidator = new RequestValidator(extensionInfo, requestHandler.Requires);

            var valid = await requestValidator.Validate(context);

            if (!valid)
            {
                await RespondWithValidationError(context, requestValidator);
                return;
            }

            var result = await requestHandler.Invoke();

            await context.Respond(result, null);
        }

        internal async Task RespondWithValidationError(HttpContext context, RequestValidator validator)
        {
            if (validator.ErrorMessage != null)
            {
                var result = new ObjectResult(validator.ErrorMessage);
                result.StatusCode = (int)validator.StatusCode;
                await context.Respond(result, null);
            }
            else
            {
                var result = new StatusCodeResult((int)validator.StatusCode);
                await context.Respond(result, null);
            }
        }
    }
}

#endif