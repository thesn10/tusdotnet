using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.Controllers;
using tusdotnet.Routing;

namespace tusdotnet.RequestHandlers.Validation
{
    internal sealed class UploadOffset : RequestRequirement
    {
        public override Task<ITusActionResult> Validate(FeatureSupportContext extensionInfo, HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey(HeaderConstants.UploadOffset))
            {
                return BadRequestTask($"Missing {HeaderConstants.UploadOffset} header");
            }

            if (!long.TryParse(context.Request.Headers[HeaderConstants.UploadOffset].FirstOrDefault(), out long requestOffset))
            {
                return BadRequestTask($"Could not parse {HeaderConstants.UploadOffset} header");
            }

            if (requestOffset < 0)
            {
                return BadRequestTask($"Header {HeaderConstants.UploadOffset} must be a positive number");
            }

            return OkTask();
        }
    }
}