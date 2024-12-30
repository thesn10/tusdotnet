using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using tusdotnet.Controllers.ActionResults;
using tusdotnet.Controllers.Contexts.Tus2;
using tusdotnet.ExternalMiddleware.EndpointRouting;
using tusdotnet.Models;
using tusdotnet.Routing;
using tusdotnet.Storage.Results.Tus2;
using tusdotnet.Storage.Tus2;
using tusdotnet.Tus2;
//using tusdotnet.Tus2;

namespace tusdotnet.Controllers
{
    public class Tus2ControllerBase
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
        public Tus2StorageClient StorageClient { get; internal set; }

        /// <summary>
        /// Gets the <see cref="ITusStorageClientProvider"/> for the executing action.
        /// </summary>
        /// /// <remarks>
        /// <see cref="TusProtocolHandlerEndpointBased{TController}"/> activates this property while activating controllers.
        /// If user code directly instantiates a controller, the getter returns an empty ITusStorageClientProvider
        /// </remarks>
        //public ITusStorageClientProvider StorageClientProvider { get; internal set; }

        private void EnsureStorageClientNotNull(string methodName)
        {
            if (StorageClient == null)
                throw new TusConfigurationException(
                    $"No storage client is configured for {GetType().FullName}. Implement virtual controller method \"{methodName}\" yourself or configure a storage client.");
        }


        public virtual async Task<UploadRetrievingProcedureResponse> RetrieveOffset(RetrieveOffsetContext context)
        {
            EnsureStorageClientNotNull(nameof(RetrieveOffset));

            var result = await StorageClient.RetrieveOffset(context.Headers.UploadToken);

            return new UploadRetrievingProcedureResponse(result);
        }

        public virtual async Task<UploadCreationProcedureResponse> CreateFile(CreateFileContext context)
        {
            EnsureStorageClientNotNull(nameof(CreateFile));

            await StorageClient.CreateFile(context.Headers.UploadToken, context.Metadata);

            return new UploadCreationProcedureResponse();
        }

        public virtual async Task<UploadAppendingProcedureResponse> WriteData(WriteDataContext context)
        {
            EnsureStorageClientNotNull(nameof(WriteData));

            var result = await StorageClient.WriteData(
                context.Headers.UploadToken, 
                context.BodyReader, 
                context.Headers.UploadIncomplete ?? false, // TODO
                context.CancellationToken);

            return new UploadAppendingProcedureResponse(result);
        }

        public virtual async Task<UploadCancellationProcedureResponse> Delete(Contexts.Tus2.DeleteContext context)
        {
            EnsureStorageClientNotNull(nameof(Delete));

            await StorageClient.Delete(context.Headers.UploadToken);

            return new UploadCancellationProcedureResponse();
        }

        protected Tus2RetrieveOffsetResult RetrieveOffsetResult(long uploadOffset, bool uploadIncomplete) => 
            new Tus2RetrieveOffsetResult(uploadOffset, uploadIncomplete);

        protected Tus2RetrieveOffsetResult RetrieveOffsetResult(RetrieveOffsetResult result) =>
            RetrieveOffsetResult(result.UploadOffset, result.UploadIncomplete);


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
        protected Tus2BaseResult StatusCode(HttpStatusCode statusCode, string message) => new Tus2BaseResult(statusCode, message);
    }
}
