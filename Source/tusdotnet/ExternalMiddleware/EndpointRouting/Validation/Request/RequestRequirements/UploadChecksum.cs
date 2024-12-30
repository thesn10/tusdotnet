#if endpointrouting
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using tusdotnet.Adapters;
using tusdotnet.Constants;
using tusdotnet.Interfaces;
using tusdotnet.Models;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.Validation.Requirements
{
    internal sealed class UploadChecksum : RequestRequirement
    {
        private Checksum RequestChecksum { get; }

        public UploadChecksum() : this(null)
        {
        }

        public UploadChecksum(Checksum requestChecksum)
        {
            RequestChecksum = requestChecksum;
        }

        public override async Task<(HttpStatusCode status, string error)> Validate(TusExtensionInfo extensionInfo, HttpContext context)
        {
            var providedChecksum = RequestChecksum ?? GetProvidedChecksum(context);

            if (extensionInfo.SupportedExtensions.Checksum && providedChecksum != null)
            {
                if (!providedChecksum.IsValid)
                {
                    return BadRequest($"Could not parse {HeaderConstants.UploadChecksum} header");
                }

                /*var checksumAlgorithms = (await checksumStore.GetSupportedAlgorithmsAsync(context.RequestAborted)).ToList();
                if (!checksumAlgorithms.Contains(providedChecksum.Algorithm))
                {
                    return BadRequest(
                        $"Unsupported checksum algorithm. Supported algorithms are: {string.Join(",", checksumAlgorithms)}");
                }*/
            }
            return Ok();
        }

        private static Checksum GetProvidedChecksum(HttpContext context)
        {
            return context.Request.Headers.ContainsKey(HeaderConstants.UploadChecksum)
                ? new Checksum(context.Request.Headers[HeaderConstants.UploadChecksum][0])
                : null;
        }
    }
}
#endif