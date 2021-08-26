#if endpointrouting

using tusdotnet.Interfaces;
using tusdotnet.Models.Expiration;
using tusdotnet.Stores;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Options class used by <see cref="SimpleTusController"/> to configure the storage service
    /// </summary>
    public class StorageOptions
    {
        public ITusStore Store { get; set; }

        public ExpirationBase Expiration { get; set; }

        public void UseDiskStore(string directoryPath)
        {
            Store = new TusDiskStore(directoryPath);
        }
    }
}
#endif
