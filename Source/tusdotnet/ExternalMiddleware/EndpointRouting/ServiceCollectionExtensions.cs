#if endpointrouting

using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTus(this IServiceCollection services)
        {
            //services.AddHttpContextAccessor();
            services.AddSingleton<TusStorageService>();

            var controllerTypes = Assembly.GetCallingAssembly().GetTypes().Where(type => 
                type.GetCustomAttribute<TusControllerAttribute>() != null && 
                type.IsSubclassOf(typeof(TusControllerBase)));

            foreach (var controllerType in controllerTypes)
            {
                services.AddTransient(controllerType);
            }

            // TODO
            services.AddTransient<SimpleTusController>();

            //return new TusServiceCollection(services);
            return services;
        }
    }
}

#endif