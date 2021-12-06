using Microsoft.AspNetCore.Http;
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

        internal ConcatenateRequestHandler(TusContext context, TusControllerBase controller)
            : base(context, controller)
        {
            _uploadConcat = ParseUploadConcatHeader();
            _isPartialFile = _uploadConcat.Type is FileConcatPartial;
        }

        internal override async Task<ITusActionResult> Invoke()
        {
            var metadata = HttpContext.Request.Headers[HeaderConstants.UploadMetadata].FirstOrDefault();

            long uploadLength;
            if (HttpContext.Request.Headers.ContainsKey(HeaderConstants.UploadLength))
            {
                uploadLength = long.Parse(HttpContext.Request.Headers[HeaderConstants.UploadLength].First());
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
                FileConcatenation = _uploadConcat.Type,
            };

            ICreateResult createResult;
            try
            {
                createResult = await _controller.Create(createContext);
            }
            catch (TusException ex)
            {
                return new TusStatusCodeResult(ex.StatusCode, ex.Message);
            }

            if (createResult is not TusCreateStatusResult createOk)
                return createResult;

            if (_isPartialFile)
            {
                var isEmptyFile = createContext.UploadLength == 0;
                if (isEmptyFile)
                {
                    createOk.UploadOffset = 0;
                    return createOk;
                }

                var hasData = HttpContext.Request.ContentType == "application/offset+octet-stream";
                if (hasData)
                {
                    // creation-with-upload
                    var writeHandler = new WriteRequestHandler(_context, _controller, createOk.FileId, true);
                    var writeValidator = new RequestValidator(writeHandler.Requires);

                    var authorizeContext = new AuthorizeContext()
                    {
                        IntentType = IntentType.WriteFile,
                        ControllerMethod = AuthorizeContext.GetControllerActionMethodInfo(IntentType.WriteFile, _controller),
                        FileId = createOk.FileId,
                        RequestHandler = this,
                    };

                    var authorizeResult = await _controller.Authorize(authorizeContext);

                    if (!authorizeResult.IsSuccessResult)
                    {
                        createOk.UploadOffset = 0;
                        return createOk;
                    }

                    var validateResult = await writeValidator.Validate(_context);

                    if (!validateResult.IsSuccessResult)
                    {
                        createOk.UploadOffset = 0;
                        return createOk;
                    }

                    var writeResult = await writeHandler.Invoke();

                    if (writeResult is TusWriteStatusResult writeOk)
                    {
                        createOk.FileExpires = writeOk.FileExpires;
                        createOk.UploadOffset = writeOk.UploadOffset;
                    }
                    else if (writeResult is TusStatusCodeResult statusCodeResult && statusCodeResult.Exception is TusIncompleteWriteException incompleteWriteEx)
                    {
                        createOk.UploadOffset = incompleteWriteEx.UploadOffset;
                    }
                    else if (!writeResult.IsSuccessResult)
                    {
                        createOk.UploadOffset = 0;
                    }
                }
            }
            else
            {
                ISimpleResult completedResult;
                try
                {
                    completedResult = await _controller.FileCompleted(new() { FileId = createOk.FileId });
                }
                catch (TusException ex)
                {
                    return new TusStatusCodeResult(ex.StatusCode, ex.Message);
                }

                if (!completedResult.IsSuccessResult)
                    return completedResult;
            }

            return createOk;

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
                requirements.Add(new UploadLengthForCreateFileAndConcatenateFiles(Options.MaxAllowedUploadSizeInBytes));
            }

            requirements.Add(new UploadMetadata(metadata => _metadataFromRequirement = metadata, Options.MetadataParsingStrategy));

            return requirements.ToArray();
        }

        private UploadConcat ParseUploadConcatHeader()
        {
            string uploadConcat = HttpContext.Request.Headers[HeaderConstants.UploadConcat].FirstOrDefault();

            return new UploadConcat(uploadConcat, RoutingHelper);
        }
    }
}
