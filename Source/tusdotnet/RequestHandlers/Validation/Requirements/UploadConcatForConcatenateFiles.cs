using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using tusdotnet.Controllers;
using tusdotnet.Models.Concatenation;
using tusdotnet.Routing;

namespace tusdotnet.RequestHandlers.Validation
{
    internal sealed class UploadConcatForConcatenateFiles : RequestRequirement
    {
        private readonly UploadConcat _uploadConcat;

        public UploadConcatForConcatenateFiles(UploadConcat uploadConcat)
        {
            _uploadConcat = uploadConcat;
        }

        public override Task<ITusActionResult> Validate(FeatureSupportContext extensionInfo, HttpContext context)
        {
            if (!_uploadConcat.IsValid)
            {
                return BadRequestTask(_uploadConcat.ErrorMessage);
            }

            return OkTask();
        }
    }
}