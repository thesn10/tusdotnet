#if endpointrouting

using tusdotnet.FileLocks;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;
using tusdotnet.Models.Expiration;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Options class used by <see cref="EventsBasedTusController"/>
    /// </summary>
    public class TusSimpleEndpointOptions : ITusEndpointOptions
    {
        /// <inheritdoc/>
        public long? MaxAllowedUploadSizeInBytes { get; set; } = null;

        /// <inheritdoc/>
        public MetadataParsingStrategy MetadataParsingStrategy { get; set; } = MetadataParsingStrategy.AllowEmptyValues;

        /// <inheritdoc/>
        public string StorageProfile { get; set; } = "default";

        /// <summary>
        /// Set an expiration time where incomplete files can no longer be updated.
        /// This value can either be <c>AbsoluteExpiration</c> or <c>SlidingExpiration</c>.
        /// Absolute expiration will be saved per file when the file is created.
        /// Sliding expiration will be saved per file when the file is created and updated on each time the file is updated.
        /// Setting this property to null will disable file expiration.
        /// </summary>
        public ExpirationBase Expiration { get; set; }

        /// <summary>
        /// Callbacks to run during different stages of the tusdotnet pipeline.
        /// </summary>
        public Events Events { get; set; }

        /// <summary>
        /// Lock provider to use when locking to prevent files from being accessed while the file is still in use.
        /// Defaults to using in-memory locks.
        /// </summary>
        public ITusFileLockProvider FileLockProvider { get; set; } = InMemoryFileLockProvider.Instance;

#if pipelines

        /// <summary>
        /// Use the incoming request's PipeReader instead of the stream to read data from the client.
        /// This is only available on .NET Core 3.1 or later and if the store supports it through the ITusPipelineStore interface.
        /// </summary>
        public bool UsePipelinesIfAvailable { get; set; }

#endif
    }
}
#endif
