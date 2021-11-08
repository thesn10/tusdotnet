#if endpointrouting

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.ExternalMiddleware.EndpointRouting.Validation;
using tusdotnet.ExternalMiddleware.EndpointRouting.Validation.Storage;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Models.Expiration;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Client to access tus storage. Provided by <see cref="ITusStorageClientProvider"/>
    /// </summary>
    public sealed class TusStorageClient
    {
        private readonly StoreAdapter _storeAdapter;

        /// <summary>
        /// The underlying Store
        /// </summary>
        public ITusStore Store => _storeAdapter.Store;

        internal TusStorageClient(ITusStore store)
        {
            _storeAdapter = new StoreAdapter(store);
        }

        /// <summary>
        /// Creates a new file
        /// </summary>
        /// <exception cref="TusStoreException"></exception>
        public async Task<CreateResult> Create(long uploadLength, string uploadMetadata, 
            CreateOptions options = default, CancellationToken cancellationToken = default)
        {
            options ??= new CreateOptions();

            var createResult = new CreateResult();

            createResult.FileId = await _storeAdapter.CreateFileAsync(uploadLength, uploadMetadata, cancellationToken);

            if (_storeAdapter.Extensions.Expiration && options.Expiration != null && uploadLength != 0)
            {
                // Expiration is only used when patching files so if the file is not empty and we did not have any data in the current request body,
                // we need to update the header here to be able to keep track of expiration for this file.
                createResult.FileExpires = options.GetSystemTime().Add(options.Expiration.Timeout);
                await _storeAdapter.SetExpirationAsync(createResult.FileId, createResult.FileExpires.Value, cancellationToken);
            }

            return createResult;
        }

        /// <summary>
        /// Writes to a file
        /// </summary>
        /// <exception cref="TusFileAlreadyInUseException"></exception>
        /// <exception cref="TusFileNotFoundException"></exception>
        /// <exception cref="TusDeferLengthException"></exception>
        /// <exception cref="TusInvalidConcatException"></exception>
        /// <exception cref="TusUnsupportedChecksumAlgorithmException"></exception>
        /// <exception cref="TusFileExpiredException"></exception>
        /// <exception cref="TusInvalidOffsetException"></exception>
        /// <exception cref="TusFileAlreadyCompleteException"></exception>
        /// <exception cref="TusConfigurationException"></exception>
        /// <exception cref="TusStoreException"></exception>
        public async Task<WriteResult> Write(string fileId, Stream requestStream, long uploadOffset, long? uploadLength, 
            WriteOptions options = default, CancellationToken cancellationToken = default)
        {
            options ??= new WriteOptions();

            var checksum = options.GetChecksumProvidedByClient?.Invoke();

            var validator = new StorageValidator(
                new FileExist(fileId),
                new UploadLengthForWriteFile(fileId, uploadLength),
                new UploadConcatForWriteFile(fileId),
                new UploadChecksum(checksum),
                new FileHasNotExpired(fileId),
                new RequestOffsetMatchesFileOffset(uploadOffset, fileId),
                new FileIsNotCompleted(fileId));

            await validator.Validate(_storeAdapter, cancellationToken);

            var writeResult = new WriteResult();
            var fileLock = await options.FileLockProvider.AquireLock(fileId);
            var hasLock = await fileLock.Lock();

            if (!hasLock)
            {
                throw new TusFileAlreadyInUseException(fileId);
            }

            if (uploadLength.HasValue && _storeAdapter.Extensions.CreationDeferLength)
            {
                await _storeAdapter.SetUploadLengthAsync(fileId, uploadLength.Value, cancellationToken);
            }

            var guardedStream = new ClientDisconnectGuardedReadOnlyStream(requestStream, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken));
            var bytesWritten = await _storeAdapter.AppendDataAsync(fileId, guardedStream, guardedStream.CancellationToken);
            await fileLock.ReleaseIfHeld();

            writeResult.UploadOffset = uploadOffset + bytesWritten;

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

            if (_storeAdapter.Extensions.Checksum)
            {
                if (checksum != null)
                    writeResult.ChecksumMatches = await _storeAdapter.VerifyChecksumAsync(fileId, checksum.Algorithm, checksum.Hash, cancellationToken);
            }

            writeResult.IsComplete = await _storeAdapter.GetUploadLengthAsync(fileId, cancellationToken) == writeResult.UploadOffset;
            return writeResult;
        }

        /// <summary>
        /// Deletes a file
        /// </summary>
        /// <exception cref="TusFileNotFoundException"></exception>
        /// <exception cref="TusFileExpiredException"></exception>
        /// <exception cref="TusStoreException"></exception>
        public async Task Delete(string fileId, CancellationToken cancellationToken = default)
        {
            var validator = new StorageValidator(
                new FileExist(fileId),
                new FileHasNotExpired(fileId));

            await validator.Validate(_storeAdapter, cancellationToken);

            await _storeAdapter.DeleteFileAsync(fileId, cancellationToken);
        }

        /// <summary>
        /// Returns <see cref="ITusFile"/> which can be used to read the file if it is supported
        /// </summary>
        public Task<ITusFile> Get(string fileId, CancellationToken cancellationToken = default)
        {
            if (!_storeAdapter.Features.Readable) return null;

            return _storeAdapter.GetFileAsync(fileId, cancellationToken);
        }

        /// <summary>
        /// Gets the file info
        /// </summary>
        public async Task<GetFileInfoResult> GetFileInfo(string fileId, CancellationToken cancellationToken = default)
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
                getFileInfoResult.UploadConcat = await _storeAdapter.GetUploadConcatAsync(fileId, cancellationToken);
            }

            return getFileInfoResult;
        }

        /// <summary>
        /// Gets the extension information
        /// </summary>
        public async Task<TusExtensionInfo> GetExtensionInfo(CancellationToken cancellationToken = default)
        {
            TusExtensionInfo extensionInfo = new TusExtensionInfo()
            {
                SupportedExtensions = _storeAdapter.Extensions,
            };

            if (_storeAdapter.Extensions.Checksum)
            {
                var algos = await _storeAdapter.GetSupportedAlgorithmsAsync(cancellationToken);
                extensionInfo.SupportedChecksumAlgorithms.AddRange(algos);
            }

            return extensionInfo;
        }

        /// <inheritdoc cref="Create(long, string, CreateOptions, CancellationToken)"/>
        public Task<CreateResult> Create(CreateContext context, CreateOptions options = default, CancellationToken cancellationToken = default)
        {
            return Create(context.UploadLength, context.UploadMetadata, options, cancellationToken);
        }

        /// <inheritdoc cref="Write(string, Stream, long, long?, WriteOptions, CancellationToken)"/>
        public Task<WriteResult> Write(WriteContext context, WriteOptions options = default, CancellationToken cancellationToken = default)
        {
            return Write(context.FileId, context.RequestStream, context.UploadOffset, context.UploadLength, new WriteOptions()
            {
                Expiration = options.Expiration,
                FileLockProvider = options.FileLockProvider,
                GetChecksumProvidedByClient = options.GetChecksumProvidedByClient ?? context.GetChecksumProvidedByClient,
            }, cancellationToken);
        }

        /// <inheritdoc cref="Delete(string, CancellationToken)"/>
        public Task Delete(DeleteContext context, CancellationToken cancellationToken = default)
        {
            return Delete(context.FileId, cancellationToken);
        }

        /// <inheritdoc cref="GetFileInfo(string, CancellationToken)"/>
        public Task<GetFileInfoResult> GetFileInfo(GetFileInfoContext context, CancellationToken cancellationToken = default)
        {
            return GetFileInfo(context.FileId, cancellationToken);
        }
    }
}

#endif