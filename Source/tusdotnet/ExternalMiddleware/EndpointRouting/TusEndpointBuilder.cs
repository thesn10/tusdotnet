#if endpointrouting

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public interface ITusEndpointBuilder
    {
        public IEndpointConventionBuilder MapController<TController>(string pattern, Action<TusEndpointOptions> config = default)
            where TController : TusControllerBase;

        public IEndpointConventionBuilder Map(string pattern, Action<TusEndpointOptions, StorageOptions> config);
    }

    public class TusEndpointBuilder : ITusEndpointBuilder
    {
        private readonly IEndpointRouteBuilder _endpoints;

        public TusEndpointBuilder(IEndpointRouteBuilder endpoints)
        {
            _endpoints = endpoints;
        }

        public IEndpointConventionBuilder MapController<TController>(string pattern, Action<TusEndpointOptions> config = default)
            where TController : TusControllerBase
        {
            var options = new TusEndpointOptions();
            config?.Invoke(options);

            //var reqDelegate = _endpoints
            //    .CreateApplicationBuilder()
            //    .UseMiddleware<TusProtocolHandlerEndpointBased<TController>>(options)
            //    .Build();

            var handler = new TusProtocolHandlerEndpointBased<TController>(options);
            return _endpoints.Map(pattern + "/{TusFileId?}", handler.Invoke);
        }

        public IEndpointConventionBuilder Map(string pattern, Action<TusEndpointOptions, StorageOptions> config)
        {
            var options = new TusEndpointOptions();
            var storageOptions = new StorageOptions();

            config(options, storageOptions);

            var handler = new TusProtocolHandlerEndpointBased<SimpleTusController, StorageOptions>(options, storageOptions);
            return _endpoints.Map(pattern + "/{TusFileId?}", handler.Invoke);
        }
    }
}
#endif