using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tusdotnet.Constants;
using tusdotnet.Controllers;
using tusdotnet.Exceptions;
using tusdotnet.RequestHandlers.Validation;
using tusdotnet.Routing;
using tusdotnet.Tus2;

namespace tusdotnet.RequestHandlers.Tus2
{
    /*
     If an upload is interrupted, the client MAY attempt to fetch the offset of the incomplete upload by sending a HEAD request to the server with the same Upload-Token header field ({{upload-token}}). The client MUST NOT initiate this procedure without the knowledge of server support.
     The request MUST use the HEAD method and include the Upload-Token header. The request MUST NOT include the Upload-Offset header or the Upload-Incomplete header. The server MUST reject the request with the Upload-Offset header or the Upload-Incomplete header by sending a 400 (Bad Request) response.
     If the server considers the upload associated with this token active, it MUST send back a 204 (No Content) response. The response MUST include the Upload-Offset header set to the current resumption offset for the client. The response MUST include the Upload-Incomplete header which is set to true if and only if the upload is incomplete. An upload is considered complete if and only if the server completely and succesfully received a corresponding Upload Transfer Procedure ({{upload-transfer}}) request with the Upload-Incomplete header being omitted or set to false.
     The client MUST NOT perform the Offset Retrieving Procedure ({{offset-retrieving}}) while the Upload Transfer Procedures ({{upload-transfer}}) is in progress.
     The offset MUST be accepted by a subsequent Upload Transfer Procedure ({{upload-transfer}}). Due to network delay and reordering, the server might still be receiving data from an ongoing transfer for the same token, which in the client perspective has failed. The server MAY terminate any transfers for the same token before sending the response by abruptly terminating the HTTP connection or stream. Alternatively, the server MAY keep the ongoing transfer alive but ignore further bytes received past the offset.
     The client MUST NOT start more than one Upload Transfer Procedures ({{upload-transfer}}) based on the resumption offset from a single Offset Retrieving Procedure ({{offset-retrieving}}).
     The response SHOULD include Cache-Control: no-store header to prevent HTTP caching.
     If the server does not consider the upload associated with this token active, it MUST respond with 404 (Not Found) status code.
     The client MAY automatically start uploading from the beginning using Upload Transfer Procedure ({{upload-transfer}}) if 404 (Not Found) status code is received. The client SHOULD NOT automatically retry if a status code other than 204 and 404 is received.
     * */

    /*Upload Creation Procedure {#upload-creation}
The Upload Creation Procedure is intended for starting a new upload. A limited form of this procedure MAY be used by the client without the knowledge of server support of the Resumable Uploads Protocol.

This procedure is designed to be compatible with a regular upload. Therefore all methods are allowed with the exception of GET, HEAD, DELETE, and OPTIONS. All response status codes are allowed. The client is RECOMMENDED to use the POST method if not otherwise intended. The server MAY only support a limited number of methods.

The request MUST include the Upload-Token header field ({{upload-token}}) which uniquely identifies an upload. The client MUST NOT reuse the token for a different upload. The request MUST NOT include the Upload-Offset header.

If the end of the request body is not the end of the upload, the Upload-Incomplete header field ({{upload-incomplete}}) MUST be set to true.

If the server already has an active upload with the same token in the Upload-Token header field, it MUST respond with 409 (Conflict) status code.

The server MUST send the Upload-Offset header in the response if it considers the upload active, either when the response is a success (e.g. 201 (Created)), or when the response is a failure (e.g. 409 (Conflict)). The value MUST be equal to the end offset of the entire upload, or the begin offset of the next chunk if the upload is still incomplete. The client SHOULD consider the upload failed if the response status code indicates a success but the offset in the Upload-Offset header field in the response does not equal to the begin offset plus the number of bytes uploaded in the request.

If the request completes successfully and the entire upload is complete, the server MUST acknowledge it by responding with a successful status code between 200 and 299 (inclusive). Server is RECOMMENDED to use 201 (Created) response if not otherwise specified. The response MUST NOT include the Upload-Incomplete header with the value of true.

If the request completes successfully but the entire upload is not yet complete indicated by the Upload-Incomplete header, the server MUST acknowledge it by responding with the 201 (Created) status code, the Upload-Incomplete header set to true.

:method: POST
:scheme: https
:authority: example.com
:path: /upload
upload-token: :SGVs…SGU=:
upload-draft-interop-version: 2
content-length: 100
[content (100 bytes)]

:status: 104
upload-draft-interop-version: 2

:status: 201
upload-offset: 100
:method: POST
:scheme: https
:authority: example.com
:path: /upload
upload-token: :SGVs…SGU=:
upload-draft-interop-version: 2
upload-incomplete: ?1
content-length: 25
[partial content (25 bytes)]

:status: 201
upload-incomplete: ?1
upload-offset: 25
The client MAY automatically attempt upload resumption when the connection is terminated unexpectedly, or if a server error status code between 500 and 599 (inclusive) is received. The client SHOULD NOT automatically retry if a client error status code between 400 and 499 (inclusive) is received.

File metadata can affect how servers might act on the uploaded file. Clients can send Representation Metadata (see {{Section 8.3 of HTTP}}) in the Upload Creation Procedure request that starts an upload. Servers MAY interpret this metadata or MAY ignore it. The Content-Type header can be used to indicate the MIME type of the file. The Content-Disposition header can be used to transmit a filename. If included, the parameters SHOULD be either filename, filename* or boundary.*/

    internal class RetrieveOffsetRequestHandler : RequestHandlerV2
    {
        public override RequestRequirement[] Requires => new RequestRequirement[] { };

        internal RetrieveOffsetRequestHandler(TusContext context, Tus2ControllerBase controller)
            : base(context, controller)
        {

        }

        public override async Task<ITusActionResult> Invoke()
        {
            //var headerParser = HttpContext.RequestServices.GetRequiredService<IHeaderParser>();
            var uploadManager = HttpContext.RequestServices.GetRequiredService<IOngoingUploadManager>();

            //var headers = headerParser.Parse(HttpContext);

            //Tus2Validator.AssertNoInvalidHeaders(headers);

            //await uploadManager.CancelOtherUploads(headers.UploadToken);

            //await Tus2Validator.AssertFileExist(storageFacade.Storage, context.Headers.UploadToken);

            ITus2RetrieveOffsetResult response;
            try
            {
                response = await _controller.RetrieveOffset(new() { Headers = null /*headers*/ });
            }
            catch (TusException ex)
            {
                return new Tus2BaseResult(ex.StatusCode, ex.Message);
            }

            return response;
        }
    }
}
