#if endpointrouting

using tusdotnet.Models;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusEndpointOptions : ITusEndpointOptions
    {
        /// <inheritdoc/>
        public string StorageProfile { get; set; }

        /// <inheritdoc/>
        public long? MaxAllowedUploadSizeInBytes { get; set; } = null;

        /// <inheritdoc/>
        public MetadataParsingStrategy MetadataParsingStrategy { get; set; } = MetadataParsingStrategy.AllowEmptyValues;
    }
}
#endif
