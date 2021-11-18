using System.IO;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.ExternalMiddleware.EndpointRouting.StorageOperations;
using tusdotnet.ExternalMiddleware.EndpointRouting.Validation;
using tusdotnet.ExternalMiddleware.EndpointRouting.Validation.Storage;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Stores;
using tusdotnet.Models.Concatenation;

#if pipelines
using System.IO.Pipelines;
#endif

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
        public Task<CreateResult> Create(long uploadLength, string uploadMetadata, bool isPartialFile = false,
            CreateOptions options = default, CancellationToken cancellationToken = default)
        {
            options ??= new CreateOptions();

            var createOp = new CreateOperationHandler(_storeAdapter);

            return createOp.Create(uploadLength, uploadMetadata, isPartialFile, null, options, cancellationToken);
        }

        /// <summary>
        /// Creates a file from multiple partial files
        /// </summary>
        public Task<CreateResult> Create(string[] partialFiles, string uploadMetadata,
            CreateOptions options = default, CancellationToken cancellationToken = default)
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
        /// <exception cref="TusDeferLengthException"></exception>
        /// <exception cref="TusInvalidConcatException"></exception>
        /// <exception cref="TusUnsupportedChecksumAlgorithmException"></exception>
        /// <exception cref="TusFileExpiredException"></exception>
        /// <exception cref="TusInvalidOffsetException"></exception>
        /// <exception cref="TusFileAlreadyCompleteException"></exception>
        /// <exception cref="TusConfigurationException"></exception>
        /// <exception cref="TusStoreException"></exception>
        public Task<WriteResult> Write(string fileId, Stream requestStream, long uploadOffset, long? uploadLength, 
            WriteOptions options = default, CancellationToken cancellationToken = default)
        {
            options ??= new WriteOptions();

            var writeOp = new WriteOperationHandler(_storeAdapter);

            return writeOp.Write(fileId, requestStream, uploadOffset, uploadLength, options, cancellationToken);
        }

#if pipelines
        /// <inheritdoc cref="Write(string, Stream, long, long?, WriteOptions, CancellationToken)"/>
        public Task<WriteResult> Write(string fileId, PipeReader requestReader, long uploadOffset, long? uploadLength,
            WriteOptions options = default, CancellationToken cancellationToken = default)
        {
            options ??= new WriteOptions();

            WriteOperationHandler writeOp = new WriteOperationHandler(_storeAdapter);

            return writeOp.Write(fileId, requestReader, uploadOffset, uploadLength, options, cancellationToken);
        }
#endif

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
        public Task<GetFileInfoResult> GetFileInfo(string fileId, CancellationToken cancellationToken = default)
        {
            var getFileInfoOp = new GetFileInfoOperationHandler(_storeAdapter);
            return getFileInfoOp.GetFileInfo(fileId, cancellationToken);
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

        /// <inheritdoc cref="Create(long, string, bool, CreateOptions, CancellationToken)"/>
        public Task<CreateResult> Create(CreateContext context, CreateOptions options = default, CancellationToken cancellationToken = default)
        {
            if (context.FileConcat is FileConcatPartial)
            {
                return Create(context.UploadLength, context.UploadMetadata, true, options, cancellationToken);
            }
            else if (context.FileConcat is FileConcatFinal final)
            {
                return Create(final.Files, context.UploadMetadata, options, cancellationToken);
            }
            else 
            { 
                return Create(context.UploadLength, context.UploadMetadata, false, options, cancellationToken); 
            }
        }

        /// <inheritdoc cref="Write(string, Stream, long, long?, WriteOptions, CancellationToken)"/>
        public Task<WriteResult> Write(WriteContext context, WriteOptions options = default, CancellationToken cancellationToken = default)
        {
#if pipelines
            if (_storeAdapter.Features.Pipelines && options.UsePipelinesIfAvailable)
            {
                return Write(context.FileId, context.RequestReader, context.UploadOffset, context.UploadLength, new WriteOptions()
                {
                    Expiration = options.Expiration,
                    FileLockProvider = options.FileLockProvider,
                    GetChecksumProvidedByClient = options.GetChecksumProvidedByClient ?? context.GetChecksumProvidedByClient,
                }, cancellationToken);
            }
            else
#endif
            {
                return Write(context.FileId, context.RequestStream, context.UploadOffset, context.UploadLength, new WriteOptions()
                {
                    Expiration = options.Expiration,
                    FileLockProvider = options.FileLockProvider,
                    GetChecksumProvidedByClient = options.GetChecksumProvidedByClient ?? context.GetChecksumProvidedByClient,
                }, cancellationToken);
            }
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