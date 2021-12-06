using System.IO;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.ExternalMiddleware.EndpointRouting.Validation;
using tusdotnet.ExternalMiddleware.EndpointRouting.Validation.Storage;
using tusdotnet.Models;
using tusdotnet.Models.Expiration;
using tusdotnet.Stores;
using System;

#if pipelines
using System.IO.Pipelines;
#endif

namespace tusdotnet.ExternalMiddleware.EndpointRouting.StorageOperations
{
    internal class WriteOperationHandler : StorageOperationHandler
    {
        internal WriteOperationHandler(StoreAdapter storeAdapter) : base(storeAdapter)
        {
        }

#if pipelines
        internal Task<WriteResult> Write(string fileId, PipeReader requestPipe, long uploadOffset, long? uploadLength, bool assumeFileCreated,
            WriteOptions options = default, CancellationToken cancellationToken = default)
        {
            return WriteInternal(fileId, requestPipe, true, uploadOffset, uploadLength, assumeFileCreated, options, cancellationToken);
        }
#endif

        internal Task<WriteResult> Write(string fileId, Stream requestStream, long uploadOffset, long? uploadLength, bool assumeFileCreated,
            WriteOptions options = default, CancellationToken cancellationToken = default)
        {
            return WriteInternal(fileId, requestStream, false, uploadOffset, uploadLength, assumeFileCreated, options, cancellationToken);
        }

        private async Task<WriteResult> WriteInternal(string fileId, object requestBody, bool isPipeReader, long uploadOffset, long? uploadLength, bool assumeFileCreated,
            WriteOptions options = default, CancellationToken cancellationToken = default)
        {
            options ??= new WriteOptions();

            var checksum = options.GetChecksumProvidedByClient?.Invoke();

            var validator = new StorageValidator(
                assumeFileCreated ? null : new FileExist(fileId),
                new UploadLengthForWriteFile(fileId, uploadLength),
                assumeFileCreated ? null : new UploadConcatForWriteFile(fileId),
                new UploadChecksum(options.GetChecksumProvidedByClient, fileId),
                new FileHasNotExpired(fileId),
                assumeFileCreated ? null : new RequestOffsetMatchesFileOffset(uploadOffset, fileId),
                assumeFileCreated ? null : new FileIsNotCompleted(fileId));

            await validator.Validate(_storeAdapter, cancellationToken);

            var writeResult = new WriteResult();
            var fileLock = await options.FileLockProvider.AquireLock(fileId);
            var hasLock = await fileLock.Lock();

            if (!hasLock)
            {
                throw new TusFileAlreadyInUseException(fileId);
            }

            try
            {
                if (uploadLength.HasValue && _storeAdapter.Extensions.CreationDeferLength)
                {
                    await _storeAdapter.SetUploadLengthAsync(fileId, uploadLength.Value, cancellationToken);
                }

                long bytesWritten;
                try
                {
#if pipelines
                    if (isPipeReader)
                    {
                        var guardedPipeReader = new ClientDisconnectGuardedPipeReader((PipeReader)requestBody, cancellationToken);
                        bytesWritten = await _storeAdapter.AppendDataAsync(fileId, guardedPipeReader, cancellationToken);
                    }
                    else
#endif
                    {
                        var guardedStream = new ClientDisconnectGuardedReadOnlyStream((Stream)requestBody, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken));
                        bytesWritten = await _storeAdapter.AppendDataAsync(fileId, guardedStream, guardedStream.CancellationToken);
                        cancellationToken = guardedStream.CancellationToken;
                    }
                }
                catch (Exception ex)
                {
                    uploadOffset = await _storeAdapter.GetUploadOffsetAsync(fileId, default);

                    throw new TusIncompleteWriteException(uploadOffset, ex);
                }

                writeResult.UploadOffset = uploadOffset + bytesWritten;

                await validator.PostValidate(_storeAdapter, cancellationToken);

                if (_storeAdapter.Extensions.Expiration)
                {
                    if (options.Expiration is SlidingExpiration)
                    {
                        writeResult.FileExpires = options.GetSystemTime().Add(options.Expiration.Timeout);
                        await _storeAdapter.SetExpirationAsync(fileId, writeResult.FileExpires.Value, cancellationToken);
                    }
                    else
                    {
                        writeResult.FileExpires = await _storeAdapter.GetExpirationAsync(fileId, cancellationToken);
                    }
                }

                if (_storeAdapter.Extensions.Concatenation)
                {
                    writeResult.FileConcatenation = await _storeAdapter.GetUploadConcatAsync(fileId, cancellationToken);
                }

                writeResult.IsComplete = await _storeAdapter.GetUploadLengthAsync(fileId, cancellationToken) == writeResult.UploadOffset;
            }
            finally
            {
                await fileLock.ReleaseIfHeld();
            }
            return writeResult;
        }
    }
}
