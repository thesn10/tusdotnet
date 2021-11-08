#if endpointrouting

using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public static class ServiceCollectionExtensions
    {
        public static TusServiceCollection AddTus(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            //services.Configure<TusStorageClientProviderOptions>();
            services.AddSingleton<ITusStorageClientProvider, TusStorageClientProvider>();

            return new TusServiceCollection(services);
        }
    }
}

#endif