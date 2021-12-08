#if endpointrouting

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using tusdotnet.Interfaces;
using tusdotnet.Storage;

namespace tusdotnet
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