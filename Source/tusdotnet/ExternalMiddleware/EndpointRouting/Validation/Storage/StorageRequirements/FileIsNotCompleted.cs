using System.Threading;
using System.Threading.Tasks;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.Validation.Storage
{
    internal sealed class FileIsNotCompleted : StorageRequirement
    {
        private readonly string _fileId;

        public FileIsNotCompleted(string fileId)
        {
            _fileId = fileId;
        }

        public override async Task Validate(StoreAdapter store, CancellationToken cancellationToken)
        {
            var fileUploadLength = store.GetUploadLengthAsync(_fileId, cancellationToken);
            var fileOffset = store.GetUploadOffsetAsync(_fileId, cancellationToken);

            await Task.WhenAll(fileUploadLength, fileOffset);

            if (fileUploadLength != null && fileOffset.Result == fileUploadLength.Result)
            {
                throw new TusFileAlreadyCompleteException();
            }
        }
    }
}
