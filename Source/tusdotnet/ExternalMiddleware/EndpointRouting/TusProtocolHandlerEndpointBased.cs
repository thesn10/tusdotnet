#if endpointrouting

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using tusdotnet.Adapters;
using tusdotnet.Constants;
using tusdotnet.Extensions;
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

            var storageClientProvider = context.RequestServices.GetRequiredService<ITusStorageClientProvider>();

            return Invoke(context, storageClientProvider, controller);
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

            var storageClientProvider = context.RequestServices.GetRequiredService<ITusStorageClientProvider>();

            return Invoke(context, storageClientProvider, controller);
        }
    }

    internal class TusProtocolHandlerEndpointBased
    {
        private readonly ITusEndpointOptions _options;

        internal TusProtocolHandlerEndpointBased(ITusEndpointOptions options)
        {
            _options = options;
        }

        public string UrlPath { get; set; }

        public RequestDelegate Next { get; set; }

        internal async Task Invoke(HttpContext context, ITusStorageClientProvider storageClientProvider, TusControllerBase controller)
        {
            ApplyControllerAttributeOptions(controller.GetType());

            var tusContext = new TusContext()
            {
                HttpContext = context,
                Options = _options,
                UrlPath = UrlPath,
            };

            // Inject services into the controller
            controller.TusContext = tusContext;
            controller.StorageClientProvider = storageClientProvider;
            controller.StorageClient = await storageClientProvider.GetOrNull(_options.StorageProfile);

            tusContext.ExtensionInfo = await controller.GetOptions();
            controller.TusContext.ExtensionInfo = tusContext.ExtensionInfo;

            var contextAdapter = CreateContextAdapter(context);
            var intentType = IntentAnalyzer.DetermineIntent(contextAdapter, tusContext.ExtensionInfo.SupportedExtensions);
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

            var fileId = GetFileId(context);

            var authorizeContext = new AuthorizeContext()
            {
                IntentType = intentType,
                ControllerMethod = GetControllerActionMethodInfo(intentType, controller),
                FileId = fileId,
            };

            var authorizeResult = await controller.Authorize(authorizeContext);

            if (!authorizeResult.IsSuccessResult)
            {
                await authorizeResult.Execute(tusContext);
                return;
            }

            var versionResult = await VerifyTusVersionIfApplicable(context, intentType);

            if (!versionResult.IsSuccessResult)
            {
                await versionResult.Execute(tusContext);
                return;
            }

            RequestHandler requestHandler = RequestHandler.GetInstance(intentType, tusContext, controller, fileId);
            RequestValidator requestValidator = new RequestValidator(requestHandler.Requires);

            var result = await requestValidator.Validate(tusContext);

            if (result.IsSuccessResult)
            {
                result = await requestHandler.Invoke();
            }

            await result.Execute(tusContext);
        }

        private MethodInfo GetControllerActionMethodInfo(IntentType intent, TusControllerBase controller)
        {
            return intent switch
            {
                IntentType.WriteFile => ((Func<WriteContext, Task<IWriteResult>>)controller.Write).Method,
                IntentType.CreateFile => ((Func<CreateContext, Task<ICreateResult>>)controller.Create).Method,
                IntentType.ConcatenateFiles => ((Func<CreateContext, Task<ICreateResult>>)controller.Create).Method,
                IntentType.DeleteFile => ((Func<DeleteContext, Task<ISimpleResult>>)controller.Delete).Method,
                IntentType.GetFileInfo => ((Func<GetFileInfoContext, Task<IFileInfoResult>>)controller.GetFileInfo).Method,
                IntentType.GetOptions => ((Func<Task<TusExtensionInfo>>)controller.GetOptions).Method,
                _ => throw new ArgumentException(),
            };
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

        private ContextAdapter CreateContextAdapter(HttpContext context)
        {
            var config = new DefaultTusConfiguration();

            if (UrlPath != null)
            {
                config.UrlPath = UrlPath;
            }
            else 
            {
                var urlPath = (string)context.GetRouteValue("TusFileId");

                if (string.IsNullOrWhiteSpace(urlPath))
                {
                    urlPath = context.Request.Path;
                }
                else
                {
                    var span = context.Request.Path.ToString().TrimEnd('/').AsSpan();
                    urlPath = span.Slice(0, span.LastIndexOf('/')).ToString();
                }

                config.UrlPath = urlPath;
            }

            var adapter = ContextAdapterBuilder.FromHttpContext(context, config);

            return adapter;
        }

        protected string GetFileId(HttpContext context)
        {
            string fileId = (string)context.GetRouteValue("TusFileId");

            if (fileId == null)
            {
                var startIndex = context.Request.Path.Value.IndexOf(UrlPath, StringComparison.OrdinalIgnoreCase) + UrlPath.Length;

                fileId = context.Request.Path.Value.Substring(startIndex).Trim('/');

                if (string.IsNullOrWhiteSpace(fileId))
                {
                    fileId = null;
                }
            }
            return fileId;
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