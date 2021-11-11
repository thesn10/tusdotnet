#if endpointrouting

using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using tusdotnet.Adapters;
using tusdotnet.ExternalMiddleware.Core;
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

            var onBeforeCreateResult = await EventHelper.Validate<BeforeCreateContext>(GetContext(), ctx =>
            {
                ctx.Metadata = context.Metadata;
                ctx.UploadLength = context.UploadLength;
            });

            if (onBeforeCreateResult == ResultType.StopExecution)
            {
                return BadRequest();
            }

            var createResult = await StorageClient.Create(context, new CreateOptions()
            {
                Expiration = Options.Expiration,

            });

            await EventHelper.Notify<CreateCompleteContext>(GetContext(), ctx =>
            {
                ctx.FileId = createResult.FileId;
                ctx.FileConcatenation = null;
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
#if pipelines
                UsePipelinesIfAvailable = Options.UsePipelinesIfAvailable,
#endif

            }, HttpContext.RequestAborted);

            return WriteStatus(writeResult);
        }

        public override async Task<ISimpleResult> FileCompleted(FileCompletedContext context)
        {
            await EventHelper.NotifyFileComplete(GetContext());

            return Ok();
        }

        public override async Task<IFileInfoResult> GetFileInfo(GetFileInfoContext context)
        {
            this.StorageClient = await StorageClientProvider.Get(Options.StorageProfile);

            var info = await StorageClient.GetFileInfo(context);

            return FileInfo(info);
        }

        private ContextAdapter GetContext()
        {
            return ContextAdapterBuilder.CreateFakeContextAdapter(HttpContext, new DefaultTusConfiguration()
            {
                Store = StorageClient.Store,
                Events = Options.Events,
                Expiration = Options.Expiration
            });
        }

        public override async Task<ISimpleResult> Delete(DeleteContext context)
        {
            var contextAdapter = GetContext();
            if (await EventHelper.Validate<BeforeDeleteContext>(contextAdapter) == ResultType.StopExecution)
            {
                return BadRequest();
            }

            await StorageClient.Delete(context, HttpContext.RequestAborted);

            await EventHelper.Notify<DeleteCompleteContext>(contextAdapter);

            return Ok();
        }

        public override async Task<ISimpleResult> Authorize(AuthorizeContext context)
        {
            var onAuhorizeResult = await EventHelper.Validate<Models.Configuration.AuthorizeContext>(GetContext(), ctx =>
            {
                ctx.Intent = context.IntentType;
                ctx.CancellationToken = HttpContext.RequestAborted;
                ctx.FileConcatenation = null; //GetFileConcatenationFromIntentHandler(intentHandler);
            });

            if (onAuhorizeResult == ResultType.StopExecution)
            {
                return Forbidden();
            }
            return Ok();
        }
    }
}
#endif
