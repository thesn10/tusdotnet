#if endpointrouting

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
        /// <summary>
        /// The maximum upload size to allow. Exceeding this limit will return a "413 Request Entity Too Large" error to the client.
        /// Set to null to allow any size. The size might still be restricted by the web server or operating system.
        /// This property will take precedence over <see cref="MaxAllowedUploadSizeInBytes" />.
        /// </summary>
        public long? MaxAllowedUploadSizeInBytes { get; set; } = null;

        /// <summary>
        /// Set the strategy to use when parsing metadata. Defaults to <see cref="MetadataParsingStrategy.AllowEmptyValues"/> for better compatibility with tus clients.
        /// Change to <see cref="MetadataParsingStrategy.Original"/> to use the old format.
        /// </summary>
        public MetadataParsingStrategy MetadataParsingStrategy { get; set; } = MetadataParsingStrategy.AllowEmptyValues;

        /// <summary>
        /// The storage profile to use when storing files.
        /// </summary>
        public string StorageProfile { get; set; }

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
    }
}
#endif
