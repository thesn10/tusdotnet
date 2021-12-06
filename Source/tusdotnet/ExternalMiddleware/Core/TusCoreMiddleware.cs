﻿using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using tusdotnet.Adapters;
using tusdotnet.Models;
using tusdotnet.Routing;

#if endpointrouting
using tusdotnet.ExternalMiddleware.EndpointRouting;
#endif

// ReSharper disable once CheckNamespace
namespace tusdotnet
{
    /// <summary>
    /// Processes tus.io requests for ASP.NET Core.
    /// </summary>
    public class TusCoreMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly Func<HttpContext, Task<DefaultTusConfiguration>> _configFactory;

        /// <summary>Creates a new instance of TusCoreMiddleware.</summary>
        /// <param name="next"></param>
        /// <param name="configFactory"></param>
        public TusCoreMiddleware(RequestDelegate next, Func<HttpContext, Task<DefaultTusConfiguration>> configFactory)
        {
            _next = next;
            _configFactory = configFactory;
        }

        /// <summary>
        /// Handles the tus.io request.
        /// </summary>
        /// <param name="context">The HttpContext</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            var config = await _configFactory(context);

            if (config == null)
            {
                await _next(context);
                return;
            }

            var requestUri = GetRequestUri(context);

            if (!TusProtocolHandlerIntentBased.RequestIsForTusEndpoint(requestUri, config))
            {
                await _next(context);
                return;
            }

#if endpointrouting

            config.Validate();

            var options = new TusSimpleEndpointOptions()
            {
                Events = config.Events,
                Expiration = config.Expiration,
                MaxAllowedUploadSizeInBytes = config.GetMaxAllowedUploadSizeInBytes(),
                MetadataParsingStrategy = config.MetadataParsingStrategy,
                UsePipelinesIfAvailable = config.UsePipelinesIfAvailable,
                FileLockProvider = config.FileLockProvider,
                MockedTime = config.MockedTime,

#pragma warning disable CS0618 // Type or member is obsolete
                OnUploadCompleteAsync = config.OnUploadCompleteAsync,
#pragma warning restore CS0618 // Type or member is obsolete
            };

            var storageClientProvider = new SingleStorageClientProvider(config.Store);
            var routingHelperFactory = new TusUrlPathRoutingHelperFactory(config.UrlPath);

            var controller = new EventsBasedTusController()
            {
                Options = options,
            };

            var handler = new TusProtocolHandlerEndpointBased(options)
            {
                Controller = controller,
                StorageClientProvider = storageClientProvider,
                RoutingHelperFactory = routingHelperFactory,
                Next = _next,
            };

            await handler.Invoke(context);

#else
            var request = CreateRequestAdapter(context, config, requestUri);
            var response = CreateResponseAdapter(context);

            var handled = await TusProtocolHandlerIntentBased.Invoke(new ContextAdapter
            {
                Request = request,
                Response = response,
                Configuration = config,
                CancellationToken = context.RequestAborted,
                HttpContext = context
            });

            if (handled == ResultType.ContinueExecution)
            {
                await _next(context);
            }
#endif
        }

        private RequestAdapter CreateRequestAdapter(HttpContext context, DefaultTusConfiguration config, Uri requestUri)
        {
            return new RequestAdapter(config.UrlPath)
            {
                Headers =
                    context.Request.Headers.ToDictionary(
                        f => f.Key,
                        f => f.Value.ToList(),
                        StringComparer.OrdinalIgnoreCase),
                Body = context.Request.Body,
#if pipelines
                BodyReader = context.Request.BodyReader,
#endif
                Method = context.Request.Method,
                RequestUri = requestUri
            };
        }

        private static Uri GetRequestUri(HttpContext context)
        {
            return new Uri($"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}{context.Request.Path}");
        }

        private ResponseAdapter CreateResponseAdapter(HttpContext context)
        {
            return new ResponseAdapter
            {
                Body = context.Response.Body,
                SetHeader = (key, value) => context.Response.Headers[key] = value,
                SetStatus = status => context.Response.StatusCode = (int)status
            };
        }
    }
}