using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Models;
using tusdotnet.Stores;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.Validation.Storage
{
    internal sealed class UploadChecksum : StorageRequirement
    {
        private Checksum RequestChecksum { get; }

        public UploadChecksum() : this(null)
        {
        }

        public UploadChecksum(Checksum requestChecksum)
        {
            RequestChecksum = requestChecksum;
        }

        public override async Task Validate(StoreAdapter store, CancellationToken cancellationToken)
        {
            var providedChecksum = RequestChecksum;

            if (store.Extensions.Checksum && providedChecksum != null)
            {
                var checksumAlgorithms = (await store.GetSupportedAlgorithmsAsync(cancellationToken)).ToList();
                if (!checksumAlgorithms.Contains(providedChecksum.Algorithm))
                {
                    throw new TusUnsupportedChecksumAlgorithmException(
                        $"Unsupported checksum algorithm. Supported algorithms are: {string.Join(",", checksumAlgorithms)}");
                }
            }
        }
    }
}
