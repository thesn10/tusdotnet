using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Storage.Validation;
using tusdotnet.Stores;

namespace tusdotnet.Storage.Handlers
{
    internal class CreateOperationHandler : StorageOperationHandler
    {
        internal CreateOperationHandler(StoreAdapter storeAdapter) : base(storeAdapter)
        {
        }

        internal async Task<CreateResult> Create(long uploadLength, string uploadMetadata, bool isPartial = false, string[] partialFiles = null,
                    CreateOptions options = default, CancellationToken cancellationToken = default)
        {
            options ??= new CreateOptions();

            var createResult = new CreateResult();

            if (_storeAdapter.Extensions.Concatenation && isPartial)
            {
                createResult.FileId = await _storeAdapter.CreatePartialFileAsync(uploadLength, uploadMetadata, cancellationToken);
            }
            else if (_storeAdapter.Extensions.Concatenation && partialFiles != null)
            {
                var validator = new StorageValidator(new FinalFileConcatValid(partialFiles, options.MaxConcatFileSize));
                await validator.Validate(_storeAdapter, cancellationToken);

                createResult.FileId = await _storeAdapter.CreateFinalFileAsync(partialFiles, uploadMetadata, cancellationToken);
            }
            else
            {
                createResult.FileId = await _storeAdapter.CreateFileAsync(uploadLength, uploadMetadata, cancellationToken);
            }

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
