using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.ExternalMiddleware.EndpointRouting;
using tusdotnet.Models;
using tusdotnet.Models.Expiration;
using tusdotnet.Stores;

namespace AspNetCore_netcoreapp3._1_TestApp
{
    [TusController]
    [TusStorageProfile("my-storage")]
    [TusFileExpiration(5, false)]
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
            return base.FileCompleted(context);
        }

        public override async Task<IWriteResult> Write(WriteContext context)
        {
            _logger.LogInformation($"Started writing file {context.FileId} at offset {context.UploadOffset}");
            var result = await base.Write(context);
            _logger.LogInformation($"Done writing file {context.FileId}. New offset: {(result as TusWriteStatusResult)?.UploadOffset}");
            return result;
        }

        public override Task<IFileInfoResult> GetFileInfo(GetFileInfoContext context)
        {
            _logger.LogInformation($"Getting file info of file {context.FileId}");
            return base.GetFileInfo(context);
        }

        public override Task<ISimpleResult> Delete(DeleteContext context)
        {
            _logger.LogInformation($"Deleting file {context.FileId}");
            return base.Delete(context);
        }
    }
}
