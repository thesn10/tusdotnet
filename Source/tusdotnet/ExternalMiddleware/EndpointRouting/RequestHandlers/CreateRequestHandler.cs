#if endpointrouting
using Microsoft.AspNetCore.Http;
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
            new UploadLengthForCreateFileAndConcatenateFiles(Options.MaxAllowedUploadSizeInBytes),
            new UploadMetadata(metadata => _metadataFromRequirement = metadata, Options.MetadataParsingStrategy)
        };

        internal CreateRequestHandler(TusContext context, TusControllerBase controller)
            : base(context, controller)
        {

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
                uploadLength = -1;
            }

            var createContext = new CreateContext
            {
                UploadMetadata = metadata,
                Metadata = _metadataFromRequirement,
                UploadLength = uploadLength,
                FileConcat = null,
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

            var isEmptyFile = createContext.UploadLength == 0;

            if (isEmptyFile)
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

                return createOk;
            }
            else
            {
                if (HttpContext.Request.Headers.ContentLength > 0)
                {
                    // creation-with-upload
                    var writeHandler = new WriteRequestHandler(_context, _controller, createOk.FileId);
                    var writeResult = await writeHandler.Invoke();

                    if (writeResult is TusWriteStatusResult writeOk)
                    {
                        createOk.Expires = writeOk.FileExpires;
                        createOk.UploadOffset = writeOk.UploadOffset;
                    }
                }

                return createOk;
            }
        }
    }
}
#endif
