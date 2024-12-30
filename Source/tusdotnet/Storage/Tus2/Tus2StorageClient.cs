using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Models;
using tusdotnet.Storage.Results.Tus2;

namespace tusdotnet.Storage.Tus2
{
    public class Tus2StorageClient
    {
        public Tus2StorageClient(Tus2Storage storage)
        {
            Storage = storage;
        }

        public Tus2Storage Storage { get; }

        public virtual async Task<RetrieveOffsetResult> RetrieveOffset(string uploadToken)
        {
            var offsetTask = Storage.GetOffset(uploadToken);
            var isCompleteTask = Storage.IsComplete(uploadToken);

            await Task.WhenAll(offsetTask, isCompleteTask);

            var offset = offsetTask.Result;
            var isComplete = isCompleteTask.Result;

            return new()
            {
                UploadOffset = offset,
                UploadIncomplete = !isComplete,
            };
        }

        public virtual async Task Delete(string uploadToken)
        {
            await Storage.Delete(uploadToken);
        }

        public virtual async Task CreateFile(string fileId, IDictionary<string, Metadata> metadata)
        {
            
        }

        public async Task<Results.Tus2.WriteResult> WriteData(string uploadToken, PipeReader reader, bool isUploadIncomplete, CancellationToken cancellationToken = default)
        {
            try
            {
                await Storage.WriteData(uploadToken, reader, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // Left blank. This is the case when the store does throws on cancellation instead of returning.
            }

            var uploadOffset = await Storage.GetOffset(uploadToken);

            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            if (isUploadIncomplete)
            {
                await Storage.MarkComplete(uploadToken);
            }

            return new()
            {
                //Status = HttpStatusCode.Created,
                UploadOffset = uploadOffset,
            };
        }

    }
}

