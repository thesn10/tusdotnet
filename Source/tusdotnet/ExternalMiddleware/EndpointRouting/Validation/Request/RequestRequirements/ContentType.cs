﻿using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading.Tasks;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.Validation.Requirements
{
    internal sealed class ContentType : RequestRequirement
    {
        public override Task<ITusActionResult> Validate(TusExtensionInfo extensionInfo, HttpContext context)
        {
            if (context.Request.ContentType?.Equals("application/offset+octet-stream", StringComparison.OrdinalIgnoreCase) != true)
            {
                var errorMessage = $"Content-Type {context.Request.ContentType} is invalid. Must be application/offset+octet-stream";
                return UnsupportedMediaTypeTask(errorMessage);
            }

            return OkTask();
        }
    }
}