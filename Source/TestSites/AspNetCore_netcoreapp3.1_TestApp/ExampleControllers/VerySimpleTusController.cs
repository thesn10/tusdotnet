using System.Threading.Tasks;
using tusdotnet.ExternalMiddleware.EndpointRouting;

namespace AspNetCore_netcoreapp3._1_TestApp
{
    [TusController]
    public class VerySimpleTusController : TusControllerBase
    {
        public override async Task<ICompletedResult> FileCompleted(FileCompletedContext context)
        {
            await StorageClient.Delete(context.FileId, HttpContext.RequestAborted);

            return BadRequest("Upload failed successfully. Please try again :)");
        }
    }
}
