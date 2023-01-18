using System.IO;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Stores;
using tusdotnet.Storage.Validation;
using tusdotnet.Exceptions;
using tusdotnet.Storage.Handlers;
using tusdotnet.Routing;

#if pipelines
using System.IO.Pipelines;
#endif

namespace tusdotnet.Storage
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

        /// <summary>
        /// The StoreAdapter
        /// </summary>
        public StoreAdapter StoreAdapter => _storeAdapter;

        internal TusStorageClient(ITusStore store)
        {
            _storeAdapter = new StoreAdapter(store);
        }

        /// <summary>
        /// Creates a new TusStorageClient using an <see cref="ITusStore"/>. 
        /// If you want to reuse configurated <see cref="ITusStore"/>s, 
        /// use <see cref="ITusStorageClientProvider.Get(string)"/>
        /// </summary>
        public static TusStorageClient Create(ITusStore store)
        {
            return new TusStorageClient(store);
        }

        /// <summary>
        /// Creates a new file
        /// </summary>
        /// <exception cref="TusStoreException"></exception>
        public Task<CreateResult> Create(long uploadLength, string uploadMetadata, bool isPartialFile = false,
            CreateOptions? options = default, CancellationToken cancellationToken = default)
        {
            options ??= new CreateOptions();

            var createOp = new CreateOperationHandler(_storeAdapter);

            return createOp.Create(uploadLength, uploadMetadata, isPartialFile, null, options, cancellationToken);
        }

        /// <summary>
        /// Creates a file from multiple partial files
        /// </summary>
        public Task<CreateResult> Create(string[] partialFiles, string uploadMetadata,
            CreateOptions? options = default, CancellationToken cancellationToken = default)
        {
            options ??= new CreateOptions();

            var createOp = new CreateOperationHandler(_storeAdapter);

            return createOp.Create(0, uploadMetadata, false, partialFiles, options, cancellationToken);
        }

        /// <summary>
        /// Writes to a file
        /// </summary>
        /// <exception cref="TusFileAlreadyInUseException"></exception>
        /// <exception cref="TusFileNotFoundException"></exception>
        /// <exception cref="TusUploadLengthException"></exception>
        /// <exception cref="TusInvalidConcatException"></exception>
        /// <exception cref="TusChecksumException"></exception>
        /// <exception cref="TusFileExpiredException"></exception>
        /// <exception cref="TusInvalidOffsetException"></exception>
        /// <exception cref="TusFileAlreadyCompleteException"></exception>
        /// <exception cref="TusConfigurationException"></exception>
        /// <exception cref="TusStoreException"></exception>
        public Task<WriteResult> Write(string fileId, Stream requestStream, long uploadOffset, long? uploadLength, bool assumeFileCreated,
            WriteOptions? options = default, CancellationToken cancellationToken = default)
        {
            options ??= new WriteOptions();

            var writeOp = new WriteOperationHandler(_storeAdapter);

            return writeOp.Write(fileId, requestStream, uploadOffset, uploadLength, assumeFileCreated, options, cancellationToken);
        }

#if pipelines
        /// <inheritdoc cref="Write(string, Stream, long, long?, bool, WriteOptions, CancellationToken)"/>
        public Task<WriteResult> Write(string fileId, PipeReader requestReader, long uploadOffset, long? uploadLength, bool assumeFileCreated,
            WriteOptions? options = default, CancellationToken cancellationToken = default)
        {
            options ??= new WriteOptions();

            WriteOperationHandler writeOp = new WriteOperationHandler(_storeAdapter);

            return writeOp.Write(fileId, requestReader, uploadOffset, uploadLength, assumeFileCreated, options, cancellationToken);
        }
#endif

        /// <summary>
        /// Deletes a file
        /// </summary>
        /// <exception cref="TusFileNotFoundException"></exception>
        /// <exception cref="TusFileExpiredException"></exception>
        /// <exception cref="TusStoreException"></exception>
        public async Task Delete(string fileId, DeleteOptions? options = default, CancellationToken cancellationToken = default)
        {
            options ??= new DeleteOptions();

            var validator = new StorageValidator(
                new FileExist(fileId),
                new FileHasNotExpired(fileId));

            await validator.Validate(_storeAdapter, cancellationToken);

            var fileLock = await options.FileLockProvider.AquireLock(fileId);
            var hasLock = await fileLock.Lock();

            if (!hasLock)
            {
                throw new TusFileAlreadyInUseException(fileId);
            }

            try
            {
                await _storeAdapter.DeleteFileAsync(fileId, cancellationToken);
            }
            finally
            {
                await fileLock.ReleaseIfHeld();
            }
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
        public Task<GetFileInfoResult> GetFileInfo(string fileId, CancellationToken cancellationToken = default)
        {
            var getFileInfoOp = new GetFileInfoOperationHandler(_storeAdapter);
            return getFileInfoOp.GetFileInfo(fileId, cancellationToken);
        }

        /// <summary>
        /// Gets the supported features and extensions
        /// </summary>
        public Task<FeatureSupportContext> GetExtensionInfo(CancellationToken cancellationToken = default)
        {
            var featureSupport = FeatureSupportContext.FromStoreAdapter(_storeAdapter);

            return Task.FromResult(featureSupport);
        }
    }
}