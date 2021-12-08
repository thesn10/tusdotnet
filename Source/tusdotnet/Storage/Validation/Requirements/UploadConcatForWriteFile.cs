using System.Net;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Exceptions;
using tusdotnet.Helpers;
using tusdotnet.Models.Concatenation;
using tusdotnet.Stores;

namespace tusdotnet.Storage.Validation
{
    internal sealed class UploadConcatForWriteFile : StorageRequirement
    {
        private readonly string _fileId;

        public UploadConcatForWriteFile(string fileId)
        {
            _fileId = fileId;
        }

        public override Task Validate(StoreAdapter store, CancellationToken cancellationToken)
        {
            if (!store.Extensions.Concatenation)
            {
                return TaskHelper.Completed;
            }

            return ValidateForPatch(store, cancellationToken);
        }

        private async Task ValidateForPatch(StoreAdapter store, CancellationToken cancellationToken)
        {
            var uploadConcat = await store.GetUploadConcatAsync(_fileId, cancellationToken);

            if (uploadConcat is FileConcatFinal)
            {
                throw new TusInvalidConcatException("File with \"Upload-Concat: final\" cannot be patched", HttpStatusCode.Forbidden);
            }
        }
    }
}
