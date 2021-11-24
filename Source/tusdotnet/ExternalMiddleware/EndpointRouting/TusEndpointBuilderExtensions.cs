#if endpointrouting

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public static class TusEndpointBuilderExtensions
    {
        /// <summary>
        /// Maps a tus controller to the specified pattern
        /// </summary>
        public static IEndpointConventionBuilder MapTusController<TController>(this IEndpointRouteBuilder endpoints, string pattern, Action<TusEndpointOptions> config = default)
            where TController : TusControllerBase
        {
            var options = new TusEndpointOptions();
            config?.Invoke(options);

            //var reqDelegate = _endpoints
            //    .CreateApplicationBuilder()
            //    .UseMiddleware<TusProtocolHandlerEndpointBased<TController>>(options)
            //    .Build();

            var handler = new TusProtocolHandlerEndpointBased<TController>(options)
            {
                UrlPath = pattern
            };
            return endpoints.Map(pattern + "/{TusFileId?}", handler.Invoke);
        }

        /// <summary>
        /// Maps a tus endpoint to the specified pattern. For advanced configuration use MapTusController
        /// </summary>
        public static IEndpointConventionBuilder MapTus(this IEndpointRouteBuilder endpoints, string pattern, Action<TusSimpleEndpointOptions> config = null)
        {
            var options = new TusSimpleEndpointOptions();

            config?.Invoke(options);

            var handler = new TusProtocolHandlerEndpointBased<EventsBasedTusController, TusSimpleEndpointOptions>(options, options)
            {
                UrlPath = pattern
            };
            return endpoints.Map(pattern + "/{TusFileId?}", handler.Invoke);
        }

        /*private static IEndpointConventionBuilder MapTusController<TController, TControllerOptions>(this IEndpointRouteBuilder endpoints, string pattern, Action<TusEndpointOptions> config, TControllerOptions opts)
            where TController : TusControllerBase, IControllerWithOptions<TControllerOptions>
        {
            var options = new TusEndpointOptions();

            config(options);

            var handler = new TusProtocolHandlerEndpointBased<TController, TControllerOptions>(options, opts);
            return endpoints.Map(pattern + "/{TusFileId?}", handler.Invoke);
        }*/
    }
}

#endif