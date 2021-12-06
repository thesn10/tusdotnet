#if endpointrouting

using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers;
using tusdotnet.Helpers;
using tusdotnet.Models.Configuration;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// A builtin tus controller which is used by endpoints.MapTus() and the Middleware
    /// </summary>
    [TusController]
    internal sealed class EventsBasedTusController : TusControllerBase, IControllerWithOptions<TusSimpleEndpointOptions>
    {
        public TusSimpleEndpointOptions Options { get; set; }

        public override async Task<ICreateResult> Create(CreateContext context)
        {
            var onBeforeCreateResult = await Notify<BeforeCreateContext>(null, ctx =>
            {
                ctx.Metadata = context.Metadata;
                ctx.UploadLength = context.UploadLength;
                ctx.FileConcatenation = context.FileConcatenation;
            });

            if (onBeforeCreateResult != null && onBeforeCreateResult.HasFailed)
            {
                return StatusCode(onBeforeCreateResult.StatusCode, onBeforeCreateResult.ErrorMessage);
            }

            var createOptions = new CreateOptions()
            {
                Expiration = Options.Expiration,
                MaxConcatFileSize = Options.MaxAllowedUploadSizeInBytes,
            };

            createOptions.MockSystemTime(Options.MockedTime);

            var createResult = await StorageClient.Create(context, createOptions, HttpContext.RequestAborted);

            await Notify<CreateCompleteContext>(createResult.FileId, ctx =>
            {
                ctx.FileId = createResult.FileId;
                ctx.FileConcatenation = context.FileConcatenation;
                ctx.Metadata = context.Metadata;
                ctx.UploadLength = context.UploadLength;
            });

            return CreateStatus(createResult);
        }

        public override async Task<IWriteResult> Write(WriteContext context)
        {
            var writeOptions = new WriteOptions()
            {
                Expiration = Options.Expiration,
                FileLockProvider = Options.FileLockProvider,
#if pipelines
                UsePipelinesIfAvailable = Options.UsePipelinesIfAvailable,
#endif
            };

            writeOptions.MockSystemTime(Options.MockedTime);

            var writeResult = await StorageClient.Write(context, writeOptions, HttpContext.RequestAborted);

            return WriteStatus(writeResult);
        }

        public override async Task<ISimpleResult> FileCompleted(FileCompletedContext context)
        {
            if (Options.OnUploadCompleteAsync == null && Options.Events?.OnFileCompleteAsync == null)
            {
                return Ok();
            }

            var eventContext = FileCompleteContext.Create(context.FileId, HttpContext, StorageClient.Store, HttpContext.RequestAborted);

            if (Options.OnUploadCompleteAsync != null)
            {
                await Options.OnUploadCompleteAsync(eventContext.FileId, eventContext.Store, eventContext.CancellationToken);
            }

            if (Options.OnUploadCompleteAsync != null)
            {
                await Options.Events.OnFileCompleteAsync(eventContext);
            }

            return Ok();
        }

        public override async Task<IFileInfoResult> GetFileInfo(GetFileInfoContext context)
        {
            var info = await StorageClient.GetFileInfo(context);

            return FileInfo(info);
        }

        public override async Task<ISimpleResult> Delete(DeleteContext context)
        {
            var onBeforeDelete = await Notify<BeforeDeleteContext>(context.FileId);

            if (onBeforeDelete != null && onBeforeDelete.HasFailed)
            {
                return StatusCode(onBeforeDelete.StatusCode, onBeforeDelete.ErrorMessage);
            }

            await StorageClient.Delete(context, new DeleteOptions() 
            { 
                FileLockProvider = Options.FileLockProvider

            }, HttpContext.RequestAborted);

            await Notify<DeleteCompleteContext>(context.FileId);

            return Ok();
        }

        public override async Task<ISimpleResult> Authorize(AuthorizeContext context)
        {
            var onAuhorizeResult = await Notify<Models.Configuration.AuthorizeContext>(context.FileId, ctx =>
            {
                ctx.Intent = context.IntentType;
                ctx.FileConcatenation = (context.RequestHandler as ConcatenateRequestHandler)?.UploadConcat.Type;
            });

            if (onAuhorizeResult != null && onAuhorizeResult.HasFailed)
            {
                return StatusCode(onAuhorizeResult.StatusCode, onAuhorizeResult.ErrorMessage);
            }
            return Ok();
        }

        private async Task<T> Notify<T>(string fileId, Action<T> configure = null) where T : EventContext<T>, new()
        {
            var handler = EventHelper.GetHandlerFromEvents<T>(Options.Events);

            if (handler == null)
                return null;

            var eventContext = EventContext<T>.Create(fileId, HttpContext, StorageClient.Store, HttpContext.RequestAborted, configure);

            await handler(eventContext);

            return eventContext;
        }
    }
}
#endif
