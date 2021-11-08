#if endpointrouting

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using tusdotnet.Interfaces;
using tusdotnet.Models.Concatenation;
using tusdotnet.Constants;
using tusdotnet.Stores;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public sealed class StoreAdapter : ITusStore, ITusCreationStore, ITusExpirationStore, ITusChecksumStore
    {
        private readonly ITusStore _store;
        // TODO: Make into a property and require the property to be set and throw an invalid operation exception otherwise? Prevents null refs.
        private readonly ITusCreationStore _creationStore;
        private readonly ITusExpirationStore _expirationStore;
        private readonly ITusChecksumStore _checksumStore;
        private readonly ITusConcatenationStore _concatStore;
        private readonly ITusCreationDeferLengthStore _creationDeferLengthStore;
        private readonly ITusReadableStore _readableStore;
        private readonly ITusTerminationStore _terminationStore;

        public StoreExtensions Extensions { get; }
        public StoreFeatures Features { get; }
        public ITusStore Store => _store;

        public StoreAdapter(ITusStore store)
        {
            _store = store;

            Extensions = new();
            Features = new();

            if (store is ITusCreationStore creationStore)
            {
                _creationStore = creationStore;
                Extensions.Creation = true;
            }

            if (store is ITusExpirationStore expirationStore)
            {
                _expirationStore = expirationStore;
                Extensions.Expiration = true;
            }

            if (store is ITusChecksumStore checksumStore)
            {
                _checksumStore = checksumStore;
                Extensions.Checksum = true;
            }

            if (store is ITusConcatenationStore concatStore)
            {
                _concatStore = concatStore;
                Extensions.Concatenation = true;
            }

            if (store is ITusCreationDeferLengthStore creationDeferLengthStore)
            {
                _creationDeferLengthStore = creationDeferLengthStore;
                Extensions.CreationDeferLength = true;
            }

            if (store is ITusTerminationStore terminationStore)
            {
                _terminationStore = terminationStore;
                Extensions.Termination = true;
            }

            if (store is ITusReadableStore readableStore)
            {
                _readableStore = readableStore;
                Features.Readable = true;
            }
        }

        public static List<string> GetCapabilities(Type storeType)
        {
            List<string> capabilities = new List<string>();

            var interfaces = storeType.GetInterfaces();

            if (interfaces.Contains(typeof(ITusCreationStore)))
            {
                capabilities.Add(ExtensionConstants.Creation);
                capabilities.Add(ExtensionConstants.CreationWithUpload);
            }
            if (interfaces.Contains(typeof(ITusTerminationStore)))
            {
                capabilities.Add(ExtensionConstants.Termination);
            }
            if (interfaces.Contains(typeof(ITusChecksumStore)))
            {
                capabilities.Add(ExtensionConstants.Checksum);
            }
            if (interfaces.Contains(typeof(ITusConcatenationStore)))
            {
                capabilities.Add(ExtensionConstants.Concatenation);
            }
            if (interfaces.Contains(typeof(ITusExpirationStore)))
            {
                capabilities.Add(ExtensionConstants.Expiration);
            }
            if (interfaces.Contains(typeof(ITusCreationDeferLengthStore)))
            {
                capabilities.Add(ExtensionConstants.CreationDeferLength);
            }

            return capabilities;
        }

        public Task<long> AppendDataAsync(string fileId, Stream stream, CancellationToken cancellationToken)
        {
            return _store.AppendDataAsync(fileId, stream, cancellationToken);
        }

        public Task DeleteFileAsync(string fileId, CancellationToken cancellationToken)
        {
            return _terminationStore.DeleteFileAsync(fileId, cancellationToken);
        }

        public Task<string> CreateFileAsync(long uploadLength, string metadata, CancellationToken cancellationToken)
        {
            return _creationStore.CreateFileAsync(uploadLength, metadata, cancellationToken);
        }

        public Task<bool> FileExistAsync(string fileId, CancellationToken cancellationToken)
        {
            return _store.FileExistAsync(fileId, cancellationToken);
        }

        public Task<long?> GetUploadLengthAsync(string fileId, CancellationToken cancellationToken)
        {
            return _store.GetUploadLengthAsync(fileId, cancellationToken);
        }

        public Task<string> GetUploadMetadataAsync(string fileId, CancellationToken cancellationToken)
        {
            return _creationStore.GetUploadMetadataAsync(fileId, cancellationToken);
        }

        public Task<long> GetUploadOffsetAsync(string fileId, CancellationToken cancellationToken)
        {
            return _store.GetUploadOffsetAsync(fileId, cancellationToken);
        }

        public Task SetExpirationAsync(string fileId, DateTimeOffset expires, CancellationToken cancellationToken)
        {
            return _expirationStore.SetExpirationAsync(fileId, expires, cancellationToken);
        }

        public Task<DateTimeOffset?> GetExpirationAsync(string fileId, CancellationToken cancellationToken)
        {
            return _expirationStore.GetExpirationAsync(fileId, cancellationToken);
        }

        public Task<IEnumerable<string>> GetExpiredFilesAsync(CancellationToken cancellationToken)
        {
            return _expirationStore.GetExpiredFilesAsync(cancellationToken);
        }

        public Task<int> RemoveExpiredFilesAsync(CancellationToken cancellationToken)
        {
            return _expirationStore.RemoveExpiredFilesAsync(cancellationToken);
        }

        public Task<IEnumerable<string>> GetSupportedAlgorithmsAsync(CancellationToken cancellationToken)
        {
            return _checksumStore.GetSupportedAlgorithmsAsync(cancellationToken);
        }

        public Task<bool> VerifyChecksumAsync(string fileId, string algorithm, byte[] checksum, CancellationToken cancellationToken)
        {
            return _checksumStore.VerifyChecksumAsync(fileId, algorithm, checksum, cancellationToken);
        }

        public Task<FileConcat> GetUploadConcatAsync(string fileId, CancellationToken cancellationToken)
        {
            return _concatStore.GetUploadConcatAsync(fileId, cancellationToken);
        }

        public Task<string> CreatePartialFileAsync(long uploadLength, string metadata, CancellationToken cancellationToken)
        {
            return _concatStore.CreatePartialFileAsync(uploadLength, metadata, cancellationToken);
        }

        public Task<string> CreateFinalFileAsync(string[] partialFiles, string metadata, CancellationToken cancellationToken)
        {
            return _concatStore.CreateFinalFileAsync(partialFiles, metadata, cancellationToken);
        }

        public Task SetUploadLengthAsync(string fileId, long uploadLength, CancellationToken cancellationToken)
        {
            return _creationDeferLengthStore.SetUploadLengthAsync(fileId, uploadLength, cancellationToken);
        }

        public Task<ITusFile> GetFileAsync(string fileId, CancellationToken cancellationToken)
        {
            return _readableStore.GetFileAsync(fileId, cancellationToken);
        }
    }
}

#endif