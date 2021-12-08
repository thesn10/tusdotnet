using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;
using tusdotnet.Controllers;
using tusdotnet.Routing;

namespace tusdotnet.RequestHandlers.Validation
{
    internal abstract class RequestRequirement
    {
        public abstract Task<ITusActionResult> Validate(FeatureSupportContext extensionInfo, HttpContext context);

        protected ITusActionResult Ok()
        {
            return new TusOkResult();
        }

        protected ITusActionResult Conflict(string errorMessage)
        {
            return new TusStatusCodeResult(HttpStatusCode.Conflict, errorMessage);
        }

        protected ITusActionResult BadRequest(string errorMessage)
        {
            return new TusStatusCodeResult(HttpStatusCode.BadRequest, errorMessage);
        }

        protected ITusActionResult RequestEntityTooLarge(string errorMessage)
        {
            return new TusStatusCodeResult(HttpStatusCode.RequestEntityTooLarge, errorMessage);
        }

        protected ITusActionResult Forbidden(string errorMessage)
        {
            return new TusStatusCodeResult(HttpStatusCode.Forbidden, errorMessage);
        }

        protected ITusActionResult NotFound()
        {
            return new TusStatusCodeResult(HttpStatusCode.NotFound, null);
        }

        protected ITusActionResult UnsupportedMediaType(string errorMessage)
        {
            return new TusStatusCodeResult(HttpStatusCode.UnsupportedMediaType, errorMessage);
        }

        protected Task<ITusActionResult> OkTask()
        {
            return Task.FromResult(Ok());
        }

        protected Task<ITusActionResult> ConflictTask(string errorMessage)
        {
            return Task.FromResult(Conflict(errorMessage));
        }

        protected Task<ITusActionResult> BadRequestTask(string errorMessage)
        {
            return Task.FromResult(BadRequest(errorMessage));
        }

        protected Task<ITusActionResult> RequestEntityTooLargeTask(string errorMessage)
        {
            return Task.FromResult(RequestEntityTooLarge(errorMessage));
        }

        protected Task<ITusActionResult> ForbiddenTask(string errorMessage)
        {
            return Task.FromResult(Forbidden(errorMessage));
        }

        protected Task<ITusActionResult> NotFoundTask()
        {
            return Task.FromResult(NotFound());
        }

        protected Task<ITusActionResult> UnsupportedMediaTypeTask(string errorMessage)
        {
            return Task.FromResult(UnsupportedMediaType(errorMessage));
        }
    }
}