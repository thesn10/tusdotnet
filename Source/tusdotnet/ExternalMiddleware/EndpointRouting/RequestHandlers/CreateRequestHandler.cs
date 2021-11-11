#if endpointrouting
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.ExternalMiddleware.EndpointRouting.Validation;
using tusdotnet.ExternalMiddleware.EndpointRouting.Validation.Requirements;
using tusdotnet.Models;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers
{
    /*
    * The Client MUST send a POST request against a known upload creation URL to request a new upload resource. 
    * The request MUST include one of the following headers:
    * a) Upload-Length to indicate the size of an entire upload in bytes.
    * b) Upload-Defer-Length: 1 if upload size is not known at the time. 
    * Once it is known the Client MUST set the Upload-Length header in the next PATCH request. 
    * Once set the length MUST NOT be changed. As long as the length of the upload is not known, t
    * he Server MUST set Upload-Defer-Length: 1 in all responses to HEAD requests.
    * If the Server supports deferring length, it MUST add creation-defer-length to the Tus-Extension header.
    * The Client MAY supply the Upload-Metadata header to add additional metadata to the upload creation request. 
    * The Server MAY decide to ignore or use this information to further process the request or to reject it. 
    * If an upload contains additional metadata, responses to HEAD requests MUST include the Upload-Metadata 
    * header and its value as specified by the Client during the creation.
    * If the length of the upload exceeds the maximum, which MAY be specified using the Tus-Max-Size header, 
    * the Server MUST respond with the 413 Request Entity Too Large status.
    * The Server MUST acknowledge a successful upload creation with the 201 Created status. 
    * The Server MUST set the Location header to the URL of the created resource. This URL MAY be absolute or relative.
    * The Client MUST perform the actual upload using the core protocol.
    * 
    * If the expiration is known at the creation, the Upload-Expires header MUST be included in the response to the initial POST request. 
    * 
    * The Client MAY include parts of the upload in the initial Creation request using the Creation With Upload extension.
    */

    internal class CreateRequestHandler : RequestHandler
    {
        private Dictionary<string, Metadata> _metadataFromRequirement;

        internal override RequestRequirement[] Requires => new RequestRequirement[] 
        {
            new UploadLengthForCreateFileAndConcatenateFiles(_options.MaxAllowedUploadSizeInBytes),
            new UploadMetadata(metadata => _metadataFromRequirement = metadata, _options.MetadataParsingStrategy)
        };

        internal CreateRequestHandler(HttpContext context, TusControllerBase controller, TusExtensionInfo extensionInfo, ITusEndpointOptions options)
            : base(context, controller, extensionInfo, options)
        {

        }

        internal override async Task<IActionResult> Invoke()
        {
            var authorizeContext = new AuthorizeContext()
            {
                IntentType = IntentType.CreateFile,
                ControllerMethod = ((Func<CreateContext, Task<ICreateResult>>)_controller.Create).Method,
            };

            var authorizeResult = await _controller.Authorize(authorizeContext);

            if (!authorizeResult.IsSuccessResult)
            {
                return authorizeResult.Translate();
            }

            var metadata = _context.Request.Headers[HeaderConstants.UploadMetadata].FirstOrDefault();

            long uploadLength;
            if (_context.Request.Headers.ContainsKey(HeaderConstants.UploadLength))
            {
                uploadLength = long.Parse(_context.Request.Headers[HeaderConstants.UploadLength].First());
            }
            else
            {
                uploadLength = 0;
            }

            var createContext = new CreateContext
            {
                UploadMetadata = metadata,
                Metadata = _metadataFromRequirement,
                UploadLength = uploadLength,
            };

            ICreateResult createResult;
            try
            {
                createResult = await _controller.Create(createContext);
            }
            catch (TusException ex)
            {
                return new ObjectResult(ex.Message)
                {
                    StatusCode = (int)ex.StatusCode
                };
            }

            if (createResult is TusBadRequestResult fail) return fail.Translate();
            if (createResult is TusForbiddenResult forbid) return forbid.Translate();

            var createOk = createResult as TusCreateStatusResult;

            if (createOk == null)
            {
                throw new InvalidOperationException($"Unknown action result: {createResult.GetType().FullName}");
            }

            var isEmptyFile = createContext.UploadLength == 0;

            if (isEmptyFile)
            {
                SetTusResumableHeader();
                var completedResult = await _controller.FileCompleted(new() { FileId = createOk.FileId });

                if (completedResult is TusBadRequestResult completeFail)
                    return new BadRequestObjectResult(completeFail.Message);

                var createdResult = new CreatedResult($"{_context.Request.GetDisplayUrl().TrimEnd('/')}/{createOk.FileId}", null);

                SetCreateHeaders(createOk.Expires, null);
                return createdResult;
            }
            else
            {
                if (_context.Request.Headers.ContentLength > 0)
                {
                    // creation-with-upload
                    var writeHandler = new WriteRequestHandler(_context, _controller, _extensionInfo, _options, createOk.FileId);
                    await writeHandler.Invoke();
                }

                var createdResult = new CreatedResult($"{_context.Request.GetDisplayUrl().TrimEnd('/')}/{createOk.FileId}", null);

                return createdResult;
            }
        }
    }
}
#endif
