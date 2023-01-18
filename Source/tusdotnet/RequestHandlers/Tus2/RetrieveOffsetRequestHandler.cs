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

    internal class RetrieveOffsetRequestHandler : RequestHandler
    {
        internal override RequestRequirement[] Requires => new RequestRequirement[] { };

        internal GetOptionsRequestHandler(TusContext context, TusControllerBase controller)
            : base(context, controller)
        {

        }

        internal override async Task<ITusActionResult> Invoke()
        {
            var headerParser = HttpContext.RequestServices.GetRequiredService<IHeaderParser>();

            var headers = headerParser.Parse(HttpContext);

            //Tus2Validator.AssertNoInvalidHeaders(context.Headers);

            await uploadManager.CancelOtherUploads(headers.UploadToken);

            //await Tus2Validator.AssertFileExist(storageFacade.Storage, context.Headers.UploadToken);

            ISimpleResult retrieveOffsetResult;
            try
            {
                retrieveOffsetResult = await _controller.RetrieveOffset(new() { Headers = headers });
            }
            catch (TusException ex)
            {
                // TODO: try get offset

                return new TusStatusCodeResult(ex.StatusCode, ex.Message);
            }

            if (retrieveOffsetResult is not TusCreateStatusResult createOk)
                return retrieveOffsetResult;

            return new TusOkResult();
        }
    }
}
