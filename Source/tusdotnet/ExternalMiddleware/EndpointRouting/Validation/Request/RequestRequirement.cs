#if endpointrouting
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.Validation
{
    internal abstract class RequestRequirement
    {
        public abstract Task<(HttpStatusCode status, string error)> Validate(TusExtensionInfo extensionInfo, HttpContext context);

        protected (HttpStatusCode status, string error) Ok()
        {
            return (HttpStatusCode.OK, null);
        }

        protected (HttpStatusCode status, string error) Conflict(string errorMessage)
        {
            return (HttpStatusCode.Conflict, errorMessage);
        }

        protected (HttpStatusCode status, string error) BadRequest(string errorMessage)
        {
            return (HttpStatusCode.BadRequest, errorMessage);
        }

        protected (HttpStatusCode status, string error) RequestEntityTooLarge(string errorMessage)
        {
            return (HttpStatusCode.RequestEntityTooLarge, errorMessage);
        }

        protected (HttpStatusCode status, string error) Forbidden(string errorMessage)
        {
            return (HttpStatusCode.Forbidden, errorMessage);
        }

        protected (HttpStatusCode status, string error) NotFound()
        {
            return (HttpStatusCode.NotFound, null);
        }

        protected (HttpStatusCode status, string error) UnsupportedMediaType(string errorMessage)
        {
            return (HttpStatusCode.UnsupportedMediaType, errorMessage);
        }

        protected Task<(HttpStatusCode status, string error)> OkTask()
        {
            return Task.FromResult<(HttpStatusCode status, string error)>((HttpStatusCode.OK, null));
        }

        protected Task<(HttpStatusCode status, string error)> ConflictTask(string errorMessage)
        {
            return Task.FromResult<(HttpStatusCode status, string error)>((HttpStatusCode.Conflict, errorMessage));
        }

        protected Task<(HttpStatusCode status, string error)> BadRequestTask(string errorMessage)
        {
            return Task.FromResult<(HttpStatusCode status, string error)>((HttpStatusCode.BadRequest, errorMessage));
        }

        protected Task<(HttpStatusCode status, string error)> RequestEntityTooLargeTask(string errorMessage)
        {
            return Task.FromResult<(HttpStatusCode status, string error)>((HttpStatusCode.RequestEntityTooLarge, errorMessage));
        }

        protected Task<(HttpStatusCode status, string error)> ForbiddenTask(string errorMessage)
        {
            return Task.FromResult<(HttpStatusCode status, string error)>((HttpStatusCode.Forbidden, errorMessage));
        }

        protected Task<(HttpStatusCode status, string error)> NotFoundTask()
        {
            return Task.FromResult<(HttpStatusCode status, string error)>((HttpStatusCode.NotFound, null));
        }

        protected Task<(HttpStatusCode status, string error)> UnsupportedMediaTypeTask(string errorMessage)
        {
            return Task.FromResult<(HttpStatusCode status, string error)>((HttpStatusCode.UnsupportedMediaType, errorMessage));
        }
    }
}
#endif