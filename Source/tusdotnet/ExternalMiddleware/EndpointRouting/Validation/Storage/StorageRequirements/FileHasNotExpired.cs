using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Extensions;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.Validation.Storage
{
    internal class FileHasNotExpired : StorageRequirement
    {
        private readonly string _fileId;

        public FileHasNotExpired(string fileId)
        {
            _fileId = fileId;
        }

        public override Task Validate(StoreAdapter store, CancellationToken cancellationToken)
        {
            if (!store.Extensions.Expiration)
            {
                return Task.FromResult(0);
            }

            return ValidateInternal(store, cancellationToken);
        }

        private async Task ValidateInternal(StoreAdapter store, CancellationToken cancellationToken)
        {
            var expires = await store.GetExpirationAsync(_fileId, cancellationToken);
            if (expires?.HasPassed() == true)
            {
                throw new TusFileExpiredException();
            }
        }
    }
}
