#if endpointrouting

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using tusdotnet.Constants;
using tusdotnet.Routing;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Tus Extensions for <see cref="IEndpointRouteBuilder"/>
    /// </summary>
    public static class TusEndpointBuilderExtensions
    {
        /// <summary>
        /// Maps a tus controller to the specified pattern
        /// </summary>
        public static IEndpointConventionBuilder MapTusControllerRoute<TController>(this IEndpointRouteBuilder endpoints, string pattern, Action<TusEndpointOptions> config = default)
            where TController : TusControllerBase
        {
            EnsureControllerServices(endpoints);

            var routePattern = RoutePatternFactory.Parse(pattern);
            if (!routePattern.Parameters.Any(x => x.Name == RouteConstants.FileId))
            {
                throw new ArgumentException($"Routing pattern must contain {RouteConstants.FileId} parameter", nameof(pattern));
            }

            var options = new TusEndpointOptions();
            config?.Invoke(options);

            var handler = new TusProtocolHandlerEndpointBased<TController>(options);
            return endpoints.Map(routePattern, handler.Invoke).WithMetadata(new EndpointNameMetadata(pattern));
        }

        /// <summary>
        /// Maps a tus controller and adds the default route <code>/{controllerName}/{fileId?}</code>
        /// Uses the controller type name if no <paramref name="controllerName"/> is specified
        /// </summary>
        public static IEndpointConventionBuilder MapTusController<TController>(this IEndpointRouteBuilder endpoints, string controllerName = null, Action<TusEndpointOptions> config = default)
            where TController : TusControllerBase
        {
            if (controllerName == null)
            {
                controllerName = typeof(TController).FullName.Replace("TusController", "").Replace("Controller", "");
                if (string.IsNullOrWhiteSpace(controllerName))
                {
                    throw new ArgumentException("Cannot extract controller name from Type", nameof(TController));
                }
            }

            return MapTusControllerRoute<TController>(endpoints, "/" + controllerName + "/{" + RouteConstants.FileId + "?}", config);
        }

        /// <summary>
        /// Maps a tus endpoint to the specified pattern. For advanced configuration use MapTusController
        /// </summary>
        public static IEndpointConventionBuilder MapTus(this IEndpointRouteBuilder endpoints, string pattern, Action<TusSimpleEndpointOptions> config = null)
        {
            EnsureControllerServices(endpoints);
            EnsureEndpointServices(endpoints);

            var routePattern = RoutePatternFactory.Parse(pattern);
            if (!routePattern.Parameters.Any(x => x.Name == RouteConstants.FileId))
            {
                throw new ArgumentException($"Routing pattern must contain {RouteConstants.FileId} parameter", nameof(pattern));
            }

            var options = new TusSimpleEndpointOptions();
            config?.Invoke(options);

            var handler = new TusProtocolHandlerEndpointBased<EventsBasedTusController, TusSimpleEndpointOptions>(options, options);
            return endpoints.Map(routePattern, handler.Invoke).WithMetadata(new EndpointNameMetadata(pattern));
        }

        private static void EnsureEndpointServices(IEndpointRouteBuilder endpoints)
        {
            var marker = endpoints.ServiceProvider.GetService<EventsBasedTusController>();
            if (marker == null)
            {
                throw new InvalidOperationException($"Unable to find required services. Call IServiceCollection.AddTus().AddEndpointServices() in ConfigureServices(...)");
            }
        }

        private static void EnsureControllerServices(IEndpointRouteBuilder endpoints)
        {
            var marker = endpoints.ServiceProvider.GetService<ITusStorageClientProvider>();
            var marker2 = endpoints.ServiceProvider.GetService<ITusRoutingHelperFactory>();
            if (marker == null || marker2 == null)
            {
                throw new InvalidOperationException($"Unable to find required services. Call IServiceCollection.AddTus() in ConfigureServices(...)");
            }
        }
    }
}

#endif