using tusdotnet.ExternalMiddleware.EndpointRouting;
using tusdotnet.Interfaces;

namespace AspNetCore_net6._0_TestApp.Services
{
    public class ExpiredFilesCleanupService : IHostedService, IDisposable
    {
        private readonly ILogger<ExpiredFilesCleanupService> _logger;
        private readonly ITusStorageClientProvider _storageClientProvider;
        private Timer _timer;
        private TimeSpan _timeout;
        private TusStorageClient _storageClient;

        public ExpiredFilesCleanupService(ILogger<ExpiredFilesCleanupService> logger, ITusStorageClientProvider storageClientProvider)
        {
            _logger = logger;
            _storageClientProvider = storageClientProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _timeout = TimeSpan.FromMinutes(Constants.FileExpirationInMinutes);
            _storageClient = await _storageClientProvider.Default();

            await RunCleanup(cancellationToken);
            _timer = new Timer(async (e) => await RunCleanup((CancellationToken)e), cancellationToken, TimeSpan.Zero, _timeout);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private async Task RunCleanup(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Running cleanup job...");
                var numberOfRemovedFiles = await (_storageClient.Store as ITusExpirationStore).RemoveExpiredFilesAsync(cancellationToken);
                _logger.LogInformation($"Removed {numberOfRemovedFiles} expired files. Scheduled to run again in {_timeout.TotalMilliseconds} ms");
            }
            catch (Exception exc)
            {
                _logger.LogWarning("Failed to run cleanup job: " + exc.Message);
            }
        }
    }
}