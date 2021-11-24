#if endpointrouting
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.Validation
{
    internal sealed class RequestValidator
    {
        private readonly RequestRequirement[] _requirements;

        public RequestValidator(params RequestRequirement[] requirements)
        {
            _requirements = requirements ?? new RequestRequirement[0];
        }

        public async Task<ITusActionResult> Validate(TusContext context)
        {
            HttpStatusCode statusCode = HttpStatusCode.OK;
            string errorMessage = null;

            foreach (var spec in _requirements)
            {
                var (status, error) = await spec.Validate(context.ExtensionInfo, context.HttpContext);

                if (status == HttpStatusCode.OK)
                {
                    continue;
                }

                statusCode = status;
                errorMessage = error;
                break;
            }

            return new TusStatusCodeResult(statusCode, errorMessage);
        }
    }
}
#endif