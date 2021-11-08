#if endpointrouting

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using tusdotnet.Interfaces;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public sealed class TusServiceCollection
    {
        public IServiceCollection Services { get; }

        internal TusServiceCollection(IServiceCollection services)
        {
            Services = services;
        }

        public TusServiceCollection AddController<TController>()
            where TController : TusControllerBase
        {
            Services.AddTransient<TController, TController>();

            return this;
        }

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
        /// Adds the neccessary services to use <see cref="TusEndpointBuilderExtensions.MapTusEndpoint(Microsoft.AspNetCore.Routing.IEndpointRouteBuilder, string, System.Action{TusSimpleEndpointOptions})"/>
        /// </summary>
        /// <returns></returns>
        public TusServiceCollection AddEndpointServices()
        {
            Services.AddTransient<EventsBasedTusController>();
            return this;
        }

        public TusServiceCollection AddStorage(string name, ITusStore store, bool isDefault = false)
        {
            return AddStorage(name, new TusStorageProfile(store), isDefault);
        }

        public TusServiceCollection AddStorage(string name, Func<HttpContext, ITusStore> storeFunc, bool isDefault = false)
        {
            return AddStorage(name, new TusStorageProfile(storeFunc), isDefault);
        }

        public TusServiceCollection AddStorage(string name, Func<HttpContext, Task<ITusStore>> storeFunc, bool isDefault = false)
        {
            return AddStorage(name, new TusStorageProfile(storeFunc), isDefault);
        }

        public TusServiceCollection AddStorage<TProfile>(string name, bool isDefault = false) where TProfile : ITusStorageProfile, new()
        {
            return AddStorage(name, new TProfile(), isDefault);
        }

        public TusServiceCollection AddStorage<TProfile>(string name, TProfile profile, bool isDefault = false) where TProfile : ITusStorageProfile
        {
            Services.Configure<TusStorageClientProviderOptions>((options) =>
            {
                options.Profiles.Add(name, profile);
                if (isDefault) options.DefaultProfile = name;
            });

            return this;
        }

        public TusServiceCollection SetDefaultStorage(string name)
        {
            Services.Configure<TusStorageClientProviderOptions>((options) =>
            {
                options.DefaultProfile = name;
            });

            return this;
        }
    }
}

#endif