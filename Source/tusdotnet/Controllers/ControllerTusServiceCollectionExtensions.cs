using Microsoft.Extensions.DependencyInjection;
using System;
using tusdotnet.ExternalMiddleware.EndpointRouting;
using tusdotnet.Routing;

namespace tusdotnet
{
    /// <summary>
    /// Tus service collection extensions to configure tus controller services
    /// </summary>
    public static class ControllerTusServiceCollectionExtensions
    {
        /// <summary>
        /// Adds all neccesary services to use tus controllers
        /// </summary>
        public static TusServiceCollection AddControllerServices(this TusServiceCollection serviceCollection, Action<ControllerServicesOptions> configure)
        {
            serviceCollection.Services.AddSingleton<ITusRoutingHelperFactory, TusEndpointRoutingHelperFactory>();

            var opts = new ControllerServicesOptions(serviceCollection.Services);
            configure(opts);

            return serviceCollection;
        }
    }
}
