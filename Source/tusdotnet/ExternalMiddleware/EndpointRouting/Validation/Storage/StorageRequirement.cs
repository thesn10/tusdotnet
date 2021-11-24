#if endpointrouting

using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Stores;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.Validation
{
    internal abstract class StorageRequirement
    {
        public abstract Task Validate(StoreAdapter store, CancellationToken cancellationToken);
    }

    internal interface IStorageRequirementWithPostValidation
    {
        public abstract Task PostValidate(StoreAdapter store, CancellationToken cancellationToken);
    }
}
#endif