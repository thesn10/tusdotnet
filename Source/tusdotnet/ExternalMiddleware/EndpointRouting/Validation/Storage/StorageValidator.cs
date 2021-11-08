#if endpointrouting

using System.Threading;
using System.Threading.Tasks;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.Validation
{
    internal sealed class StorageValidator
    {
        private readonly StorageRequirement[] _requirements;

        public StorageValidator(params StorageRequirement[] requirements)
        {
            _requirements = requirements ?? new StorageRequirement[0];
        }

        public async Task Validate(StoreAdapter store, CancellationToken cancellationToken)
        {
            foreach (var spec in _requirements)
            {
                await spec.Validate(store, cancellationToken);
            }
        }
    }
}
#endif