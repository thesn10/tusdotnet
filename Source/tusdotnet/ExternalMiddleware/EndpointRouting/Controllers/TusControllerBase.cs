#if endpointrouting

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System;
using tusdotnet.Models.Concatenation;
using System.Reflection;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    /// <summary>
    /// Base class for writing tus controllers
    /// </summary>
    public abstract class TusControllerBase
    {
        /// <summary>
        /// Gets the <see cref="HttpContext"/> for the executing action.
        /// </summary>
        /// <remarks>
        /// <see cref="TusProtocolHandlerEndpointBased{TController}"/> activates this property while activating controllers.
        /// If user code directly instantiates a controller, the getter returns an empty HttpContext
        /// </remarks>
        public HttpContext HttpContext { get; internal set; }

        /// <summary>
        /// Gets the <see cref="HttpRequest"/> for the executing action.
        /// </summary>
        public HttpRequest Request => HttpContext?.Request!;

        /// <summary>
        /// Gets the <see cref="HttpResponse"/> for the executing action.
        /// </summary>
        public HttpResponse Response => HttpContext?.Response!;


        /// <summary>
        /// Gets the <see cref="ClaimsPrincipal"/> for user associated with the executing action.
        /// </summary>
        public ClaimsPrincipal User => HttpContext?.User!;

        /// <summary>
        /// Gets the <see cref="TusStorageClient"/> for the executing action.
        /// </summary>
        /// /// <remarks>
        /// <see cref="TusProtocolHandlerEndpointBased{TController}"/> activates this property while activating controllers.
        /// If user code directly instantiates a controller, the getter returns an empty TusStorageClient
        /// </remarks>
        public TusStorageClient StorageClient { get; internal set; }

        /// <summary>
        /// Gets the <see cref="ITusStorageClientProvider"/> for the executing action.
        /// </summary>
        /// /// <remarks>
        /// <see cref="TusProtocolHandlerEndpointBased{TController}"/> activates this property while activating controllers.
        /// If user code directly instantiates a controller, the getter returns an empty ITusStorageClientProvider
        /// </remarks>
        public ITusStorageClientProvider StorageClientProvider { get; internal set; }

        public virtual async Task<ICreateResult> Create(CreateContext context)
        {
            var createResult = await StorageClient.Create(context, new CreateOptions()
            {
                Expiration = this.GetType().GetCustomAttribute<TusFileExpirationAttribute>()?.Expiration,

            }, HttpContext.RequestAborted);

            return CreateStatus(createResult);
        }

        public virtual async Task<IWriteResult> Write(WriteContext context)
        {
            var writeResult = await StorageClient.Write(context, new WriteOptions()
            {
                Expiration = this.GetType().GetCustomAttribute<TusFileExpirationAttribute>()?.Expiration,
#if pipelines
                UsePipelinesIfAvailable = this.GetType().GetCustomAttribute<TusUsePipelineWriteAttribute>()?.UsePipelines ?? false,
#endif

            }, HttpContext.RequestAborted);

            return WriteStatus(writeResult);
        }

        public virtual async Task<ISimpleResult> Delete(DeleteContext context)
        {
            await StorageClient.Delete(context, HttpContext.RequestAborted);

            return Ok();
        }

        public virtual async Task<IFileInfoResult> GetFileInfo(GetFileInfoContext context)
        {
            var info = await StorageClient.GetFileInfo(context, HttpContext.RequestAborted);

            return FileInfo(info);
        }

        public virtual async Task<ISimpleResult> FileCompleted(FileCompletedContext context)
        {
            return Ok();
        }

        /// <summary>
        /// Override this if you want to change the supported tus extensions of your controller
        /// </summary>
        public virtual async Task<TusExtensionInfo> GetOptions()
        {
            var extensionInfo = await StorageClient?.GetExtensionInfo(HttpContext.RequestAborted);

            var disabledExts = GetType().GetCustomAttribute<TusDisableExtensionAttribute>()?.ExtensionNames;
            if (disabledExts != null)
            {
                foreach (var disabledExt in disabledExts)
                {
                    extensionInfo.SupportedExtensions.Disable(disabledExt);
                }
            }

            return extensionInfo;
        }

        // TODO
        // public virtual Task Concatenate()

        public virtual async Task<ISimpleResult> Authorize(AuthorizeContext context)
        {
            var authService = HttpContext.RequestServices.GetService<IAuthorizationService>();
            if (authService != null)
            {
                var authorizeAttribute = context.ControllerMethod.GetCustomAttribute<AuthorizeAttribute>();

                if (authorizeAttribute != default)
                {
                    var authResult = await authService.AuthorizeAsync(User, authorizeAttribute.Policy);

                    if (authResult.Succeeded)
                    {
                        return Ok();
                    }
                    else return Forbidden();
                }
            }

            return Ok();
        }

        protected ICreateResult CreateStatus(CreateResult result) 
            => new TusCreateStatusResult(result);
        protected ICreateResult CreateStatus(string fileId, DateTimeOffset? expires = null) 
            => new TusCreateStatusResult(fileId, expires);

        protected IWriteResult WriteStatus(WriteResult result) => new TusWriteStatusResult(result);
        protected IWriteResult WriteStatus(bool isComplete, long uploadOffset, bool clientDisconnectedDuringRead, bool? checksumMatches = null, DateTimeOffset? fileExpires = null) => 
            new TusWriteStatusResult(isComplete, uploadOffset, clientDisconnectedDuringRead, checksumMatches, fileExpires);

        protected IFileInfoResult FileInfo(GetFileInfoResult result)
            => new TusFileInfoResult(result.UploadMetadata, result.UploadLength, result.UploadOffset, result.UploadConcat);
        protected IFileInfoResult FileInfo(string uploadMetadata, long? uploadLength, long uploadOffset, FileConcat? uploadConcat = null)
            => new TusFileInfoResult(uploadMetadata, uploadLength, uploadOffset, uploadConcat);

        protected TusOkResult Ok() => new TusOkResult();
        protected TusBadRequestResult BadRequest() => new TusBadRequestResult();
        protected TusBadRequestResult BadRequest(string error) => new TusBadRequestResult(error);
        protected TusForbiddenResult Forbidden() => new TusForbiddenResult();

    }
}

#endif