#if endpointrouting

using System;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Models.Expiration;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class CreateOptions
    {
        /// <summary>
        /// The store to use when storing files.
        /// </summary>
        public ITusStore Store { get; set; }

        /// <summary>
        /// Set an expiration time where incomplete files can no longer be updated.
        /// This value can either be <c>AbsoluteExpiration</c> or <c>SlidingExpiration</c>.
        /// Absolute expiration will be saved per file when the file is created.
        /// Sliding expiration will be saved per file when the file is created and updated on each time the file is updated.
        /// Setting this property to null will disable file expiration.
        /// </summary>
        public ExpirationBase Expiration { get; set; }

        private DateTimeOffset? _systemTime;

        internal void MockSystemTime(DateTimeOffset systemTime)
        {
            _systemTime = systemTime;
        }

        internal DateTimeOffset GetSystemTime()
        {
            return _systemTime ?? DateTimeOffset.UtcNow;
        }

        internal void Validate()
        {
            if (Store == null)
            {
                throw new TusConfigurationException($"{nameof(Store)} cannot be null.");
            }
        }
    }
}
#endif
