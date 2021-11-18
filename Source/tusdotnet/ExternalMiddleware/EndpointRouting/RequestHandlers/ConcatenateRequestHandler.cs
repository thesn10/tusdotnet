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
using tusdotnet.Models.Concatenation;

namespace tusdotnet.ExternalMiddleware.EndpointRouting.RequestHandlers
{
    /*
    * This extension can be used to concatenate multiple uploads into a single one enabling Clients to perform parallel uploads and 
    * to upload non-contiguous chunks. If the Server supports this extension, it MUST add concatenation to the Tus-Extension header.
    * A partial upload represents a chunk of a file. It is constructed by including the Upload-Concat: partial header 
    * while creating a new upload using the Creation extension. Multiple partial uploads are concatenated into a 
    * final upload in the specified order. The Server SHOULD NOT process these partial uploads until they are 
    * concatenated to form a final upload. The length of the final upload MUST be the sum of the length of all partial uploads.
    * In order to create a new final upload the Client MUST add the Upload-Concat header to the upload creation request. 
    * The value MUST be final followed by a semicolon and a space-separated list of the partial upload URLs that need to be concatenated. 
    * The partial uploads MUST be concatenated as per the order specified in the list. 
    * This concatenation request SHOULD happen after all of the corresponding partial uploads are completed.
    * The Client MUST NOT include the Upload-Length header in the final upload creation.
    * The Client MAY send the concatenation request while the partial uploads are still in progress.
    * This feature MUST be explicitly announced by the Server by adding concatenation-unfinished to the Tus-Extension header.
    * When creating a new final upload the partial uploads’ metadata SHALL NOT be transferred to the new final upload.
    * All metadata SHOULD be included in the concatenation request using the Upload-Metadata header.
    * The Server MAY delete partial uploads after concatenation. They MAY however be used multiple times to form a final resource. 
    * 
    * If the expiration is known at the creation, the Upload-Expires header MUST be included in the response to the initial POST request. 
    * 
    * The Client MAY include parts of the upload in the initial Creation request using the Creation With Upload extension.
    *   NOTE: The above is only applicable for partial files as final files cannot be patched.
     */

    internal class ConcatenateRequestHandler : RequestHandler
    {
        private Dictionary<string, Metadata> _metadataFromRequirement;

        internal override RequestRequirement[] Requires => BuildListOfRequirements();
        public UploadConcat UploadConcat => _uploadConcat;

        private readonly UploadConcat _uploadConcat;

        private readonly bool _isPartialFile;

        internal ConcatenateRequestHandler(HttpContext context, TusControllerBase controller, TusExtensionInfo extensionInfo, ITusEndpointOptions options)
            : base(context, controller, extensionInfo, options)
        {
            _uploadConcat = ParseUploadConcatHeader();
            _isPartialFile = _uploadConcat.Type is FileConcatPartial;
        }

        internal override async Task<IActionResult> Invoke()
        {
            var authorizeContext = new AuthorizeContext()
            {
                IntentType = IntentType.ConcatenateFiles,
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
                FileConcat = _uploadConcat.Type,
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

            if (_isPartialFile && _context.Request.Headers.ContentLength > 0)
            {
                // creation-with-upload
                var writeHandler = new WriteRequestHandler(_context, _controller, _extensionInfo, _options, createOk.FileId);
                await writeHandler.Invoke();
            }
            else
            {
                SetCreateHeaders(createOk.Expires, null);
            }

            return new CreatedResult($"{_context.Request.GetDisplayUrl().TrimEnd('/')}/{createOk.FileId}", null);

        }

        private RequestRequirement[] BuildListOfRequirements()
        {
            var requirements = new List<RequestRequirement>(3)
            {
                new UploadConcatForConcatenateFiles(_uploadConcat),
            };

            // Only validate upload length for partial files as the length of a final file is implicit.
            if (_isPartialFile)
            {
                requirements.Add(new UploadLengthForCreateFileAndConcatenateFiles(_options.MaxAllowedUploadSizeInBytes));
            }

            requirements.Add(new UploadMetadata(metadata => _metadataFromRequirement = metadata, _options.MetadataParsingStrategy));

            return requirements.ToArray();
        }

        private UploadConcat ParseUploadConcatHeader()
        {
            string uploadConcat = _context.Request.Headers[HeaderConstants.UploadConcat].FirstOrDefault();

            return new UploadConcat(uploadConcat, _context.Request.GetDisplayUrl());
        }
    }
}
#endif
