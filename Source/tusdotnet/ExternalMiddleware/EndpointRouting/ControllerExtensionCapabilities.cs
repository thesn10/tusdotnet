#if endpointrouting

using System.Collections.Generic;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class ControllerCapabilities
    {
        public List<string> SupportedExtensions { get; set; } = new List<string>();
        public List<string> SupportedChecksumAlgorithms { get; set; } = new List<string>();
    }
}

#endif
