#if endpointrouting

using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Models.Expiration;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public sealed class TusStorageService
    {
        public TusStorageService()
        {

        }

        public async Task<CreateResult> Create(CreateContext context, CreateOptions options, CancellationToken cancellationToken)
        {
            options.Validate();

            var storeAdapter = new StoreAdapter(options.Store);
            var createResult = new CreateResult();

            createResult.FileId = await storeAdapter.CreateFileAsync(context.UploadLength, context.UploadMetadata, cancellationToken);

            if (storeAdapter.Extensions.Expiration && options.Expiration != null && context.UploadLength != 0)
            {
                // Expiration is only used when patching files so if the file is not empty and we did not have any data in the current request body,
                // we need to update the header here to be able to keep track of expiration for this file.
                createResult.FileExpires = options.GetSystemTime().Add(options.Expiration.Timeout);
                await storeAdapter.SetExpirationAsync(createResult.FileId, createResult.FileExpires.Value, cancellationToken);
            }

            return createResult;
        }

        public async Task<WriteResult> Write(WriteContext context, WriteOptions options, CancellationToken cancellationToken)
        {
            options.Validate();

            var storeAdapter = new StoreAdapter(options.Store);
            var writeResult = new WriteResult();

            var fileLock = await options.FileLockProvider.AquireLock(context.FileId);

            var hasLock = await fileLock.Lock();
            if (!hasLock)
            {
                throw new TusFileAlreadyInUseException(context.FileId);
            }

            if (context.UploadLength.HasValue && storeAdapter.Extensions.CreationDeferLength)
            {
                await storeAdapter.SetUploadLengthAsync(context.FileId, context.UploadLength.Value, cancellationToken);
            }

            var guardedStream = new ClientDisconnectGuardedReadOnlyStream(context.RequestStream, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken));
            var bytesWritten = await storeAdapter.AppendDataAsync(context.FileId, guardedStream, guardedStream.CancellationToken);
            await fileLock.ReleaseIfHeld();

            writeResult.UploadOffset = context.UploadOffset.Value + bytesWritten;

            if (storeAdapter.Extensions.Expiration)
            {
                if (options.Expiration is SlidingExpiration)
                {
                    writeResult.FileExpires = options.GetSystemTime().Add(options.Expiration.Timeout);
                    await storeAdapter.SetExpirationAsync(context.FileId, writeResult.FileExpires.Value, cancellationToken);
                }
                else
                {
                    writeResult.FileExpires = await storeAdapter.GetExpirationAsync(context.FileId, cancellationToken);
                }
            }

            if (storeAdapter.Extensions.Checksum)
            {
                var checksum = context.GetChecksumProvidedByClient();

                if (checksum != null)
                    writeResult.ChecksumMatches = await storeAdapter.VerifyChecksumAsync(context.FileId, checksum.Algorithm, checksum.Hash, cancellationToken);
            }

            writeResult.IsComplete = await storeAdapter.GetUploadLengthAsync(context.FileId, cancellationToken) == writeResult.UploadOffset;
            return writeResult;
        }

        public async Task Delete(DeleteContext context, DeleteOptions options, CancellationToken cancellationToken)
        {
            options.Validate();

            var storeAdapter = new StoreAdapter(options.Store);

            await storeAdapter.DeleteFileAsync(context.FileId, cancellationToken);
        }

        public Task<ITusFile> Read(ITusStore store, string fileId, CancellationToken cancellationToken)
        {
            var storeAdapter = new StoreAdapter(store);

            return storeAdapter.GetFileAsync(fileId, cancellationToken);
        }

        public async Task<GetFileInfoResult> GetFileInfo(GetFileInfoContext context, GetFileInfoOptions options, CancellationToken cancellationToken)
        {
            options.Validate();

            var storeAdapter = new StoreAdapter(options.Store);
            var getFileInfoResult = new GetFileInfoResult();

            if (storeAdapter.Extensions.Creation)
            {
                getFileInfoResult.UploadMetadata = await storeAdapter.GetUploadMetadataAsync(context.FileId, cancellationToken);
            }

            getFileInfoResult.UploadLength = await storeAdapter.GetUploadLengthAsync(context.FileId, cancellationToken);
            getFileInfoResult.UploadOffset = await storeAdapter.GetUploadOffsetAsync(context.FileId, cancellationToken);

            if (storeAdapter.Extensions.Concatenation)
            {
                getFileInfoResult.UploadConcat = await storeAdapter.GetUploadConcatAsync(context.FileId, cancellationToken);
            }

            return getFileInfoResult;
        }
    }
}

#endif