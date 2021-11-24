#if endpointrouting
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using tusdotnet.Interfaces;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class StaticTusStorageClientProvider : ITusStorageClientProvider
    {
        private readonly TusStorageClient _storageClient;

        public StaticTusStorageClientProvider(ITusStore store)
        {
            _storageClient = new TusStorageClient(store);
        }

        public Task<TusStorageClient> Default()
        {    
            return Task.FromResult(_storageClient);
        }

        public Task<TusStorageClient> Get(string name)
        {
            return Task.FromResult(_storageClient);
        }

        public Task<TusStorageClient> GetOrNull(string name)
        {
            return Task.FromResult(_storageClient);
        }

        public TusStorageClient Create(ITusStore store)
        {
            return _storageClient;
        }
    }
}
#endif