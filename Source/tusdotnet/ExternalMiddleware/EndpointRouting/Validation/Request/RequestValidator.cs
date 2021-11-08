#if endpointrouting
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.Validation
{
    internal sealed class RequestValidator
    {
        public HttpStatusCode StatusCode { get; private set; }
        public string ErrorMessage { get; private set; }

        private readonly TusExtensionInfo _extensionInfo;
        private readonly RequestRequirement[] _requirements;

        public RequestValidator(TusExtensionInfo extensionInfo, params RequestRequirement[] requirements)
        {
            _extensionInfo = extensionInfo;
            _requirements = requirements ?? new RequestRequirement[0];
        }

        public async Task<bool> Validate(HttpContext context)
        {
            StatusCode = HttpStatusCode.OK;
            ErrorMessage = null;

            foreach (var spec in _requirements)
            {
                var (status, error) = await spec.Validate(_extensionInfo, context);

                if (status == HttpStatusCode.OK)
                {
                    continue;
                }

                StatusCode = status;
                ErrorMessage = error;
                break;
            }

            return StatusCode == HttpStatusCode.OK;
        }
    }
}
#endif