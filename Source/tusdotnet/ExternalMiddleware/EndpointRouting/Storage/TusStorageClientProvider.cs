#if endpointrouting
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using tusdotnet.Interfaces;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusStorageClientProvider : ITusStorageClientProvider
    {
        private readonly TusStorageClientProviderOptions _options;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TusStorageClientProvider(IOptions<TusStorageClientProviderOptions> options, IHttpContextAccessor httpContextAccessor)
        {
            _options = options.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<TusStorageClient> Default()
        {    
            return Get("default");
        }

        public async Task<TusStorageClient> Get(string name)
        {
            var client = await GetOrNull(name);

            if (client == null)
            {
                throw new ArgumentException($"Profile {name} not found", nameof(name));
            }

            return client;
        }

        public async Task<TusStorageClient> GetOrNull(string name)
        {
            if (name == "default") name = _options.DefaultProfile ?? name;

            if (_options.Profiles.TryGetValue(name, out var profile))
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var store = await profile.GetStore(httpContext);

                return new TusStorageClient(store);
            }
            else return default;
        }

        public TusStorageClient Create(ITusStore store)
        {
            return new TusStorageClient(store);
        }
    }
}
#endif