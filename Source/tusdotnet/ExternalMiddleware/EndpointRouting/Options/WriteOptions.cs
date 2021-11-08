#if endpointrouting

using System;
using tusdotnet.FileLocks;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Models.Expiration;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
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
        public ExpirationBase Expiration { get; set; } = null;

        /// <summary>
        /// Lock provider to use when locking to prevent files from being accessed while the file is still in use.
        /// Defaults to using in-memory locks.
        /// </summary>
        public ITusFileLockProvider FileLockProvider { get; set; } = InMemoryFileLockProvider.Instance;

        public Func<Checksum> GetChecksumProvidedByClient { get; set; } = null;

        private DateTimeOffset? _systemTime;

        internal void MockSystemTime(DateTimeOffset systemTime)
        {
            _systemTime = systemTime;
        }

        internal DateTimeOffset GetSystemTime()
        {
            return _systemTime ?? DateTimeOffset.UtcNow;
        }
    }
}

#endif
