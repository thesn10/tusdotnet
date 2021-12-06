#if endpointrouting

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using tusdotnet.Interfaces;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Tus service collection to configure tus services
    /// </summary>
    public sealed class TusServiceCollection
    {
        /// <summary>
        /// Gets the <see cref="IServiceCollection"/> where tus services are configured
        /// </summary>
        public IServiceCollection Services { get; }

        internal TusServiceCollection(IServiceCollection services)
        {
            Services = services;
        }

        /// <summary>
        /// Adds a tus controller
        /// </summary>
        /// <typeparam name="TController"></typeparam>
        /// <returns></returns>
        public TusServiceCollection AddController<TController>()
            where TController : TusControllerBase
        {
            Services.AddTransient<TController, TController>();

            return this;
        }

        /// <summary>
        /// Adds all controllers in the given assembly
        /// </summary>
        public TusServiceCollection AddControllersFromAssemblyOf<TType>()
        {
            var controllerTypes = Assembly.GetAssembly(typeof(TType)).GetTypes().Where(type =>
                type.GetCustomAttribute<TusControllerAttribute>() != null &&
                type.IsSubclassOf(typeof(TusControllerBase)));

            foreach (var controllerType in controllerTypes)
            {
                Services.AddTransient(controllerType);
            }

            return this;
        }

        /// <summary>
        /// Adds the neccessary services to use <see cref="TusEndpointBuilderExtensions.MapTus(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder, string, System.Action{TusSimpleEndpointOptions})"/>
        /// </summary>
        /// <returns></returns>
        public TusServiceCollection AddEndpointServices()
        {
            Services.AddTransient<EventsBasedTusController>();
            return this;
        }

        /// <inheritdoc cref="AddStorage{TStoreConfigurator}(string, Func{IServiceProvider, TStoreConfigurator}, bool)"/>
        public TusServiceCollection AddStorage(string name, ITusStore store, bool isDefault = false)
        {
            return AddStorage(name, new DefaultStoreConfigurator(store), isDefault);
        }

        /// <inheritdoc cref="AddStorage{TStoreConfigurator}(string, Func{IServiceProvider, TStoreConfigurator}, bool)"/>
        public TusServiceCollection AddStorage(string name, Func<HttpContext, ITusStore> configure, bool isDefault = false)
        {
            return AddStorage(name, (services) => new HttpContextStoreConfigurator(configure, services), isDefault);
        }

        /// <inheritdoc cref="AddStorage{TStoreConfigurator}(string, Func{IServiceProvider, TStoreConfigurator}, bool)"/>
        public TusServiceCollection AddStorage(string name, Func<HttpContext, Task<ITusStore>> configure, bool isDefault = false)
        {
            return AddStorage(name, (services) => new HttpContextStoreConfigurator(configure, services), isDefault);
        }

        /// <inheritdoc cref="AddStorage{TStoreConfigurator}(string, Func{IServiceProvider, TStoreConfigurator}, bool)"/>
        public TusServiceCollection AddStorage<TStoreConfigurator>(string name, bool isDefault = false) where TStoreConfigurator : ITusStoreConfigurator
        {
            return AddStorage(name, (services) => ActivatorUtilities.CreateInstance<TStoreConfigurator>(services), isDefault);
        }

        /// <inheritdoc cref="AddStorage{TStoreConfigurator}(string, Func{IServiceProvider, TStoreConfigurator}, bool)"/>
        public TusServiceCollection AddStorage<TStoreConfigurator>(string name, TStoreConfigurator configurator, bool isDefault = false) where TStoreConfigurator : ITusStoreConfigurator
        {
            return AddStorage(name, (_) => configurator, isDefault);
        }

        /// <summary>
        /// Adds a storage profile
        /// </summary>
        public TusServiceCollection AddStorage<TStoreConfigurator>(string name, Func<IServiceProvider, TStoreConfigurator> configure, bool isDefault = false) where TStoreConfigurator : ITusStoreConfigurator
        {
            if (isDefault)
            {
                SetDefaultStorage(name);
            }

            Services.AddTransient<IConfigureOptions<TusStorageClientConfiguratorOptions>>(services =>
            {
                return new ConfigureNamedOptions<TusStorageClientConfiguratorOptions>(name, options =>
                {
                    options.Configurator = configure(services);
                });
            });

            return this;
        }

        /// <summary>
        /// Set the default storage to use
        /// </summary>
        public TusServiceCollection SetDefaultStorage(string name)
        {
            Services.Configure<DefaultStorageClientProviderOptions>((options) =>
            {
                options.DefaultName = name;
            });

            return this;
        }
    }
}

#endif