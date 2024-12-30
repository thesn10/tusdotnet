#nullable enable
using System.Threading.Tasks;
using tusdotnet.Controllers.Contexts.Tus2;
using tusdotnet.Storage.Tus2;

namespace tusdotnet.Tus2
{
    public class TusHandler
    {
        private Tus2StorageClient _storageFacade;
        private readonly ITus2ConfigurationManager _configurationManager;
        private readonly string? _storageConfigurationName;

        public TusHandler(Tus2StorageClient storageFacade)
        {
            _storageFacade = storageFacade;
        }

        public TusHandler(ITus2ConfigurationManager configurationManager, string? storageConfigurationName)
        {
            _configurationManager = configurationManager;
            _storageConfigurationName = storageConfigurationName;
        }

        public virtual bool AllowClientToDeleteFile { get; }

        public virtual async Task<UploadRetrievingProcedureResponse> RetrieveOffset(RetrieveOffsetContext context)
        {
            var storage = await GetStorageFacade();

            return await storage.RetrieveOffset(context);
        }

        public virtual async Task<CreateFileProcedureResponse> CreateFile(CreateFileContext context)
        {
            var storage = await GetStorageFacade();

            return await storage.CreateFile(context);
        }

        public virtual async Task<UploadTransferProcedureResponse> WriteData(WriteDataContext context)
        {
            var storage = await GetStorageFacade();

            return await storage.WriteData(context);
        }

        public virtual async Task<UploadCancellationProcedureResponse> Delete(DeleteContext context)
        {
            var storage = await GetStorageFacade();

            storage.Delete(context.Headers.UploadToken);

            return new UploadCancellationProcedureResponse()
            {
                DisconnectClient = false,
                UploadOffset = 0,
                ErrorMessage = "",
            };
        }

        public virtual Task FileComplete(FileCompleteContext context)
        {
            return Task.CompletedTask;
        }

        internal async Task<Tus2StorageClient> GetStorageFacade()
        {
            return _storageFacade ?? await CreateStorage();
        }

        private async Task<Tus2StorageClient> CreateStorage()
        {
            var storage = !string.IsNullOrEmpty(_storageConfigurationName)
                ? (await _configurationManager.GetNamedStorage(_storageConfigurationName))
                : (await _configurationManager.GetDefaultStorage());

            return (_storageFacade = storage);
        }
    }
}
