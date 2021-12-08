#if endpointrouting
using Microsoft.Extensions.DependencyInjection;
using tusdotnet.Storage;

namespace tusdotnet
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
            services.AddSingleton<ITusStorageClientProvider, DefaultStorageClientProvider>();

            return new TusServiceCollection(services);
        }
    }
}
#endif