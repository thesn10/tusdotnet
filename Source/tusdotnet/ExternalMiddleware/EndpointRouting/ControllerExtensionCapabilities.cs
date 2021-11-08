#if endpointrouting

using System.Collections.Generic;
using tusdotnet.Stores;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusExtensionInfo
    {
        public StoreExtensions SupportedExtensions { get; set; }
        public List<string> SupportedChecksumAlgorithms { get; set; } = new List<string>();
    }
}

#endif
