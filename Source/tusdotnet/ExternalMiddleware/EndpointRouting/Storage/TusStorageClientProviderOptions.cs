using System.Collections.Generic;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusStorageClientProviderOptions
    {
        public string? DefaultProfile { get; set; } = null;
        public Dictionary<string, ITusStorageProfile> Profiles { get; set; } = new Dictionary<string, ITusStorageProfile>();
    }
}
