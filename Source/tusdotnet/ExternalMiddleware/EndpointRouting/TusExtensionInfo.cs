using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Stores;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public class TusExtensionInfo
    {
        public StoreExtensions SupportedExtensions { get; set; }
        public StoreFeatures SupportedFeatures { get; set; }


        private IEnumerable<string> _supportedChecksumCache;
        public async Task<IEnumerable<string>> GetSupportedChecksumAlgorithms(CancellationToken cancellationToken = default)
        {
            if (_supportedChecksumCache == null)
            {
                if (GetSupportedChecksumAlgorithmsFunc != null)
                {
                    _supportedChecksumCache = await GetSupportedChecksumAlgorithmsFunc(cancellationToken);
                }
                else
                {
                    _supportedChecksumCache = new List<string>(0);
                }
            }
            return _supportedChecksumCache;
        }
        public Func<CancellationToken, Task<IEnumerable<string>>> GetSupportedChecksumAlgorithmsFunc { get; set; }
    }
}
