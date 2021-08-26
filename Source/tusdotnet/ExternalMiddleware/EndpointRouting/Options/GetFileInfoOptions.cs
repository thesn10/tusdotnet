#if endpointrouting

using tusdotnet.Interfaces;
using tusdotnet.Models;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class GetFileInfoOptions
    {
        /// <summary>
        /// The store to use when storing files.
        /// </summary>
        public ITusStore Store { get; set; }

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