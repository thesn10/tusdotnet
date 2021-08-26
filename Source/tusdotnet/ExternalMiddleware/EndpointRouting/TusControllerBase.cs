#if endpointrouting

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System;
using tusdotnet.Models.Concatenation;
using System.Reflection;
using tusdotnet.Constants;
using tusdotnet.Models.Expiration;

namespace tusdotnet.ExternalMiddleware.EndpointRouting
{
    public abstract class TusControllerBase
    {
        protected TusControllerBase()
        {

        }

        /// <summary>
        /// Gets the <see cref="Http.HttpContext"/> for the executing action.
        /// </summary>
        /// <remarks>
        /// <see cref="TusProtocolHandlerEndpointBased{TController}"/> activates this property while activating controllers.
        /// If user code directly instantiates a controller, the getter returns an empty HttpContext
        /// </remarks>
        public HttpContext HttpContext { get; set; }

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

        public virtual async Task<ITusCompletedActionResult> FileCompleted(FileCompletedContext context, CancellationToken cancellation) 
        {
            return FileCompletedOk();
        }

        public abstract Task<ITusCreateActionResult> Create(CreateContext context, CancellationToken cancellation);

        public abstract Task<ITusWriteActionResult> Write(WriteContext context, CancellationToken cancellationToken);

        public abstract Task<ITusInfoActionResult> GetFileInfo(GetFileInfoContext context, CancellationToken cancellationToken);

        /// <summary>
        /// This method returns which extensions the controller supports
        /// according to the controller attributes.
        /// Only overwrite this method if you know what youre doing.
        /// </summary>
        public virtual Task<ControllerCapabilities> GetCapabilities()
        {
            ControllerCapabilities capabilities = new ControllerCapabilities();

            var capAttribute = this.GetType().GetCustomAttribute<TusInheritCapabilitiesAttribute>();
            if (capAttribute != null)
            {
                var storeCapabilities = StoreAdapter.GetCapabilities(capAttribute.StoreType);
                capabilities.SupportedExtensions.AddRange(storeCapabilities);

                if (storeCapabilities.Contains(ExtensionConstants.Checksum))
                {
                    // TODO: how to get supported checksum algos from store without instantiating it?
                    capabilities.SupportedChecksumAlgorithms.Add("sha1");
                }
            }

            var enableExtAttribute = this.GetType().GetCustomAttribute<TusEnableExtensionAttribute>();
            if (enableExtAttribute != null)
            {
                capabilities.SupportedExtensions.AddRange(enableExtAttribute.ExtensionNames);
            }

            var disableExtAttribute = this.GetType().GetCustomAttribute<TusDisableExtensionAttribute>();
            if (disableExtAttribute != null)
            {
                foreach (var disableExt in disableExtAttribute.ExtensionNames)
                {
                    if (capabilities.SupportedExtensions.Contains(disableExt))
                    {
                        capabilities.SupportedExtensions.Remove(disableExt);
                    }
                }
            }

            return Task.FromResult(capabilities);
        }

        internal async Task<bool> AuthorizeForAction(HttpContext context, string actionName)
        {
            var authService = context.RequestServices.GetService<IAuthorizationService>();
            if (authService != null)
            {
                var authorizeAttribute = GetType().GetMethod(actionName).GetCustomAttributes(false).OfType<AuthorizeAttribute>().FirstOrDefault();

                if (authorizeAttribute != default)
                {
                    var authResult = await authService.AuthorizeAsync(context.User, authorizeAttribute.Policy);
                    return authResult.Succeeded;
                }
            }

            return true;
        }

        protected ITusCreateActionResult CreateOk(CreateResult result) 
            => new TusCreateOk(result);
        protected ITusCreateActionResult CreateOk(string fileId, DateTimeOffset? expires = null) 
            => new TusCreateOk(fileId, expires);

        protected ITusWriteActionResult WriteOk(WriteResult result) => new TusWriteOk(result);
        protected ITusWriteActionResult WriteOk(bool isComplete, long uploadOffset, bool clientDisconnectedDuringRead) => 
            new TusWriteOk(isComplete, uploadOffset, clientDisconnectedDuringRead, null, null);
        protected ITusWriteActionResult WriteOk(bool isComplete, long uploadOffset, bool clientDisconnectedDuringRead, bool? checksumMatches) => 
            new TusWriteOk(isComplete, uploadOffset, clientDisconnectedDuringRead, checksumMatches, null);
        protected ITusWriteActionResult WriteOk(bool isComplete, long uploadOffset, bool clientDisconnectedDuringRead, bool? checksumMatches, DateTimeOffset? fileExpires) => 
            new TusWriteOk(isComplete, uploadOffset, clientDisconnectedDuringRead, checksumMatches, fileExpires);

        protected ITusInfoActionResult FileInfoOk(GetFileInfoResult result)
            => new TusInfoOk(result.UploadMetadata, result.UploadLength, result.UploadOffset, result.UploadConcat, result.UploadDeferLength);
        protected ITusInfoActionResult FileInfoOk(string uploadMetadata, long? uploadLength, long uploadOffset, bool uploadDeferLength = false) 
            => new TusInfoOk(uploadMetadata, uploadLength, uploadOffset, uploadDeferLength);
        protected ITusInfoActionResult FileInfoOk(string uploadMetadata, long? uploadLength, long uploadOffset, FileConcat? uploadConcat, bool uploadDeferLength = false)
            => new TusInfoOk(uploadMetadata, uploadLength, uploadOffset, uploadConcat, uploadDeferLength);

        protected TusFail Fail(string error) => new TusFail(error);
        protected TusForbidden Forbidden() => new TusForbidden();


        // needed?
        protected ITusCompletedActionResult FileCompletedOk() => new TusCompletedOk();

    }
}

#endif