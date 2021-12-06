#if endpointrouting

using System.Threading.Tasks;
using tusdotnet.Interfaces;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// An <see cref="ITusStorageClientProvider"/> which only provides one single specific ITusStore
    /// Used by <see cref="TusCoreMiddleware"/>
    /// </summary>
    public class SingleStorageClientProvider : ITusStorageClientProvider
    {
        private readonly TusStorageClient _storageClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleStorageClientProvider"/> class.
        /// </summary>
        public SingleStorageClientProvider(ITusStore store)
        {
            _storageClient = new TusStorageClient(store);
        }

        /// <inheritdoc />
        public Task<TusStorageClient> Default()
        {    
            return Task.FromResult(_storageClient);
        }

        /// <inheritdoc />
        public Task<TusStorageClient> Get(string name)
        {
            return Task.FromResult(_storageClient);
        }

        /// <inheritdoc />
        public Task<TusStorageClient> GetOrNull(string name)
        {
            return Task.FromResult(_storageClient);
        }
    }
}
#endif