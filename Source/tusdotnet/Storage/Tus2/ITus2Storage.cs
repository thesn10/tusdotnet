using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Models;

namespace tusdotnet.Storage.Tus2
{
    public abstract class Tus2Storage
    {
        public virtual Task WriteData(string uploadToken, PipeReader reader, CancellationToken cancellationToken)
        {
            Tus2StorageThrowHelper.ThrowNotImplemented();
            return Task.CompletedTask;
        }

        public virtual Task CreateFile(string uploadToken, IDictionary<string, Metadata> metadata)
        {
            Tus2StorageThrowHelper.ThrowNotImplemented();
            return Task.CompletedTask;
        }

        public virtual Task Delete(string uploadToken)
        {
            Tus2StorageThrowHelper.ThrowNotImplemented();
            return Task.CompletedTask;
        }

        // TODO: Replace FileExist, GetOffset and IsComplete with something like "GetFileInfo"
        // so that we can grab these in a single call to the storage implementation.
        public virtual Task<bool> FileExist(string uploadToken)
        {
            Tus2StorageThrowHelper.ThrowNotImplemented();
            return Task.FromResult(false);
        }

        public virtual Task<long> GetOffset(string uploadToken)
        {
            Tus2StorageThrowHelper.ThrowNotImplemented();
            return Task.FromResult(0L);
        }

        public virtual Task<bool> IsComplete(string uploadToken)
        {
            Tus2StorageThrowHelper.ThrowNotImplemented();
            return Task.FromResult(false);
        }

        public virtual Task MarkComplete(string uploadToken)
        {
            Tus2StorageThrowHelper.ThrowNotImplemented();
            return Task.CompletedTask;
        }
    }
}