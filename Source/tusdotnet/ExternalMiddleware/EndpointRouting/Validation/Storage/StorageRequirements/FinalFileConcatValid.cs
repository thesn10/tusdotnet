using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Models.Concatenation;
using tusdotnet.Stores;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.Validation.Storage
{
    internal sealed class FinalFileConcatValid : StorageRequirement
    {
        private readonly string[] _partialFiles;
        private readonly long? _maxConcatSize;

        public FinalFileConcatValid(string[] partialFiles, long? maxConcatSize)
        {
            _partialFiles = partialFiles;
            _maxConcatSize = maxConcatSize;
        }

        public override async Task Validate(StoreAdapter store, CancellationToken cancellationToken)
        {
            var filesExist = await Task.WhenAll(_partialFiles.Select(f =>
                store.FileExistAsync(f, cancellationToken)));

            if (filesExist.Any(f => !f))
            {
                throw new TusInvalidConcatException(
                    $"Could not find some of the files supplied for concatenation: {string.Join(", ", filesExist.Zip(_partialFiles, (b, s) => new { exist = b, name = s }).Where(f => !f.exist).Select(f => f.name))}");
            }

            var filesArePartial = await Task.WhenAll(
                _partialFiles.Select(f => store.GetUploadConcatAsync(f, cancellationToken)));

            if (filesArePartial.Any(f => !(f is FileConcatPartial)))
            {
                throw new TusInvalidConcatException(
                    $"Some of the files supplied for concatenation are not marked as partial and can not be concatenated: {string.Join(", ", filesArePartial.Zip(_partialFiles, (s, s1) => new { partial = s is FileConcatPartial, name = s1 }).Where(f => !f.partial).Select(f => f.name))}");
            }

            var incompleteFiles = new List<string>(_partialFiles.Length);
            var totalSize = 0L;
            foreach (var file in _partialFiles)
            {
                var length = store.GetUploadLengthAsync(file, cancellationToken);
                var offset = store.GetUploadOffsetAsync(file, cancellationToken);
                await Task.WhenAll(length, offset);

                if (length.Result != null)
                {
                    totalSize += length.Result.Value;
                }

                if (length.Result != offset.Result)
                {
                    incompleteFiles.Add(file);
                }
            }

            if (incompleteFiles.Count > 0)
            {
                throw new TusInvalidConcatException(
                    $"Some of the files supplied for concatenation are not finished and can not be concatenated: {string.Join(", ", incompleteFiles)}");
            }

            if (_maxConcatSize.HasValue && totalSize > _maxConcatSize)
            {
                throw new TusFileTooLargeException("The concatenated file exceeds the server's max file size.");
            }
        }
    }
}
