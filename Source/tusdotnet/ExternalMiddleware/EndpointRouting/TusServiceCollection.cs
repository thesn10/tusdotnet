#if endpointrouting

// Uncomment when needed again in the future

/*using Microsoft.Extensions.DependencyInjection;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    // Uncomment when needed again
    public sealed class TusServiceCollection
    {
        public IServiceCollection Services { get; }

        internal TusServiceCollection(IServiceCollection services)
        {
            Services = services;
        }

        public TusServiceCollection AddController<TController>()
            where TController : TusController
        {
            Services.AddTransient<TController, TController>();

            return this;
        }
    }
}*/

#endif