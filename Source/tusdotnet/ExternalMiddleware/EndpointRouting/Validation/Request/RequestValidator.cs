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
            ITusActionResult result = new TusOkResult();

            foreach (var spec in _requirements)
            {
                if (spec == null) continue;

                result = await spec.Validate(context.ExtensionInfo, context.HttpContext);

                if (!result.IsSuccessResult)
                {
                    break;
                }
            }

            return result;
        }
    }
}