using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Models;
using tusdotnet.Stores;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.Validation.Storage
{
    internal sealed class UploadChecksum : StorageRequirement, IStorageRequirementWithPostValidation
    {
        private readonly Func<Task<Checksum>> _getChecksum;
        private readonly string _fileId;

        private bool hasChecked = false;

        public UploadChecksum(Func<Task<Checksum>> getChecksum, string fileId)
        {
            _getChecksum = getChecksum;
            _fileId = fileId;
        }

        public override async Task Validate(StoreAdapter store, CancellationToken cancellationToken)
        {
            if (store.Extensions.Checksum)
            {
                var checksum = await _getChecksum?.Invoke();

                if (checksum != null)
                {
                    if (!checksum.IsValid)
                    {
                        TusChecksumException.ThrowCouldNotParseHeader();
                    }

                    var checksumAlgorithms = (await store.GetSupportedAlgorithmsAsync(cancellationToken)).ToList();
                    if (!checksumAlgorithms.Contains(checksum.Algorithm))
                    {
                        TusChecksumException.ThrowUnsupportedAlgorithm(checksumAlgorithms);
                    }

                    hasChecked = true;
                }
            }
        }

        public async Task PostValidate(StoreAdapter store, CancellationToken cancellationToken)
        {
            if (store.Extensions.Checksum)
            {
                var checksum = await _getChecksum?.Invoke();

                if (checksum != null)
                {
                    if (!hasChecked)
                    {
                        if (!checksum.IsValid)
                        {
                            TusChecksumException.ThrowCouldNotParseHeader();
                        }

                        var checksumAlgorithms = (await store.GetSupportedAlgorithmsAsync(cancellationToken)).ToList();
                        if (!checksumAlgorithms.Contains(checksum.Algorithm))
                        {
                            TusChecksumException.ThrowUnsupportedAlgorithm(checksumAlgorithms);
                        }
                    }

                    var success = await store.VerifyChecksumAsync(_fileId, checksum.Algorithm, checksum.Hash, cancellationToken);
                    if (!success)
                    {
                        TusChecksumException.ThrowChecksumNotMatching();
                    }
                }
            }
        }
    }
}
