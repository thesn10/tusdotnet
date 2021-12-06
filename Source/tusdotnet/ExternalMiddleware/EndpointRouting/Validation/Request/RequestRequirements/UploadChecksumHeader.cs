using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.Extensions;
using tusdotnet.Models;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.Validation.Requirements
{
    internal sealed class UploadChecksumHeader : RequestRequirement
    {
        private readonly Checksum _checksum;

        public UploadChecksumHeader() : this(null)
        {
        }

        public UploadChecksumHeader(Checksum checksum)
        {
            _checksum = checksum;
        }

        public override Task<ITusActionResult> Validate(TusExtensionInfo extensionInfo, HttpContext context)
        {
            if (!extensionInfo.SupportedExtensions.Checksum)
            {
                return OkTask();
            }

            if (_checksum != null && !_checksum.IsValid)
            {
                return BadRequestTask($"Could not parse {HeaderConstants.UploadChecksum} header");
            }

#if trailingheaders

            var hasDeclaredChecksumTrailer = context.Request.HasDeclaredTrailingUploadChecksumHeader();
            if (_checksum != null && hasDeclaredChecksumTrailer)
            {
                return BadRequestTask("Headers Upload-Checksum and trailing header Upload-Checksum are mutually exclusive and cannot be used in the same request");
            }

            if (hasDeclaredChecksumTrailer && !context.Request.SupportsTrailers())
            {
                return BadRequestTask("Trailing header Upload-Checksum has been specified but http request does not support trailing headers");
            }
#endif

            return OkTask();
        }
    }
}