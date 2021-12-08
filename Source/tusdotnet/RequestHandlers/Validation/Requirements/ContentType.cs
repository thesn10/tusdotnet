using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using tusdotnet.Controllers;
using tusdotnet.Routing;

namespace tusdotnet.RequestHandlers.Validation
{
    internal sealed class ContentType : RequestRequirement
    {
        public override Task<ITusActionResult> Validate(FeatureSupportContext extensionInfo, HttpContext context)
        {
            if (context.Request.ContentType?.Equals("application/offset+octet-stream", StringComparison.OrdinalIgnoreCase) != true)
            {
                var errorMessage = $"Content-Type {context.Request.ContentType} is invalid. Must be application/offset+octet-stream";
                return UnsupportedMediaTypeTask(errorMessage);
            }

            return OkTask();
        }
    }
}