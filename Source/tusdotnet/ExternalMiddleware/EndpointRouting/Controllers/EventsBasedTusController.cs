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

            return CreateOk(createResult);
        }

        public override async Task<IWriteResult> Write(WriteContext context)
        {
            this.StorageClient = await StorageClientProvider.Get(Options.StorageProfile);

            var writeResult = await StorageClient.Write(context, new WriteOptions()
            {
                Expiration = Options.Expiration,

            }, HttpContext.RequestAborted);

            return WriteOk(writeResult);
        }

        public override async Task<ICompletedResult> FileCompleted(FileCompletedContext context)
        {
            await EventHelper.NotifyFileComplete(GetContext());

            return await base.FileCompleted(context);
        }

        public override async Task<IInfoResult> GetFileInfo(GetFileInfoContext context)
        {
            this.StorageClient = await StorageClientProvider.Get(Options.StorageProfile);

            var info = await StorageClient.GetFileInfo(context);

            return FileInfoOk(info);
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

        public override async Task<IDeleteResult> Delete(DeleteContext context)
        {
            var contextAdapter = GetContext();
            if (await EventHelper.Validate<BeforeDeleteContext>(contextAdapter) == ResultType.StopExecution)
            {
                return BadRequest();
            }

            await StorageClient.Delete(context, HttpContext.RequestAborted);

            await EventHelper.Notify<DeleteCompleteContext>(contextAdapter);

            return DeleteOk();
        }

        public override async Task<bool> AuthorizeForAction(string actionName)
        {
            var onAuhorizeResult = await EventHelper.Validate<AuthorizeContext>(GetContext(), ctx =>
            {
                ctx.Intent = actionName switch
                {
                    nameof(Create) => IntentType.CreateFile,
                    nameof(Write) => IntentType.WriteFile,
                    nameof(Delete) => IntentType.DeleteFile,
                    nameof(GetFileInfo) => IntentType.GetFileInfo,
                    nameof(GetOptions) => IntentType.GetOptions,
                    // TODO
                    //nameof(Concatenate) => IntentType.ConcatenateFiles,
                    _ => IntentType.NotApplicable,
                };
                ctx.FileConcatenation = null; //GetFileConcatenationFromIntentHandler(intentHandler);
            });

            if (onAuhorizeResult == ResultType.StopExecution)
            {
                return false;
            }
            return true;
        }
    }
}
#endif
