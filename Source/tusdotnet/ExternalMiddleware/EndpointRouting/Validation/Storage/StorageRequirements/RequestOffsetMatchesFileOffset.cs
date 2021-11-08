using System.Threading;
using System.Threading.Tasks;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.Validation.Storage
{
    internal sealed class RequestOffsetMatchesFileOffset : StorageRequirement
    {
        private readonly long _requestOffset;
        private readonly string _fileId;

        public RequestOffsetMatchesFileOffset(long requestOffset, string fileId)
        {
            _requestOffset = requestOffset;
            _fileId = fileId;
        }

        public override async Task Validate(StoreAdapter store, CancellationToken cancellationToken)
        {
            var fileOffset = await store.GetUploadOffsetAsync(_fileId, cancellationToken);

            if (_requestOffset != fileOffset)
            {
                throw new TusInvalidOffsetException(
                    $"Offset does not match file. File offset: {fileOffset}. Request offset: {_requestOffset}");
            }
        }
    }
}
