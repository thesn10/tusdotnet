using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;
using tusdotnet.Models.Concatenation;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.Validation.Requirements
{
    internal sealed class UploadConcatForConcatenateFiles : RequestRequirement
    {
        private readonly UploadConcat _uploadConcat;

        public UploadConcatForConcatenateFiles(UploadConcat uploadConcat)
        {
            _uploadConcat = uploadConcat;
        }

        public override Task<(HttpStatusCode status, string error)> Validate(TusExtensionInfo extensionInfo, HttpContext context)
        {
            if (!_uploadConcat.IsValid)
            {
                return BadRequestTask(_uploadConcat.ErrorMessage);
            }

            return OkTask();
        }
    }
}