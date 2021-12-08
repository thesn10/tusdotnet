using Microsoft.AspNetCore.Authorization;
using tusdotnet.Controllers;
using tusdotnet.Models;

namespace AspNetCore_net6._0_TestApp
{
    [TusController]
    [StorageProfile("my-storage")]
    [FileExpiration(5, ExpirationType.Absolute)]
    [UsePipelineWrite]
    public class MyTusController : TusControllerBase
    {
        private readonly ILogger<MyTusController> _logger;

        public MyTusController(ILogger<MyTusController> logger)
        {
            _logger = logger;
        }

        [Authorize(Policy = "create-file-policy")]
        public override async Task<ICreateResult> Create(CreateContext context)
        {
            var errors = ValidateMetadata(context.Metadata);

            if (errors.Count > 0)
            {
                return BadRequest(errors[0]);
            }

            // instead of calling the base, you could also use StorageClient.Create for more options
            var result = await base.Create(context);

            _logger.LogInformation($"File created");

            return result;
        }

        private List<string> ValidateMetadata(IDictionary<string, Metadata> metadata)
        {
            var errors = new List<string>();

            if (!metadata.ContainsKey("name") || metadata["name"].HasEmptyValue)
            {
                errors.Add("name metadata must be specified.");
            }

            if (!metadata.ContainsKey("contentType") || metadata["contentType"].HasEmptyValue)
            {
                errors.Add("contentType metadata must be specified.");
            }

            return errors;
        }

        public override Task<ISimpleResult> FileCompleted(FileCompletedContext context)
        {
            _logger.LogInformation($"Upload of file {context.FileId} is complete!");

            // If the store implements ITusReadableStore one could access the completed file here.
            // The default TusDiskStore implements this interface:
            //var file = await StorageClient.Get(context.FileId, HttpContext.RequestAborted);

            return Task.FromResult<ISimpleResult>(Ok());
        }

        public override async Task<ISimpleResult> Delete(DeleteContext context)
        {
            _logger.LogInformation($"Deleting file {context.FileId}");

            await StorageClient.Delete(context, default, HttpContext.RequestAborted);

            // Can the file be deleted? If not call BadRequest(<message>);
            return Ok();
        }
    }
}
