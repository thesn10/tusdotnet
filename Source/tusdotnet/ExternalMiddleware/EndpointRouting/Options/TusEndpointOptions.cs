#if endpointrouting

using tusdotnet.Models;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusEndpointOptions
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
    }
}
#endif
