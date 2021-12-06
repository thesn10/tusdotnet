using System;
using tusdotnet.Models;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Set the strategy to use when parsing metadata. Defaults to <see cref="MetadataParsingStrategy.AllowEmptyValues"/> for better compatibility with tus clients.
    /// Change to <see cref="MetadataParsingStrategy.Original"/> to use the old format.
    /// </summary>
    public class TusMetadataParsingAttribute : Attribute
    {
        /// <summary>
        /// Set the strategy to use when parsing metadata. Defaults to <see cref="MetadataParsingStrategy.AllowEmptyValues"/> for better compatibility with tus clients.
        /// Change to <see cref="MetadataParsingStrategy.Original"/> to use the old format.
        /// </summary>
        public TusMetadataParsingAttribute(MetadataParsingStrategy strategy)
        {
            Strategy = strategy;
        }

        /// <summary>
        /// The metadata parsing strategy
        /// </summary>
        public MetadataParsingStrategy Strategy { get; }
    }
}
