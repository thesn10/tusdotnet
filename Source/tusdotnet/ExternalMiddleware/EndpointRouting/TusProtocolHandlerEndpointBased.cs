#if endpointrouting

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using tusdotnet.Adapters;
using tusdotnet.Constants;
using tusdotnet.ExternalMiddleware.Core;
using tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers;
using tusdotnet.Models;
using tusdotnet.Models.Concatenation;
using tusdotnet.Parsers;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    internal class TusProtocolHandlerEndpointBased<TController> : TusProtocolHandlerEndpointBased
        where TController : TusControllerBase
    {
        internal TusProtocolHandlerEndpointBased(TusEndpointOptions options)
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
        where TController : TusControllerBase
    {
        private readonly TControllerOptions _controllerOptions;

        internal TusProtocolHandlerEndpointBased(TusEndpointOptions options, TControllerOptions controllerOptions) 
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
        private readonly TusEndpointOptions _options;

        internal TusProtocolHandlerEndpointBased(TusEndpointOptions options)
        {
            _options = options;
        }


        internal async Task Invoke(HttpContext context, TusControllerBase controller)
        {
            // Inject HttpContext into the controller
            controller.HttpContext = context;

            var contextAdapter = CreateFakeContextAdapter(context);
            var responseStream = new MemoryStream();
            var responseHeaders = new Dictionary<string, string>();
            HttpStatusCode? responseStatus = null;
            contextAdapter.Response = new ResponseAdapter
            {
                Body = responseStream,
                SetHeader = (key, value) => responseHeaders[key] = value,
                SetStatus = status => responseStatus = status
            };

            var controllerCapabilities = await controller.GetCapabilities();
            var intentType = IntentAnalyzer.DetermineIntent(contextAdapter, controllerCapabilities.SupportedExtensions);

            if (intentType == IntentType.NotApplicable)
            {
                // Cannot determine intent so return not found.
                context.Response.StatusCode = 404;
                return;
            }

            // TODO:
            // 1. Seperate request validation from file storage validation
            // 2. Validate request here
            // 3. Validate file storage in StorageService

            //var valid = await intentHandler.Validate();

            /*if (!valid)
            {
                // TODO: Optimize as there is not much worth in writing to a stream and then piping it to the response.
                context.Response.StatusCode = (int)responseStatus.Value;
                responseStream.Seek(0, SeekOrigin.Begin);
                await context.Response.BodyWriter.WriteAsync(responseStream.GetBuffer(), context.RequestAborted);

                return;
            }*/

            IActionResult result = null;

            switch (intentType)
            {
                case IntentType.CreateFile:
                    result = await new CreateRequestHandler(context, controller, _options).Invoke();
                    break;
                case IntentType.WriteFile:
                    result = await new WriteRequestHandler(context, controller, _options).Invoke();
                    break;
                case IntentType.GetFileInfo:
                    result = await new GetFileInfoRequestHandler(context, controller, _options).Invoke();
                    break;
                case IntentType.GetOptions:
                    result = await new GetOptionsRequestHandler(context, controller, _options).Invoke();
                    break;
            }

            await context.Respond(result, null);
        }

        private ContextAdapter CreateFakeContextAdapter(HttpContext context)
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

            var config = new DefaultTusConfiguration
            {
                UrlPath = urlPath
            };

            var adapter = ContextAdapterBuilder.FromHttpContext(context, config);

            return adapter;
        }
    }
}

#endif