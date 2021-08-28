#if endpointrouting

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public static class EndpointBuilderExtensions
    {
        public static IEndpointConventionBuilder MapTusController<TController>(this IEndpointRouteBuilder endpoints, string pattern, Action<TusEndpointOptions> config = default)
            where TController : TusControllerBase
        {
            var options = new TusEndpointOptions();
            config?.Invoke(options);

            //var reqDelegate = _endpoints
            //    .CreateApplicationBuilder()
            //    .UseMiddleware<TusProtocolHandlerEndpointBased<TController>>(options)
            //    .Build();

            var handler = new TusProtocolHandlerEndpointBased<TController>(options);
            return endpoints.Map(pattern + "/{TusFileId?}", handler.Invoke);
        }

        public static IEndpointConventionBuilder MapTusSimpleEndpoint(this IEndpointRouteBuilder endpoints, string pattern, Action<TusEndpointOptions, StorageOptions> config)
        {
            var options = new TusEndpointOptions();
            var storageOptions = new StorageOptions();

            config(options, storageOptions);

            var handler = new TusProtocolHandlerEndpointBased<SimpleTusController, StorageOptions>(options, storageOptions);
            return endpoints.Map(pattern + "/{TusFileId?}", handler.Invoke);
        }
    }
}

#endif