#if endpointrouting

using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using tusdotnet.Adapters;
using tusdotnet.ExternalMiddleware.Core;
using tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers;
using tusdotnet.Helpers;
using tusdotnet.Models;
using tusdotnet.Models.Configuration;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// A builtin tus controller which is used by endpoints.MapTus()
    /// </summary>
    [TusController]
    internal sealed class EventsBasedTusController : TusControllerBase, IControllerWithOptions<TusSimpleEndpointOptions>
    {
        public TusSimpleEndpointOptions Options { get; set; }

        public override async Task<ICreateResult> Create(CreateContext context)
        {
            this.StorageClient = await StorageClientProvider.Get(Options.StorageProfile);

            var onBeforeCreateResult = await Notify<BeforeCreateContext>(null, ctx =>
            {
                ctx.Metadata = context.Metadata;
                ctx.UploadLength = context.UploadLength;
                ctx.FileConcatenation = context.FileConcat;
            });

            if (onBeforeCreateResult != null && onBeforeCreateResult.HasFailed)
            {
                return StatusCode(onBeforeCreateResult.StatusCode, onBeforeCreateResult.ErrorMessage);
            }

            var createResult = await StorageClient.Create(context, new CreateOptions()
            {
                Expiration = Options.Expiration,

            });

            await Notify<CreateCompleteContext>(createResult.FileId, ctx =>
            {
                ctx.FileId = createResult.FileId;
                ctx.FileConcatenation = context.FileConcat;
                ctx.Metadata = context.Metadata;
                ctx.UploadLength = context.UploadLength;
            });

            return CreateStatus(createResult);
        }

        public override async Task<IWriteResult> Write(WriteContext context)
        {
            this.StorageClient = await StorageClientProvider.Get(Options.StorageProfile);

            var writeResult = await StorageClient.Write(context, new WriteOptions()
            {
                Expiration = Options.Expiration,
                FileLockProvider = Options.FileLockProvider,
#if pipelines
                UsePipelinesIfAvailable = Options.UsePipelinesIfAvailable,
#endif

            }, HttpContext.RequestAborted);

            return WriteStatus(writeResult);
        }

        public override async Task<ISimpleResult> FileCompleted(FileCompletedContext context)
        {
            var contextAdapter = ContextAdapterBuilder.FromHttpContext(HttpContext, new DefaultTusConfiguration()
            {
                Store = StorageClient.Store,
                Events = Options.Events,
                Expiration = Options.Expiration,
                UrlPath = UrlPath,
            });

            await EventHelper.NotifyFileComplete(contextAdapter);

            return Ok();
        }

        public override async Task<IFileInfoResult> GetFileInfo(GetFileInfoContext context)
        {
            this.StorageClient = await StorageClientProvider.Get(Options.StorageProfile);

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

            await StorageClient.Delete(context, HttpContext.RequestAborted);

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
