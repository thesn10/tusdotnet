using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using tusdotnet.Controllers;
using tusdotnet.ExternalMiddleware.EndpointRouting;

namespace tusdotnet
{
    /// <summary>
    /// Configure contoller services
    /// </summary>
    public class ControllerServicesOptions
    {
        internal ControllerServicesOptions(IServiceCollection services)
        {
            _services = services;
        }

        private readonly IServiceCollection _services;

        /// <summary>
        /// Adds a tus controller
        /// </summary>
        /// <typeparam name="TController"></typeparam>
        /// <returns></returns>
        public void AddController<TController>()
            where TController : TusControllerBase
        {
            _services.AddTransient<TController, TController>();
        }

        /// <summary>
        /// Adds all controllers in the given assembly
        /// </summary>
        public void AddControllersFromAssemblyOf<TType>()
        {
            var controllerTypes = Assembly.GetAssembly(typeof(TType)).GetTypes().Where(type =>
                type.GetCustomAttribute<TusControllerAttribute>() != null &&
                type.IsSubclassOf(typeof(TusControllerBase)));

            foreach (var controllerType in controllerTypes)
            {
                _services.AddTransient(controllerType);
            }
        }

        /// <summary>
        /// Adds the neccessary services to use <see cref="TusEndpointBuilderExtensions.MapTus"/>
        /// </summary>
        public void AddEndpointServices(string pattern, Action<TusSimpleEndpointOptions> config = null)
        {

            _services.ConfigureOptions<TusSimpleEndpointOptions>().Configure<TusSimpleEndpointOptions>(pattern, config);

            _services.AddTransient<EventsBasedTusController>();
        }
    }
}
