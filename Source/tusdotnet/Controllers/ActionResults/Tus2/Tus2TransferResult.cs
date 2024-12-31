using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using tusdotnet.Routing;
using tusdotnet.Tus2;

namespace tusdotnet.Controllers.ActionResults
{
    public class Tus2TransferResult : ITus2WriteResult
    {
        public Tus2TransferResult(long? uploadOffset, bool uploadIncomplete, bool disconnectClient)
        {
            UploadOffset = uploadOffset;
            UploadIncomplete = uploadIncomplete;
            
            //TODO remove because should be handled through OperationCancelledException
            DisconnectClient = disconnectClient;
        }


        /// <inheritdoc />
        public bool IsSuccessResult => true;
        
        public long? UploadOffset { get; set; }
        public bool UploadIncomplete { get; set; }
        public bool DisconnectClient { get; set; }

        /// <inheritdoc />
        public Task Execute(TusContext context)
        {
            var statusCode = HttpStatusCode.OK; // TODO check if correct?

            var result = new Tus2BaseResult(statusCode)
            {
                NoCache = true,
                UploadOffset = UploadOffset,
                UploadIncomplete = UploadIncomplete,
                DisconnectClient = DisconnectClient,
            };

            return result.Execute(context);
        }
    }
}
