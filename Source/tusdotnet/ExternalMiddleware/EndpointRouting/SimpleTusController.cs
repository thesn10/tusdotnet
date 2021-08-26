#if endpointrouting

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.Interfaces;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// A builtin tus controller for lazy people
    /// </summary>
    [TusController]
    internal class SimpleTusController : TusControllerBase, IControllerWithOptions<StorageOptions>
    {
        private readonly ILogger<SimpleTusController> _logger;
        private readonly TusStorageService _storage;

        public StorageOptions Options { get; set; }

        public SimpleTusController(TusStorageService storage, ILogger<SimpleTusController> logger)
        {
            _storage = storage;
            _logger = logger;
        }

        public override async Task<ITusCreateActionResult> Create(CreateContext context, CancellationToken cancellation)
        {
            var createResult = await _storage.Create(context, new CreateOptions()
            {
                Expiration = Options.Expiration,
                Store = Options.Store,

            }, cancellation);

            _logger.LogInformation($"File created with id {createResult.FileId}");

            return CreateOk(createResult);
        }


        public override async Task<ITusWriteActionResult> Write(WriteContext context, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Started writing file {context.FileId} at offset {context.UploadOffset}");

            var writeResult = await _storage.Write(context, new WriteOptions()
            {
                Expiration = Options.Expiration,
                Store = Options.Store,

            }, cancellationToken);

            _logger.LogInformation($"Done writing file {context.FileId}. New offset: {context.UploadOffset}");

            return WriteOk(writeResult);
        }

        public override Task<ITusCompletedActionResult> FileCompleted(FileCompletedContext context, CancellationToken cancellation)
        {
            _logger.LogInformation($"Upload of file {context.FileId} is complete!");
            return base.FileCompleted(context, cancellation);
        }

        public override async Task<ITusInfoActionResult> GetFileInfo(GetFileInfoContext context, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Getting file info of file {context.FileId}");

            var info = await _storage.GetFileInfo(context, new GetFileInfoOptions()
            {
                Store = Options.Store,

            }, cancellationToken);

            return FileInfoOk(info);
        }

        public override async Task<ControllerCapabilities> GetCapabilities()
        {
            ControllerCapabilities capabilities = new ControllerCapabilities();

            var storeCapabilities = StoreAdapter.GetCapabilities(Options.Store.GetType());
            capabilities.SupportedExtensions.AddRange(storeCapabilities);

            if (storeCapabilities.Contains(ExtensionConstants.Checksum) && 
                Options.Store is ITusChecksumStore checksumStore)
            {
                var algos = await checksumStore.GetSupportedAlgorithmsAsync(default);

                capabilities.SupportedChecksumAlgorithms.AddRange(algos);
            }

            return capabilities;
        }
    }
}
#endif
