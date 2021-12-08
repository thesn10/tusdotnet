#if endpointrouting

using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace tusdotnet.Storage
{
    /// <summary>
    /// Default <see cref="ITusStorageClientProvider"/> suitable for DI
    /// </summary>
    public class DefaultStorageClientProvider : ITusStorageClientProvider
    {
        private readonly DefaultStorageClientProviderOptions _options;
        private readonly IOptionsMonitor<TusStorageClientConfiguratorOptions> _configuratorsMonitor;

        /// <summary>
        /// Initializes a new instance of <see cref="DefaultStorageClientProvider"/>
        /// </summary>
        public DefaultStorageClientProvider(IOptions<DefaultStorageClientProviderOptions> options, IOptionsMonitor<TusStorageClientConfiguratorOptions> configuratorsMonitor)
        {
            _options = options.Value;
            _configuratorsMonitor = configuratorsMonitor;
        }

        /// <inheritdoc />
        public Task<TusStorageClient> Default()
        {
            return Get("default");
        }

        /// <inheritdoc />
        public async Task<TusStorageClient> Get(string name)
        {
            var client = await GetOrNull(name);

            if (client == null)
            {
                throw new ArgumentException($"Storage client \"{name}\" not found", nameof(name));
            }

            return client;
        }

        /// <inheritdoc />
        public async Task<TusStorageClient> GetOrNull(string name)
        {
            if (name == "default") name = _options.DefaultName ?? name;

            var configuratorOptions = _configuratorsMonitor.Get(name);
            if (configuratorOptions != null && configuratorOptions.Configurator != null)
            {
                var store = await configuratorOptions.Configurator.GetStoreAsync();
                return new TusStorageClient(store);
            }
            else return default;
        }
    }
}

#endif