using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.Controllers;
using tusdotnet.Exceptions;
using tusdotnet.Models;
using tusdotnet.RequestHandlers.Validation;
using tusdotnet.Routing;

namespace tusdotnet.RequestHandlers
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

        public override RequestRequirement[] Requires => new RequestRequirement[]
        {
            new UploadLengthForCreateFileAndConcatenateFiles(EndpointOptions.MaxAllowedUploadSizeInBytes),
            new UploadMetadata(metadata => _metadataFromRequirement = metadata, EndpointOptions.MetadataParsingStrategy)
        };

        internal CreateRequestHandler(TusContext context, TusControllerBase controller)
            : base(context, controller)
        {

        }

        public override async Task<ITusActionResult> Invoke()
        {
            var metadata = HttpContext.Request.Headers[HeaderConstants.UploadMetadata].FirstOrDefault();

            long uploadLength;
            if (HttpContext.Request.Headers.ContainsKey(HeaderConstants.UploadLength))
            {
                uploadLength = long.Parse(HttpContext.Request.Headers[HeaderConstants.UploadLength].First());
            }
            else
            {
                uploadLength = -1;
            }

            var createContext = new CreateContext
            {
                UploadMetadata = metadata,
                Metadata = _metadataFromRequirement,
                UploadLength = uploadLength,
                FileConcatenation = null,
            };

            ICreateResult createResult;
            try
            {
                createResult = await _controller.Create(createContext);
            }
            catch (TusException ex)
            {
                return new TusBaseResult(ex.StatusCode, ex.Message);
            }

            if (createResult is not TusCreateStatusResult createOk)
                return createResult;

            var isEmptyFile = createContext.UploadLength == 0;
            var hasData = HttpContext.Request.ContentType == "application/offset+octet-stream";

            if (isEmptyFile)
            {
                ISimpleResult completedResult;
                try
                {
                    completedResult = await _controller.FileCompleted(new() { FileId = createOk.FileId });
                }
                catch (TusException ex)
                {
                    return new TusBaseResult(ex.StatusCode, ex.Message);
                }

                if (!completedResult.IsSuccessResult)
                    return completedResult;

                return createOk;
            }
            else if (hasData)
            {
                // creation-with-upload
                var writeHandler = new WriteRequestHandler(_context, _controller, createOk.FileId, true);
                var writeValidator = new RequestValidator(writeHandler.Requires);

                var authorizeContext = new AuthorizeContext()
                {
                    IntentType = IntentType.WriteFile,
                    Controller = _controller,
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
                else if (writeResult is TusBaseResult statusCodeResult && statusCodeResult.Exception is TusIncompleteWriteException incompleteWriteEx)
                {
                    createOk.UploadOffset = incompleteWriteEx.UploadOffset;
                }
                else if (!writeResult.IsSuccessResult)
                {
                    createOk.UploadOffset = 0;
                }
            }

            return createOk;
        }
    }
}
