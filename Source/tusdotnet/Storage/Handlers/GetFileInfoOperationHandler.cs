using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Storage.Validation;
using tusdotnet.Stores;

namespace tusdotnet.Storage.Handlers
{
    internal class GetFileInfoOperationHandler : StorageOperationHandler
    {
        internal GetFileInfoOperationHandler(StoreAdapter storeAdapter) : base(storeAdapter)
        {
        }

        internal async Task<GetFileInfoResult> GetFileInfo(string fileId, CancellationToken cancellationToken = default)
        {
            var getFileInfoResult = new GetFileInfoResult();

            var validator = new StorageValidator(
                new FileExist(fileId),
                new FileHasNotExpired(fileId));

            await validator.Validate(_storeAdapter, cancellationToken);

            if (_storeAdapter.Extensions.Creation)
            {
                getFileInfoResult.UploadMetadata = await _storeAdapter.GetUploadMetadataAsync(fileId, cancellationToken);
            }

            getFileInfoResult.UploadLength = await _storeAdapter.GetUploadLengthAsync(fileId, cancellationToken);
            getFileInfoResult.UploadOffset = await _storeAdapter.GetUploadOffsetAsync(fileId, cancellationToken);

            if (_storeAdapter.Extensions.Concatenation)
            {
                getFileInfoResult.FileConcatenation = await _storeAdapter.GetUploadConcatAsync(fileId, cancellationToken);
            }

            return getFileInfoResult;
        }
    }
}
