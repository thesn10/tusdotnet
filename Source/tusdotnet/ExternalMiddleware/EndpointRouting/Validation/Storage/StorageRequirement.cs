#if endpointrouting

using System.Threading;
using System.Threading.Tasks;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.Validation
{
    internal abstract class StorageRequirement
    {
        public abstract Task Validate(StoreAdapter store, CancellationToken cancellationToken);
    }
}
#endif