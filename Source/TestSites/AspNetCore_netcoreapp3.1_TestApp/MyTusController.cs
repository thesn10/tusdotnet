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
    [TusInheritCapabilities(typeof(TusDiskStore))]
    //[TusEnableExtension("creation", "termination")]
    public class MyTusController : TusControllerBase
    {
        private readonly ILogger<MyTusController> _logger;
        private readonly TusStorageService _storage;

        public MyTusController(TusStorageService storage, ILogger<MyTusController> logger)
        {
            _storage = storage;
            _logger = logger;
        }

        [Authorize(Policy = "create-file-policy")]
        public override async Task<ITusCreateActionResult> Create(CreateContext context, CancellationToken cancellation)
        {
            var errors = ValidateMetadata(context.Metadata);

            if (errors.Count > 0)
            {
                return Fail(errors[0]);
            }

            var createResult = await _storage.Create(context, new CreateOptions()
            {

                Expiration = new AbsoluteExpiration(TimeSpan.FromMinutes(10)),
                Store = new TusDiskStore(Constants.FileDirectory),

                // Different directory per user example:
                //Store = new TusDiskStore(@"C:\tusfiles\" + User.Identity.Name + @"\")

            }, cancellation);

            _logger.LogInformation($"File created with id {createResult.FileId}");

            return CreateOk(createResult.FileId);
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

        public override async Task<ITusWriteActionResult> Write(WriteContext context, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Started writing file {context.FileId} at offset {context.UploadOffset}");

            var writeResult = await _storage.Write(context, new WriteOptions()
            {
                Expiration = new AbsoluteExpiration(TimeSpan.FromMinutes(Constants.FileExpirationInMinutes)),
                Store = new TusDiskStore(Constants.FileDirectory),

            }, cancellationToken);

            _logger.LogInformation($"Done writing file {context.FileId}. New offset: {context.UploadOffset}");

            return WriteOk(writeResult);
        }

        public override Task<ITusCompletedActionResult> FileCompleted(FileCompletedContext context, CancellationToken cancellation)
        {
            _logger.LogInformation($"Upload of file {context.FileId} is complete!");
            return base.FileCompleted(context, cancellation);
        }

        public override async Task<ITusInfoActionResult> GetFileInfo(GetFileInfoContext context, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Getting file info of file {context.FileId}");

            var info = await _storage.GetFileInfo(context, new GetFileInfoOptions()
            {
                Store = new TusDiskStore(Constants.FileDirectory),

            }, cancellationToken);

            return FileInfoOk(info);
        }
    }
}
