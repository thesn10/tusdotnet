#if endpointrouting

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using tusdotnet.Interfaces;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Configures a store using HttpContext. 
    /// Not recommended because it uses <see cref="IHttpContextAccessor"/>
    /// Alternatively, use <see cref="ITusStorageClientProvider.CreateNew(ITusStore)"/> inside your controller instead of this configurator.
    /// </summary>
    internal class HttpContextStoreConfigurator : ITusStoreConfigurator
    {
        private readonly Func<HttpContext, Task<ITusStore>> _configure;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextStoreConfigurator(Func<HttpContext, ITusStore> configure, IServiceProvider services) 
            : this((context) => Task.FromResult(configure(context)), services)
        {
        }

        public HttpContextStoreConfigurator(Func<HttpContext, Task<ITusStore>> configure, IServiceProvider services)
        {
            _configure = configure;
            _httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();
        }

        public Task<ITusStore> GetStoreAsync()
        {
            return _configure(_httpContextAccessor.HttpContext);
        }
    }
}

#endif
