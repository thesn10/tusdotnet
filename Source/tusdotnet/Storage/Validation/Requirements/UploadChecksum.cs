using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Exceptions;
using tusdotnet.Helpers;
using tusdotnet.Models;
using tusdotnet.Stores;

namespace tusdotnet.Storage.Validation
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
                        throw TusChecksumException.CouldNotParseHeader();
                    }

                    var checksumAlgorithms = (await store.GetSupportedAlgorithmsAsync(cancellationToken)).ToList();
                    if (!checksumAlgorithms.Contains(checksum.Algorithm))
                    {
                        throw TusChecksumException.UnsupportedAlgorithm(checksumAlgorithms);
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
                            await ForceStoreDiscardChunk(store);
                            throw TusChecksumException.CouldNotParseHeader();
                        }

                        var checksumAlgorithms = (await store.GetSupportedAlgorithmsAsync(cancellationToken)).ToList();
                        if (!checksumAlgorithms.Contains(checksum.Algorithm))
                        {
                            await ForceStoreDiscardChunk(store);
                            throw TusChecksumException.UnsupportedAlgorithm(checksumAlgorithms);
                        }
                    }

                    var success = await store.VerifyChecksumAsync(_fileId, checksum.Algorithm, checksum.Hash, cancellationToken);
                    if (!success)
                    {
                        throw TusChecksumException.ChecksumNotMatching();
                    }
                }
            }
        }

        /// <summary>
        /// Forces the store to discard the already written data
        /// </summary>
        public Task ForceStoreDiscardChunk(StoreAdapter store)
        {
            var checksum = ChecksumTrailerHelper.TrailingChecksumToUseIfRealTrailerIsFaulty;
            return store.VerifyChecksumAsync(_fileId, checksum.Algorithm, checksum.Hash, default);
        }
    }
}
