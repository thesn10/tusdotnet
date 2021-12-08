using System;
using tusdotnet.Models.Expiration;

namespace tusdotnet.Storage
{
    /// <summary>
    /// Options for a create operation
    /// </summary>
    public class CreateOptions
    {
        /// <summary>
        /// Set an expiration time where incomplete files can no longer be updated.
        /// This value can either be <c>AbsoluteExpiration</c> or <c>SlidingExpiration</c>.
        /// Absolute expiration will be saved per file when the file is created.
        /// Sliding expiration will be saved per file when the file is created and updated on each time the file is updated.
        /// Setting this property to null will disable file expiration.
        /// </summary>
        public ExpirationBase Expiration { get; set; }

        /// <summary>
        /// Maximum final size of a file after concatenation
        /// Set to null to allow any size. The size might still be restricted by the operating system.
        /// </summary>
        public long? MaxConcatFileSize { get; set; } = null;

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
