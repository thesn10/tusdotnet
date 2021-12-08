using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.Controllers;
using tusdotnet.Routing;

namespace tusdotnet.RequestHandlers.Validation
{
    internal sealed class UploadLengthForCreateFileAndConcatenateFiles : RequestRequirement
    {
        private readonly long? _maxUploadLength;

        public UploadLengthForCreateFileAndConcatenateFiles(long? maxUploadLength)
        {
            _maxUploadLength = maxUploadLength;
        }

        public override Task<ITusActionResult> Validate(FeatureSupportContext extensionInfo, HttpContext context)
        {
            var hasUploadDeferLengthHeader = context.Request.Headers.TryGetValue(HeaderConstants.UploadDeferLength, out var uploadDeferLengthHeader);
            var hasUploadLengthHeader = context.Request.Headers.TryGetValue(HeaderConstants.UploadLength, out var uploadLengthHeader);

            if (hasUploadLengthHeader && hasUploadDeferLengthHeader)
            {
                return BadRequestTask($"Headers {HeaderConstants.UploadLength} and {HeaderConstants.UploadDeferLength} are mutually exclusive and cannot be used in the same request");
            }

            if (!hasUploadDeferLengthHeader)
            {
                return VerifyRequestUploadLength(uploadLengthHeader.ToString());
            }

            var deferLengthStore = extensionInfo.SupportedExtensions.CreationDeferLength;
            return VerifyDeferLength(deferLengthStore, uploadDeferLengthHeader.ToString());
        }

        private Task<ITusActionResult> VerifyDeferLength(bool isDeferLengthStore, string uploadDeferLengthHeader)
        {
            if (!isDeferLengthStore)
            {
                return BadRequestTask($"Header {HeaderConstants.UploadDeferLength} is not supported");
            }

            if (uploadDeferLengthHeader != "1")
            {
                return BadRequestTask($"Header {HeaderConstants.UploadDeferLength} must have the value '1' or be omitted");
            }

            return OkTask();
        }

        private Task<ITusActionResult> VerifyRequestUploadLength(string uploadLengthHeader)
        {
            if (string.IsNullOrWhiteSpace(uploadLengthHeader))
            {
                return BadRequestTask($"Missing {HeaderConstants.UploadLength} header");
            }

            if (!long.TryParse(uploadLengthHeader, out long uploadLength))
            {
                return BadRequestTask($"Could not parse {HeaderConstants.UploadLength}");
            }

            if (uploadLength < 0)
            {
                return BadRequestTask($"Header {HeaderConstants.UploadLength} must be a positive number");
            }

            if (_maxUploadLength.HasValue && uploadLength > _maxUploadLength.Value)
            {
                return RequestEntityTooLargeTask(
                    $"Header {HeaderConstants.UploadLength} exceeds the server's max file size.");
            }

            return OkTask();
        }
    }
}