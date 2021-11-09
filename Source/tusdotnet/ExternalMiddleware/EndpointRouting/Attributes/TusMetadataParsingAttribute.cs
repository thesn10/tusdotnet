using System;
using tusdotnet.Models;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusMetadataParsingAttribute : Attribute
    {
        public TusMetadataParsingAttribute(MetadataParsingStrategy strategy)
        {
            Strategy = strategy;
        }

        public MetadataParsingStrategy Strategy { get; }
    }
}
