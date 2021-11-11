using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Models.Concatenation;
using tusdotnet.Stores;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.Validation.Storage
{
    internal sealed class UploadConcatForConcatenateFiles : StorageRequirement
    {
        private readonly UploadConcat _uploadConcat;
        private readonly long _maxUploadSize;

        public UploadConcatForConcatenateFiles(UploadConcat uploadConcat, long maxUploadSize)
        {
            _uploadConcat = uploadConcat;
            _maxUploadSize = maxUploadSize;
        }

        public override async Task Validate(StoreAdapter store, CancellationToken cancellationToken)
        {
            if (!_uploadConcat.IsValid)
            {
                throw new TusInvalidConcatException(_uploadConcat.ErrorMessage);
            }

            if (_uploadConcat.Type is FileConcatFinal finalConcat)
            {
                await ValidateFinalFileCreation(finalConcat, store, cancellationToken);
            }
        }

        private async Task ValidateFinalFileCreation(FileConcatFinal finalConcat, StoreAdapter store, CancellationToken cancellationToken)
        {
            var filesExist = await Task.WhenAll(finalConcat.Files.Select(f =>
                store.FileExistAsync(f, cancellationToken)));

            if (filesExist.Any(f => !f))
            {
                throw new TusInvalidConcatException(
                    $"Could not find some of the files supplied for concatenation: {string.Join(", ", filesExist.Zip(finalConcat.Files, (b, s) => new { exist = b, name = s }).Where(f => !f.exist).Select(f => f.name))}");
            }

            var filesArePartial = await Task.WhenAll(
                finalConcat.Files.Select(f => store.GetUploadConcatAsync(f, cancellationToken)));

            if (filesArePartial.Any(f => !(f is FileConcatPartial)))
            {
                throw new TusInvalidConcatException(
                    $"Some of the files supplied for concatenation are not marked as partial and can not be concatenated: {string.Join(", ", filesArePartial.Zip(finalConcat.Files, (s, s1) => new { partial = s is FileConcatPartial, name = s1 }).Where(f => !f.partial).Select(f => f.name))}");
            }

            var incompleteFiles = new List<string>(finalConcat.Files.Length);
            var totalSize = 0L;
            foreach (var file in finalConcat.Files)
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

            if (totalSize > _maxUploadSize)
            {
                throw new TusFileTooLargeException("The concatenated file exceeds the server's max file size.");
            }
        }
    }
}
