using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using tusdotnet.Controllers.Factory;
using tusdotnet.Models;

using tusdotnet.ExternalMiddleware.EndpointRouting;
using tusdotnet.Routing;
using tusdotnet.Storage;

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

            if (!RequestIsForTusEndpoint(requestUri, config))
            {
                await _next(context);
                return;
            }

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

            var controllerFactory = new EventsControllerFactory(options);

            var handler = new TusProtocolHandlerEndpointBased(options)
            {
                StorageClientProvider = storageClientProvider,
                RoutingHelperFactory = routingHelperFactory,
                ControllerFactory = controllerFactory,
                Next = _next,
            };

            await handler.Invoke(context);
        }

        private static Uri GetRequestUri(HttpContext context)
        {
            return new Uri($"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}{context.Request.Path}");
        }
        
        private static bool RequestIsForTusEndpoint(Uri requestUri, DefaultTusConfiguration configuration)
        {
            return requestUri.LocalPath.StartsWith(configuration.UrlPath, StringComparison.OrdinalIgnoreCase);
        }
    }
}