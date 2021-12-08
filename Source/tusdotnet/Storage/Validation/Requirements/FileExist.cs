using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Exceptions;
using tusdotnet.Stores;

namespace tusdotnet.Storage.Validation
{
    internal sealed class FileExist : StorageRequirement
    {
        private readonly string _fileId;

        public FileExist(string fileId)
        {
            _fileId = fileId;
        }

        public override async Task Validate(StoreAdapter store, CancellationToken cancellationToken)
        {
            var exists = await store.FileExistAsync(_fileId, cancellationToken);
            if (!exists)
            {
                throw new TusFileNotFoundException();
            }
        }
    }
}
