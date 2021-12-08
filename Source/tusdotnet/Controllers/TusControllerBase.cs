#if endpointrouting

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using tusdotnet.ExternalMiddleware.EndpointRouting;
using tusdotnet.Models;
using tusdotnet.Models.Concatenation;
using tusdotnet.Routing;
using tusdotnet.Storage;

namespace tusdotnet.Controllers
{
    /// <summary>
    /// Base class for writing tus controllers
    /// </summary>
    public abstract class TusControllerBase
    {
        /// <summary>
        /// Gets the <see cref="TusContext"/> for the executing action.
        /// </summary>
        /// <remarks>
        /// <see cref="TusProtocolHandlerEndpointBased{TController}"/> activates this property while activating controllers.
        /// If user code directly instantiates a controller, the getter returns an empty TusContext
        /// </remarks>
        public TusContext TusContext { get; internal set; }

        /// <summary>
        /// Gets the <see cref="HttpContext"/> for the executing action.
        /// </summary>
        public HttpContext HttpContext => TusContext?.HttpContext!;

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
        /// Gets the <see cref="ITusEndpointOptions"/> for the executing action.
        /// </summary>
        public ITusEndpointOptions EndpointOptions => TusContext?.EndpointOptions!;

        /// <summary>
        /// Gets information about the supported tus extensions.
        /// </summary>
        public FeatureSupportContext ExtensionInfo => TusContext?.FeatureSupportContext;

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

        /// <summary>
        /// Called on a create request
        /// </summary>
        public virtual async Task<ICreateResult> Create(CreateContext context)
        {
            EnsureStorageClientNotNull(nameof(Create));

            var createResult = await StorageClient.Create(context, new CreateOptions()
            {
                Expiration = this.GetType().GetCustomAttribute<FileExpirationAttribute>()?.Expiration,

            }, HttpContext.RequestAborted);

            return CreateStatus(createResult);
        }

        /// <summary>
        /// Called on a write request
        /// </summary>
        public virtual async Task<IWriteResult> Write(WriteContext context)
        {
            EnsureStorageClientNotNull(nameof(Write));

            var writeResult = await StorageClient.Write(context, new WriteOptions()
            {
                Expiration = this.GetType().GetCustomAttribute<FileExpirationAttribute>()?.Expiration,
#if pipelines
                UsePipelinesIfAvailable = this.GetType().GetCustomAttribute<UsePipelineWriteAttribute>()?.UsePipelines ?? false,
#endif

            }, HttpContext.RequestAborted);

            return WriteStatus(writeResult);
        }

        /// <summary>
        /// Called on a delete request
        /// </summary>
        public virtual async Task<ISimpleResult> Delete(DeleteContext context)
        {
            EnsureStorageClientNotNull(nameof(Delete));

            await StorageClient.Delete(context, cancellationToken: HttpContext.RequestAborted);

            return Ok();
        }

        /// <summary>
        /// Get information about a file
        /// </summary>
        public virtual async Task<IFileInfoResult> GetFileInfo(GetFileInfoContext context)
        {
            EnsureStorageClientNotNull(nameof(GetFileInfo));

            var info = await StorageClient.GetFileInfo(context, HttpContext.RequestAborted);

            return FileInfo(info);
        }

        /// <summary>
        /// Called when a file upload is completed
        /// </summary>
        public virtual Task<ISimpleResult> FileCompleted(FileCompletedContext context)
        {
            return Task.FromResult<ISimpleResult>(Ok());
        }

        /// <summary>
        /// Override this if you want to change the supported tus extensions of your controller
        /// </summary>
        public virtual async Task<FeatureSupportContext> GetOptions()
        {
            EnsureStorageClientNotNull(nameof(GetOptions));

            var extensionInfo = await StorageClient.GetExtensionInfo(HttpContext.RequestAborted);

            var disabledExts = GetType().GetCustomAttribute<DisableExtensionAttribute>()?.ExtensionNames;
            if (disabledExts != null)
            {
                foreach (var disabledExt in disabledExts)
                {
                    extensionInfo.SupportedExtensions.Disable(disabledExt);
                }
            }

            return extensionInfo;
        }

        /// <summary>
        /// Authorize an controller action
        /// </summary>
        public virtual async Task<ISimpleResult> Authorize(AuthorizeContext context)
        {
            var authService = HttpContext.RequestServices.GetService<IAuthorizationService>();
            if (authService != null)
            {
                var authorizeAttribute = context.GetMethodToAuthorize().GetCustomAttribute<AuthorizeAttribute>();

                if (authorizeAttribute != default)
                {
                    var authResult = await authService.AuthorizeAsync(User, authorizeAttribute.Policy);

                    if (authResult.Succeeded)
                    {
                        return Ok();
                    }
                    else return Unauthorized();
                }
            }

            return Ok();
        }

        private void EnsureStorageClientNotNull(string methodName)
        {
            if (StorageClient == null)
                throw new TusConfigurationException(
                    $"No storage client is configured for {GetType().FullName}. Implement virtual controller method \"{methodName}\" yourself or configure a storage client.");
        }

        /// <summary>
        /// Return a create status result
        /// </summary>
        protected TusCreateStatusResult CreateStatus(CreateResult result) => new TusCreateStatusResult(result);
        /// <summary>
        /// Return a create status result
        /// </summary>
        protected TusCreateStatusResult CreateStatus(string fileId, DateTimeOffset? expires = null) => new TusCreateStatusResult(fileId, expires);

        /// <summary>
        /// Return a write status result
        /// </summary>
        protected TusWriteStatusResult WriteStatus(WriteResult result) => new TusWriteStatusResult(result);
        /// <summary>
        /// Return a write status result
        /// </summary>
        protected TusWriteStatusResult WriteStatus(long uploadOffset, bool isComplete, DateTimeOffset? fileExpires = null, FileConcat? fileConcat = null)
            => new TusWriteStatusResult(uploadOffset, isComplete, fileExpires, fileConcat);

        /// <summary>
        /// Return a file info result
        /// </summary>
        protected TusFileInfoResult FileInfo(GetFileInfoResult result) => new TusFileInfoResult(result);
        /// <summary>
        /// Return a file info result
        /// </summary>
        protected TusFileInfoResult FileInfo(string uploadMetadata, long? uploadLength, long uploadOffset, FileConcat? uploadConcat = null)
            => new TusFileInfoResult(uploadMetadata, uploadLength, uploadOffset, uploadConcat);

        /// <summary>
        /// Return an OK result
        /// </summary>
        protected TusOkResult Ok() => new TusOkResult();
        /// <summary>
        /// Return a 400 BadRequest result
        /// </summary>
        protected TusBadRequestResult BadRequest() => new TusBadRequestResult();
        /// <summary>
        /// Return a 400 BadRequest result
        /// </summary>
        protected TusBadRequestResult BadRequest(string error) => new TusBadRequestResult(error);
        /// <summary>
        /// Return a 403 Forbidden result
        /// </summary>
        protected TusForbiddenResult Forbidden() => new TusForbiddenResult();
        /// <summary>
        /// Return a 401 Forbidden result
        /// </summary>
        protected TusUnauthorizedResult Unauthorized() => new TusUnauthorizedResult();
        /// <summary>
        /// Return the specified status code. This may not be compliant with tus spec, use with caution!
        /// </summary>
        protected TusStatusCodeResult StatusCode(HttpStatusCode statusCode, string message) => new TusStatusCodeResult(statusCode, message);

    }
}

#endif