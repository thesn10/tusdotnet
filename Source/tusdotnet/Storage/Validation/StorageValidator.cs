using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Stores;

namespace tusdotnet.Storage.Validation
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
                if (spec == null) continue;

                await spec.Validate(store, cancellationToken);
            }
        }

        public async Task PostValidate(StoreAdapter store, CancellationToken cancellationToken)
        {
            foreach (var spec in _requirements.OfType<IStorageRequirementWithPostValidation>())
            {
                if (spec == null) continue;

                await spec.PostValidate(store, cancellationToken);
            }
        }
    }
}