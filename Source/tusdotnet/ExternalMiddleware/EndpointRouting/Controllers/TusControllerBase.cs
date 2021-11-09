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

            return CreateOk(createResult);
        }

        public virtual async Task<IWriteResult> Write(WriteContext context)
        {
            var writeResult = await StorageClient.Write(context, new WriteOptions()
            {
                Expiration = this.GetType().GetCustomAttribute<TusFileExpirationAttribute>()?.Expiration,

            }, HttpContext.RequestAborted);

            return WriteOk(writeResult);
        }

        public virtual async Task<IDeleteResult> Delete(DeleteContext context)
        {
            await StorageClient.Delete(context, HttpContext.RequestAborted);

            return DeleteOk();
        }

        public virtual async Task<IInfoResult> GetFileInfo(GetFileInfoContext context)
        {
            var info = await StorageClient.GetFileInfo(context, HttpContext.RequestAborted);

            return FileInfoOk(info);
        }

        public virtual async Task<ICompletedResult> FileCompleted(FileCompletedContext context)
        {
            return FileCompletedOk();
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

        public virtual async Task<bool> AuthorizeForAction(string actionName)
        {
            var authService = HttpContext.RequestServices.GetService<IAuthorizationService>();
            if (authService != null)
            {
                var authorizeAttribute = GetType().GetMethod(actionName).GetCustomAttributes(false).OfType<AuthorizeAttribute>().FirstOrDefault();

                if (authorizeAttribute != default)
                {
                    var authResult = await authService.AuthorizeAsync(User, authorizeAttribute.Policy);
                    return authResult.Succeeded;
                }
            }

            return true;
        }

        protected ICreateResult CreateOk(CreateResult result) 
            => new TusCreateOk(result);
        protected ICreateResult CreateOk(string fileId, DateTimeOffset? expires = null) 
            => new TusCreateOk(fileId, expires);

        protected IWriteResult WriteOk(WriteResult result) => new TusWriteOk(result);
        protected IWriteResult WriteOk(bool isComplete, long uploadOffset, bool clientDisconnectedDuringRead, bool? checksumMatches = null, DateTimeOffset? fileExpires = null) => 
            new TusWriteOk(isComplete, uploadOffset, clientDisconnectedDuringRead, checksumMatches, fileExpires);

        protected IDeleteResult DeleteOk() => new TusDeleteOk();

        protected IInfoResult FileInfoOk(GetFileInfoResult result)
            => new TusInfoOk(result.UploadMetadata, result.UploadLength, result.UploadOffset, result.UploadConcat);
        protected IInfoResult FileInfoOk(string uploadMetadata, long? uploadLength, long uploadOffset, FileConcat? uploadConcat = null)
            => new TusInfoOk(uploadMetadata, uploadLength, uploadOffset, uploadConcat);

        protected TusBadRequest BadRequest() => new TusBadRequest();
        protected TusBadRequest BadRequest(string error) => new TusBadRequest(error);
        protected TusForbidden Forbidden() => new TusForbidden();


        // needed?
        protected ICompletedResult FileCompletedOk() => new TusCompletedOk();

    }
}

#endif