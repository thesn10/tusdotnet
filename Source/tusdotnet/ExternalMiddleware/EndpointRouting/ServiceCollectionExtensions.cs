#if endpointrouting

using Microsoft.Extensions.DependencyInjection;
using tusdotnet.ExternalMiddleware.EndpointRouting.Routing;
using tusdotnet.Routing;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Service collection extensions
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Tus services to the service collection
        /// </summary>
        public static TusServiceCollection AddTus(this IServiceCollection services)
        {
            //services.AddHttpContextAccessor();
            services.AddSingleton<ITusStorageClientProvider, DefaultStorageClientProvider>();
            services.AddSingleton<ITusRoutingHelperFactory, TusEndpointRoutingHelperFactory>();

            return new TusServiceCollection(services);
        }
    }
}

#endif