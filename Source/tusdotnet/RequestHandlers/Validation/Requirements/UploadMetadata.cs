using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.Controllers;
using tusdotnet.Models;
using tusdotnet.Parsers;
using tusdotnet.Routing;

namespace tusdotnet.RequestHandlers.Validation
{
    internal sealed class UploadMetadata : RequestRequirement
    {
        private readonly Action<Dictionary<string, Metadata>> _cacheResult;
        private readonly MetadataParsingStrategy _metadataParsingStrategy;

        public UploadMetadata(Action<Dictionary<string, Metadata>> cacheResult, MetadataParsingStrategy metadataParsingStrategy)
        {
            _cacheResult = cacheResult;
            _metadataParsingStrategy = metadataParsingStrategy;
        }

        public override Task<ITusActionResult> Validate(FeatureSupportContext extensionInfo, HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey(HeaderConstants.UploadMetadata))
            {
                _cacheResult?.Invoke(new Dictionary<string, Metadata>());
                return OkTask();
            }

            var metadataParserResult = MetadataParser.ParseAndValidate(
                _metadataParsingStrategy,
                context.Request.Headers[HeaderConstants.UploadMetadata]);

            if (metadataParserResult.Success)
            {
                _cacheResult?.Invoke(metadataParserResult.Metadata);
                return OkTask();
            }

            return BadRequestTask(metadataParserResult.ErrorMessage);
        }
    }
}