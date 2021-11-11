using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Stores;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.StorageOperations
{
    internal class CreateOperationHandler : StorageOperationHandler
    {
        internal CreateOperationHandler(StoreAdapter storeAdapter) : base(storeAdapter)
        {
        }

        internal async Task<CreateResult> Create(long uploadLength, string uploadMetadata,
                    CreateOptions options = default, CancellationToken cancellationToken = default)
        {
            options ??= new CreateOptions();

            var createResult = new CreateResult();

            createResult.FileId = await _storeAdapter.CreateFileAsync(uploadLength, uploadMetadata, cancellationToken);

            if (_storeAdapter.Extensions.Expiration && options.Expiration != null && uploadLength != 0)
            {
                // Expiration is only used when patching files so if the file is not empty and we did not have any data in the current request body,
                // we need to update the header here to be able to keep track of expiration for this file.
                createResult.FileExpires = options.GetSystemTime().Add(options.Expiration.Timeout);
                await _storeAdapter.SetExpirationAsync(createResult.FileId, createResult.FileExpires.Value, cancellationToken);
            }

            return createResult;
        }
    }
}
