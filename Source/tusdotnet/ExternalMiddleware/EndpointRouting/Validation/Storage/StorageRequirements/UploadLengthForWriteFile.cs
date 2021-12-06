using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.Models;
using tusdotnet.Stores;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.Validation.Storage
{
    internal sealed class UploadLengthForWriteFile : StorageRequirement
    {
        private readonly string _fileId;
        private readonly long? _uploadLength;

        public UploadLengthForWriteFile(string fileId, long? uploadLength)
        {
            _fileId = fileId;
            _uploadLength = uploadLength;
        }

        public override async Task Validate(StoreAdapter store, CancellationToken cancellationToken)
        {
            var uploadLengthIsSet = await UploadLengthIsAlreadyPresent(store, cancellationToken);

            if (!store.Extensions.CreationDeferLength && !uploadLengthIsSet)
            {
                throw new TusConfigurationException($"File {_fileId} does not have an upload length and the current store ({store.Store.GetType().FullName}) does not support Upload-Defer-Length so no new upload length can be set");
            }

            if (!_uploadLength.HasValue && !uploadLengthIsSet)
            {
                throw new TusUploadLengthException($"Header {HeaderConstants.UploadLength} must be specified as this file was created using Upload-Defer-Length");
            }

            if (_uploadLength.HasValue && uploadLengthIsSet)
            {
                throw new TusUploadLengthException($"{HeaderConstants.UploadLength} cannot be updated once set");
            }
        }

        private async Task<bool> UploadLengthIsAlreadyPresent(StoreAdapter store, CancellationToken cancellationToken)
        {
            var fileUploadLength = await store.GetUploadLengthAsync(_fileId, cancellationToken);
            return fileUploadLength != null;
        }
    }
}