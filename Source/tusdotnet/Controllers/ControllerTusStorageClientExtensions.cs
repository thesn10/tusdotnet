using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Models.Concatenation;
using tusdotnet.Storage;

namespace tusdotnet.Controllers
{
    /// <summary>
    /// Useful extensions to work with <see cref="TusStorageClient"/> inside a controller
    /// </summary>
    public static class ControllerTusStorageClientExtensions
    {
        /// <inheritdoc cref="TusStorageClient.Create(long, string, bool, CreateOptions, CancellationToken)"/>
        public static Task<CreateResult> Create(this TusStorageClient storageClient, CreateContext context, CreateOptions? options = default, CancellationToken cancellationToken = default)
        {
            if (context.FileConcatenation is FileConcatPartial)
            {
                return storageClient.Create(context.UploadLength, context.UploadMetadata, true, options, cancellationToken);
            }
            else if (context.FileConcatenation is FileConcatFinal final)
            {
                return storageClient.Create(final.Files, context.UploadMetadata, options, cancellationToken);
            }
            else
            {
                return storageClient.Create(context.UploadLength, context.UploadMetadata, false, options, cancellationToken);
            }
        }

        /// <inheritdoc cref="TusStorageClient.Write(string, System.IO.Stream, long, long?, bool, WriteOptions?, CancellationToken)"/>
        public static Task<WriteResult> Write(this TusStorageClient storageClient, WriteContext context, WriteOptions? options = default, CancellationToken cancellationToken = default)
        {
#if pipelines
            if (storageClient.StoreAdapter.Features.Pipelines && options.UsePipelinesIfAvailable)
            {
                return storageClient.Write(context.FileId, context.RequestReader, context.UploadOffset, context.UploadLength, context.IsCreationWithUpload, new WriteOptions()
                {
                    Expiration = options.Expiration,
                    FileLockProvider = options.FileLockProvider,
                    GetChecksumProvidedByClient = options.GetChecksumProvidedByClient ?? context.GetChecksumProvidedByClient,
                }, cancellationToken);
            }
            else
#endif
            {
                return storageClient.Write(context.FileId, context.RequestStream, context.UploadOffset, context.UploadLength, context.IsCreationWithUpload, new WriteOptions()
                {
                    Expiration = options.Expiration,
                    FileLockProvider = options.FileLockProvider,
                    GetChecksumProvidedByClient = options.GetChecksumProvidedByClient ?? context.GetChecksumProvidedByClient,
                }, cancellationToken);
            }
        }

        /// <inheritdoc cref="TusStorageClient.Delete(string,  DeleteOptions, CancellationToken)"/>
        public static Task Delete(this TusStorageClient storageClient, DeleteContext context, DeleteOptions? options = default, CancellationToken cancellationToken = default)
        {
            return storageClient.Delete(context.FileId, options, cancellationToken);
        }

        /// <inheritdoc cref="TusStorageClient.GetFileInfo(string, CancellationToken)"/>
        public static Task<GetFileInfoResult> GetFileInfo(this TusStorageClient storageClient, GetFileInfoContext context, CancellationToken cancellationToken = default)
        {
            return storageClient.GetFileInfo(context.FileId, cancellationToken);
        }
    }
}
