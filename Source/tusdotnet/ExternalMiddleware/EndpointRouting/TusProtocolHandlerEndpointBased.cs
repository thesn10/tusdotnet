#if endpointrouting

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.Controllers;
using tusdotnet.Extensions;
using tusdotnet.Models;
using tusdotnet.RequestHandlers;
using tusdotnet.RequestHandlers.Validation;
using tusdotnet.Routing;
using tusdotnet.Storage;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    internal class TusProtocolHandlerEndpointBasedDI : TusProtocolHandlerEndpointBased
    {
        internal TusProtocolHandlerEndpointBasedDI(ITusEndpointOptions options)
            : base(options)
        {
        }

        internal new Task Invoke(HttpContext context)
        {
            ControllerFactory = context.RequestServices.GetRequiredService<IControllerFactory>();

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
        public ITusStorageClientProvider? StorageClientProvider { get; set; }
        /// <summary>
        /// Setting this to null will disable tus v1 support
        /// </summary>
        //public TusControllerBase? V1Controller { get; set; }
        /// <summary>
        /// Setting this to null will disable tus v2 support
        /// </summary>
        //public Tus2ControllerBase? V2Controller { get; set; }

        public IControllerFactory ControllerFactory { get; set; }

        public RequestDelegate? Next { get; set; }

        internal async Task Invoke(HttpContext context)
        {
            var v1Controller = ControllerFactory.CreateController(context);
            var v2Controller = ControllerFactory.CreateV2Controller(context);

            // the routing helper handles url generation for endpoint routing (or url path routing)
            var routingHelper = RoutingHelperFactory.Get(context);

            // contains all context information of the current tus request
            var tusContext = new TusContext()
            {
                HttpContext = context,
                EndpointOptions = _options,
                RoutingHelper = routingHelper,
            };

            var intentType = IntentAnalyzer.DetermineIntent(tusContext);
            if (intentType == IntentType.NotApplicable)
            {
                if (Next is not null)
                {
                    // maintain middleware compatibility
                    await Next(context);
                    return;
                }
                
                // default endpoint routing behaivior
                context.Response.StatusCode = 404;
                return;
            }

            var versionResult = await VerifyTusVersionIfApplicable(context, intentType);

            if (!versionResult.IsSuccessResult)
            {
                await versionResult.Execute(tusContext);
                return;
            }

            //TODO check version

            IRequestHandler? requestHandler = null;

            if (v1Controller is not null)
            {
                ApplyControllerAttributeOptions(v1Controller.GetType());
                
                // Inject services into the controller
                v1Controller.TusContext = tusContext;
                v1Controller.StorageClientProvider = StorageClientProvider;
                v1Controller.StorageClient = await StorageClientProvider?.GetOrNull(_options.StorageProfile);

                tusContext.FeatureSupportContext = await v1Controller.GetOptions();
                v1Controller.TusContext.FeatureSupportContext = tusContext.FeatureSupportContext;

                requestHandler = RequestHandler.GetInstance(intentType, tusContext, v1Controller);
            }

            if (v2Controller is not null)
            {
                requestHandler = RequestHandlerV2.GetInstance(intentType, tusContext, v2Controller);
            }

            //var fileId = routingHelper.GetFileId();

            RequestValidator requestValidator = new RequestValidator(requestHandler.Requires);

            /*var authorizeContext = new AuthorizeContext()
            {
                IntentType = intentType,
                Controller = Controller,
                FileId = null, // TODO
                RequestHandler = requestHandler,
            };

            var authorizeResult = await V1Controller.Authorize(authorizeContext);*/

            var authorizeResult = await Authorize(context, GetMethodToAuthorize(v1Controller, intentType));

            /*if (!authorizeResult.IsSuccessResult)
            {
                await authorizeResult.Execute(tusContext);
                return;
            }*/

            if (!authorizeResult.Succeeded)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
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
            StorageProfileAttribute storageProfileAttr = controllerType.GetCustomAttribute<StorageProfileAttribute>();
            if (storageProfileAttr != null)
            {
                _options.StorageProfile = storageProfileAttr.ProfileName ?? "default";
            }

            MetadataParsingAttribute metadatParsingAttr = controllerType.GetCustomAttribute<MetadataParsingAttribute>();
            if (metadatParsingAttr != null)
            {
                _options.MetadataParsingStrategy = metadatParsingAttr.Strategy;
            }

            MaxUploadSizeAttribute maxUploadAttr = controllerType.GetCustomAttribute<MaxUploadSizeAttribute>();
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

            return Task.FromResult<ITusActionResult>(new TusBaseResult(HttpStatusCode.PreconditionFailed,
                $"Tus version {tusResumableHeader} is not supported. Supported versions: {HeaderConstants.TusResumableValue}"));
        }

        private async Task<AuthorizationResult?> Authorize(HttpContext context, MethodInfo methodToAuthorize)
        {
            var authService = context.RequestServices.GetService<IAuthorizationService>();
            if (authService is null)
            {
                return null;
            }

            var authorizeAttribute = methodToAuthorize.GetCustomAttribute<AuthorizeAttribute>();
            if (authorizeAttribute is null)
            {
                return null;
            }

            return await authService.AuthorizeAsync(context.User, authorizeAttribute.Policy);
        }

        /// <summary>
        /// Gets the method to authorize. Useful for reading method attributes
        /// </summary>
        public MethodInfo GetMethodToAuthorize(TusControllerBase v1Controller, IntentType intentType)
        {
#if NETCOREAPP2_0_OR_GREATER
            return intentType switch
            {
                IntentType.WriteFile => ((Func<WriteContext, Task<IWriteResult>>)v1Controller.Write).Method,
                IntentType.CreateFile => ((Func<CreateContext, Task<ICreateResult>>)v1Controller.Create).Method,
                IntentType.ConcatenateFiles => ((Func<CreateContext, Task<ICreateResult>>)v1Controller.Create).Method,
                IntentType.DeleteFile => ((Func<DeleteContext, Task<ISimpleResult>>)v1Controller.Delete).Method,
                IntentType.GetFileInfo => ((Func<GetFileInfoContext, Task<IFileInfoResult>>)v1Controller.GetFileInfo).Method,
                IntentType.GetOptions => ((Func<Task<FeatureSupportContext>>)v1Controller.GetOptions).Method,
                _ => throw new ArgumentException(),
            };
#else
            return intentType switch
            {
                IntentType.WriteFile => Controller.GetType().GetMethod(nameof(Controller.Write)),
                IntentType.CreateFile => Controller.GetType().GetMethod(nameof(Controller.Create)),
                IntentType.ConcatenateFiles => Controller.GetType().GetMethod(nameof(Controller.Create)),
                IntentType.DeleteFile => Controller.GetType().GetMethod(nameof(Controller.Delete)),
                IntentType.GetFileInfo => Controller.GetType().GetMethod(nameof(Controller.GetFileInfo)),
                IntentType.GetOptions => Controller.GetType().GetMethod(nameof(Controller.GetOptions)),
                _ => throw new ArgumentException(),
            };
#endif
        }
    }
}

#endif