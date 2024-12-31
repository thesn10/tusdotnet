using System;
using System.Threading.Tasks;
using tusdotnet.FileLocks;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Models.Expiration;

namespace tusdotnet.Storage
{
    /// <summary>
    /// Options for a write operation
    /// </summary>
    public class WriteOptions
    {
        /// <summary>
        /// Set an expiration time where incomplete files can no longer be updated.
        /// This value can either be <c>AbsoluteExpiration</c> or <c>SlidingExpiration</c>.
        /// Absolute expiration will be saved per file when the file is created.
        /// Sliding expiration will be saved per file when the file is created and updated on each time the file is updated.
        /// Setting this property to null will disable file expiration.
        /// </summary>
        public ExpirationBase? Expiration { get; set; } = null;

        /// <summary>
        /// Lock provider to use when locking to prevent files from being accessed while the file is still in use.
        /// Defaults to using in-memory locks.
        /// </summary>
        public ITusFileLockProvider FileLockProvider { get; set; } = InMemoryFileLockProvider.Instance;

        /// <summary>
        /// Callback to support checksum headers and trailers
        /// </summary>
        public Func<Task<Checksum>> GetChecksumProvidedByClient { get; set; } = null;

#if pipelines

        /// <summary>
        /// Use the incoming request's PipeReader instead of the stream to read data from the client.
        /// This is only available on .NET Core 3.1 or later and if the store supports it through the ITusPipelineStore interface.
        /// </summary>
        public bool UsePipelinesIfAvailable { get; set; }

#endif

        private DateTimeOffset? _systemTime;

        internal void MockSystemTime(DateTimeOffset? systemTime)
        {
            _systemTime = systemTime;
        }

        internal DateTimeOffset GetSystemTime()
        {
            return _systemTime ?? DateTimeOffset.UtcNow;
        }
    }
}
