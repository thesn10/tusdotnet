using Microsoft.AspNetCore.Http;
using tusdotnet.Routing;
using System.Threading.Tasks;
using tusdotnet.Storage.Results.Tus2;

namespace tusdotnet.Tus2
{
    public class UploadTransferProcedureResponse : Tus2BaseResponse
    {
        public bool UploadIncomplete { get; set; }

        public UploadTransferProcedureResponse(WriteResult writeResult)
        {
            UploadIncomplete = writeResult.UploadIncomplete;
            UploadOffset = writeResult.UploadOffset;
            //TODO remove because should be handled through OperationCancelledException
            DisconnectClient = writeResult.DisconnectClient;
        }

        protected override Task Execute(TusContext context)
        {
            if (UploadIncomplete)
            {
                context.HttpContext.SetHeader("Upload-Incomplete", UploadIncomplete.ToSfBool());
            }

            return Task.CompletedTask;
        }
    }
}
