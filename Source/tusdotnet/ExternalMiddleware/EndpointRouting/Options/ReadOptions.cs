#if endpointrouting

using tusdotnet.Interfaces;
using tusdotnet.Models;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class ReadOptions
    {
        public string FileId { get; set; }
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